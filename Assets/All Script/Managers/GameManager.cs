using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class SpawnGroup
{
    public int enemiesCount = 3;
    public float delayBetweenEnemies = 0.5f;
    public float delayBeforeNextGroup = 1f;
}

[System.Serializable]
public class WaveConfig
{
    public List<SpawnGroup> spawnGroups;
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Wave Configuration")]
    [SerializeField] private List<WaveConfig> allWaveConfigs;

    private Queue<WaveConfig> waveQueue;
    private WaveConfig currentWaveConfig;

    [Header("UI Status")]
    [SerializeField] private int wave = 0;
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private int totalEnemiesInWave = 0;
    [SerializeField] private int waveProgress = 0;
    [SerializeField] private Slider waveBar;

    private Coroutine waveCoroutine;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        SetUpWaveText();
    }

    public void StartGame()
    {
        Character.instance.SetUpNewGame();

        wave = 0;
        waveQueue = new Queue<WaveConfig>(allWaveConfigs);
        SetUpWaveText();

        MapGenerator mapGen = FindFirstObjectByType<MapGenerator>();
        if (mapGen != null) mapGen.StartGeneration();

        if (waveCoroutine != null) StopCoroutine(waveCoroutine);
        waveCoroutine = StartCoroutine(StartNextWave());
    }

    public void StopAndResetGame()
    {
        if (waveCoroutine != null) StopCoroutine(waveCoroutine);

        EnemyManager.instance.ClearAllEnemies();

        MapGenerator mapGen = FindFirstObjectByType<MapGenerator>();
        if (mapGen != null) mapGen.StopAndClear();

        Projectile[] bullets = FindObjectsByType<Projectile>(FindObjectsSortMode.None);
        foreach (var b in bullets) Destroy(b.gameObject);
    }

    private void SetUpWaveText()
    {
        if (waveText != null) waveText.text = "Wave " + wave;
    }

    public IEnumerator StartNextWave()
    {
        // ตรวจสอบว่า Wave หมดหรือยัง
        if (waveQueue.Count == 0)
        {
            Debug.Log("All Waves Cleared!");

            // [แก้ไข] เรียกหน้า Victory แทนข้อความ Text เฉยๆ
            if (MenuManager.instance != null)
            {
                MenuManager.instance.TriggerVictory();
            }

            yield break;
        }

        currentWaveConfig = waveQueue.Dequeue();
        wave++;
        waveProgress = 0;

        totalEnemiesInWave = 0;
        foreach (var group in currentWaveConfig.spawnGroups)
        {
            totalEnemiesInWave += group.enemiesCount;
        }

        if (waveBar != null)
        {
            waveBar.maxValue = totalEnemiesInWave;
            waveBar.value = 0;
        }

        if (wave > 1)
        {
            for (int i = 0; i < 3; i++)
            {
                if (waveText != null) waveText.text = $"Next Wave: {3 - i}";
                yield return new WaitForSecondsRealtime(1f);
            }
        }

        SetUpWaveText();

        foreach (var group in currentWaveConfig.spawnGroups)
        {
            if (MenuManager.instance.currentState != GameState.Playing) yield break;

            yield return EnemyManager.instance.StartCoroutine(
                EnemyManager.instance.SpawnEnemiesBatch(group.enemiesCount, group.delayBetweenEnemies)
            );

            yield return new WaitForSeconds(group.delayBeforeNextGroup);
        }
    }

    public void UpdateWaveProgress()
    {
        waveProgress++;
        if (waveBar != null) waveBar.value = waveProgress;

        if (waveBar != null && waveProgress >= waveBar.maxValue)
        {
            StartCoroutine(StartNextWave());
        }
    }
}