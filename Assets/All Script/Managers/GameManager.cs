using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#region Helper Classes
[System.Serializable]
public class EnemySpawnConfig
{
    public GameObject enemyPrefab;
    public int count = 1;
}

[System.Serializable]
public class SpawnGroup
{
    [Tooltip("รายชื่อศัตรูที่จะเกิดในกลุ่มนี้")]
    public List<EnemySpawnConfig> enemiesInGroup;
    public float delayBetweenEnemies = 0.5f;
    public float delayBeforeNextGroup = 1f;
}

[System.Serializable]
public class WaveConfig
{
    public List<SpawnGroup> spawnGroups;
}
#endregion

public class GameManager : MonoBehaviour
{
    #region Singleton
    public static GameManager instance;
    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }
    #endregion

    #region Fields
    [Header("Wave Config")]
    [SerializeField] private List<WaveConfig> allWaveConfigs;

    // คิวสำหรับจัดการ Wave
    private Queue<WaveConfig> waveQueue;
    private WaveConfig currentWaveConfig;
    private Coroutine waveCoroutine;

    [Header("Wave UI")]
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private Slider waveBar;

    private int wave = 0;
    private int waveProgress = 0;
    private int totalEnemiesInWave = 0;
    #endregion

    #region Game Flow Control
    public void StartGame()
    {
        // เตรียมผู้เล่น
        Character.instance.SetUpNewGame();

        // เตรียม Wave
        wave = 0;
        waveQueue = new Queue<WaveConfig>(allWaveConfigs);
        UpdateWaveTextUI();

        // เริ่มสร้าง Map
        MapGenerator mapGen = FindFirstObjectByType<MapGenerator>();
        if (mapGen != null) mapGen.StartGeneration();

        // เริ่ม Loop Wave
        if (waveCoroutine != null) StopCoroutine(waveCoroutine);
        waveCoroutine = StartCoroutine(StartNextWave());
    }

    /// <summary>
    /// หยุดเกมและล้างค่าทุกอย่าง (ใช้เมื่อจบเกมหรือออกเมนู)
    /// </summary>
    public void StopAndResetGame()
    {
        if (waveCoroutine != null) StopCoroutine(waveCoroutine);

        EnemyManager.instance.ClearAllEnemies();

        MapGenerator mapGen = FindFirstObjectByType<MapGenerator>();
        if (mapGen != null) mapGen.StopAndClear();

        // ล้างกระสุนที่ค้างในฉาก
        foreach (var b in FindObjectsByType<Projectile>(FindObjectsSortMode.None))
            Destroy(b.gameObject);
    }
    #endregion

    #region Wave Logic
    /// <summary>
    /// Coroutine จัดการการเกิดของศัตรูในแต่ละ Wave
    /// </summary>
    public IEnumerator StartNextWave()
    {
        // ตรวจสอบว่าชนะเกมหรือยัง
        if (waveQueue.Count == 0)
        {
            if (MenuManager.instance != null) MenuManager.instance.TriggerVictory();
            yield break;
        }

        // ดึง Wave ถัดไป
        currentWaveConfig = waveQueue.Dequeue();
        wave++;
        waveProgress = 0;

        // คำนวณจำนวนศัตรูรวมเพื่อตั้งค่าหลอด Progress
        CalculateTotalEnemies();
        InitializeWaveUI();

        // นับถอยหลังก่อนเริ่ม Wave (ยกเว้น Wave แรก)
        if (wave > 1)
        {
            for (int i = 3; i > 0; i--)
            {
                if (waveText != null) waveText.text = $"Next Wave: {i}";
                yield return new WaitForSecondsRealtime(1f);
            }
        }
        UpdateWaveTextUI();

        // Loop Spawn ศัตรูทีละกลุ่ม
        foreach (var group in currentWaveConfig.spawnGroups)
        {
            if (MenuManager.instance.currentState != GameState.Playing) yield break;

            if (group.enemiesInGroup != null)
            {
                foreach (var enemyConfig in group.enemiesInGroup)
                {
                    if (enemyConfig.enemyPrefab != null && enemyConfig.count > 0)
                    {
                        // สั่งให้ EnemyManager สร้างศัตรู
                        yield return EnemyManager.instance.StartCoroutine(
                            EnemyManager.instance.SpawnSpecificEnemies(
                                enemyConfig.enemyPrefab,
                                enemyConfig.count,
                                group.delayBetweenEnemies
                            )
                        );
                    }
                }
            }
            yield return new WaitForSeconds(group.delayBeforeNextGroup);
        }
    }

    public void UpdateWaveProgress()
    {
        waveProgress++;
        if (waveBar != null) waveBar.value = waveProgress;

        // เมื่อศัตรูครบจำนวน ให้เริ่ม Wave ถัดไป
        if (waveBar != null && waveProgress >= waveBar.maxValue)
        {
            StartCoroutine(StartNextWave());
        }
    }
    #endregion

    #region UI Helper
    private void CalculateTotalEnemies()
    {
        totalEnemiesInWave = 0;
        foreach (var group in currentWaveConfig.spawnGroups)
        {
            if (group.enemiesInGroup != null)
            {
                foreach (var config in group.enemiesInGroup)
                    totalEnemiesInWave += config.count;
            }
        }
    }

    private void InitializeWaveUI()
    {
        if (waveBar != null)
        {
            waveBar.maxValue = totalEnemiesInWave;
            waveBar.value = 0;
        }
    }

    private void UpdateWaveTextUI()
    {
        if (waveText != null) waveText.text = "Wave " + wave;
    }
    #endregion
}