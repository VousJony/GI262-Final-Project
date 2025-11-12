using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// โครงสร้างสำหรับกำหนดชุดการ Spawn ศัตรูในแต่ละ Wave
[System.Serializable]
public class SpawnGroup
{
    // จำนวนศัตรูในชุดนี้
    [Tooltip("จำนวนศัตรูที่จะ Spawn ในชุดนี้")]
    public int enemiesCount = 3;

    // เวลาหน่วงระหว่างการ Spawn ศัตรูแต่ละตัวในชุด
    [Tooltip("เวลาหน่วงระหว่างการ Spawn ศัตรูแต่ละตัว (ภายในกลุ่ม)")]
    public float delayBetweenEnemies = 0.5f;

    // เวลาหน่วงก่อนจะเริ่มชุด Spawn ถัดไป
    [Tooltip("เวลาหน่วงก่อนเริ่ม Spawn ชุดถัดไป")]
    public float delayBeforeNextGroup = 1f;
}

// โครงสร้างสำหรับกำหนดค่า Wave ทั้งหมด
[System.Serializable]
public class WaveConfig
{
    [Tooltip("รายละเอียดของแต่ละชุดการ Spawn ใน Wave นี้")]
    public List<SpawnGroup> spawnGroups;
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Wave Configuration")]
    [Tooltip("กำหนดค่า Wave ทั้งหมดตามลำดับที่จะ Spawn")]
    [SerializeField] private List<WaveConfig> allWaveConfigs; // กำหนดค่าใน Inspector

    // Queue สำหรับจัดการลำดับ Wave ที่จะ Spawn
    private Queue<WaveConfig> waveQueue;
    private WaveConfig currentWaveConfig;

    [Header("UI Status")]
    [SerializeField] private int wave = 0;
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private int totalEnemiesInWave = 0;
    [SerializeField] private int waveProgress = 0;
    [SerializeField] private Slider waveBar;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 1. นำ configs ทั้งหมดเข้าสู่ Queue
        waveQueue = new Queue<WaveConfig>(allWaveConfigs);

        SetUpWaveText();

        // 2. เริ่ม Wave แรก
        StartCoroutine(StartNextWave());
    }

    private void SetUpWaveText()
    {
        waveText.text = "Wave " + wave;
    }

    public IEnumerator StartNextWave()
    {
        if (waveQueue.Count == 0)
        {
            Debug.Log("เกมจบแล้ว! (Endless Mode หรือจบ Config)");
            waveText.text = "All Waves Cleared!";
            yield break; // จบ Coroutine ถ้าไม่มี Wave เหลือ
        }

        // 1. ดึง Wave Config ถัดไปจาก Queue
        currentWaveConfig = waveQueue.Dequeue();

        wave++;
        waveProgress = 0;

        // 2. คำนวณจำนวนศัตรูรวมทั้งหมดใน Wave นี้
        totalEnemiesInWave = 0;
        foreach (var group in currentWaveConfig.spawnGroups)
        {
            totalEnemiesInWave += group.enemiesCount;
        }

        waveBar.maxValue = totalEnemiesInWave;
        waveBar.value = 0;

        if (wave > 1)
        {
            // Count down
            for (int i = 0; i < 5; i++)
            {
                waveText.text = $"Next Wave start in...{5 - i}";

                yield return new WaitForSecondsRealtime(1f);
            }

            waveText.text = "WAVE START!";
            yield return new WaitForSecondsRealtime(0.5f);
        }

        SetUpWaveText();

        // 3. เริ่ม Spawn ตามชุดที่กำหนด
        foreach (var group in currentWaveConfig.spawnGroups)
        {
            // เรียก Coroutine ใน EnemyManager เพื่อจัดการ Spawn ภายในชุด
            yield return EnemyManager.instance.StartCoroutine(
                EnemyManager.instance.SpawnEnemiesBatch(group.enemiesCount, group.delayBetweenEnemies)
            );

            // หน่วงเวลาก่อนเริ่มชุด Spawn ถัดไป
            yield return new WaitForSeconds(group.delayBeforeNextGroup);
        }
    }

    public void UpdateWaveProgress()
    {
        waveProgress++;
        waveBar.value = waveProgress;

        if (waveProgress >= waveBar.maxValue)
        {
            // Wave completed
            Debug.Log("Wave " + wave + " completed!");

            // เริ่ม Wave ถัดไปโดยดึงจาก Queue
            StartCoroutine(StartNextWave());
        }
    }

}