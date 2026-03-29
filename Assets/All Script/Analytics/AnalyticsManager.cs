using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(IAnalyticsService))]
public class AnalyticsManager : MonoBehaviour
{
    public static AnalyticsManager instance;
    private IAnalyticsService _analyticsService;

    // สำหรับ Round (เกมแต่ละรอบ)
    private float _roundStartTime;
    private int _playCount = 0;

    // สำหรับ App Session (ตั้งแต่เปิดแอป จนปิดแอป)
    private float _appSessionStartTime;

    private void Awake()
    {
        // ทำเป็น Singleton ข้าม Scene เพื่อจับเวลาตั้งแต่เข้าเกมจนออกเกม
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // สำคัญมาก: เพื่อไม่ให้ถูกทำลายตอนเปลี่ยน Scene
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // ดึง Service มาใช้งาน (Dependency Injection)
        _analyticsService = GetComponent<IAnalyticsService>();

        // เริ่มจับเวลาตั้งแต่เปิดแอป โดยใช้ realtime (ไม่สน Time.timeScale)
        _appSessionStartTime = Time.realtimeSinceStartup;
    }

    #region Round Tracking (ในด่าน)
    /// <summary>
    /// เรียกเมื่อผู้เล่นกดเริ่มด่าน (ใช้เช็ค Replay Rate)
    /// </summary>
    public void OnGameStarted(bool isReplay)
    {
        _roundStartTime = Time.time;
        _playCount++;

        var parameters = new Dictionary<string, object>
        {
            { "is_replay", isReplay },
            { "session_play_count", _playCount }
        };

        _analyticsService.RecordCustomEvent("round_started", parameters);
    }

    /// <summary>
    /// เรียกเมื่อจบด่าน (ใช้เช็ค Completion/Failed Rate)
    /// </summary>
    public void OnGameEnded(bool isWin, int currentWave)
    {
        float roundDuration = Time.time - _roundStartTime;

        // ใช้ Mathf.Floor ปัดเศษทศนิยมทิ้ง แต่ยังเก็บชนิดตัวแปรเป็น float เหมือนเดิม
        float roundedDuration = Mathf.Floor(roundDuration);

        string outcome = isWin ? "win" : "lose";

        var parameters = new Dictionary<string, object>
        {
            { "duration_seconds", roundedDuration }, // ส่งค่าเป็น Float ที่ไม่มีเศษทศนิยม
            { "outcome", outcome },                // อัตราการผ่าน/ไม่ผ่านด่าน
            { "wave_reached", currentWave }        // เก็บข้อมูล Wave ล่าสุดที่ไปถึงด้วย
        };

        _analyticsService.RecordCustomEvent("round_ended", parameters);

        // ส่งข้อมูลขึ้น Cloud ทันทีเมื่อจบเกม
        _analyticsService.FlushData();
    }
    #endregion

    #region App Session Tracking (ทั้งเกม)
    /// <summary>
    /// ตรวจจับการพับจอ (Mobile) หรือสลับแอป
    /// </summary>
    private void OnApplicationPause(bool isPaused)
    {
        if (isPaused)
        {
            // แอปถูกพับไปแบคกราวด์ ให้ถือว่าจบ Session ย่อย และส่งข้อมูล
            //SendAppSessionEvent("app_paused");
        }
        else
        {
            // กลับมาเข้าแอปใหม่ ให้รีเซ็ตเวลาเริ่มต้น Session ใหม่
            //_appSessionStartTime = Time.realtimeSinceStartup;
        }
    }

    /// <summary>
    /// ตรวจจับตอนปิดเกม (PC/Mac/Editor หรือบางกรณีบน Mobile)
    /// </summary>
    private void OnApplicationQuit()
    {
        SendAppSessionEvent("app_quit");
    }

    /// <summary>
    /// รวบรวมเวลาทั้งหมดและส่ง Event
    /// </summary>
    private void SendAppSessionEvent(string exitReason)
    {
        // คำนวณเวลาทั้งหมดตั้งแต่เปิดแอป หรือกลับมาจากพับจอ
        float sessionDuration = Time.realtimeSinceStartup - _appSessionStartTime;

        // ป้องกันการส่งข้อมูลขยะถ้าน้อยกว่า 1 วินาที (เช่น บัคสลับจอไปมาติดๆ กัน)
        if (sessionDuration < 1f) return;

        // ใช้ Mathf.Floor ปัดเศษทศนิยมทิ้ง
        float roundedSessionDuration = Mathf.Floor(sessionDuration);

        var parameters = new Dictionary<string, object>
        {
            { "total_session_seconds", roundedSessionDuration },
            { "exit_reason", exitReason } // เหตุผลที่ออก (พับจอ หรือ ปิดเกม)
        };

        _analyticsService.RecordCustomEvent("app_session_ended", parameters);

        // บังคับส่งข้อมูลที่ค้างอยู่ขึ้น Cloud ทันทีก่อนที่แอปจะถูกปิดตัวลงจริงๆ
        _analyticsService.FlushData();
    }
    #endregion
}