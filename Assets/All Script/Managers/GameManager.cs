using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Wave")]
    [SerializeField] private int wave = 0;
    [SerializeField] private TextMeshProUGUI waveText;
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
        SetUpWaveText();
        StartCoroutine(StartNewWave(3));
    }

    private void SetUpWaveText()
    {
        waveText.text = "Wave " + wave;
    }

    public IEnumerator StartNewWave(int enemiesCount)
    {
        wave++;
        waveProgress = 0;
        waveBar.maxValue = enemiesCount;
        waveBar.value = 0;

        if (wave > 1)
        {
            for (int i = 0; i < 5; i++)
            {
                waveText.text = $"Next Wave start in...{5-i}";

                yield return new WaitForSecondsRealtime(1f);
            }

            waveText.text = "WAVE START!";
            yield return new WaitForSecondsRealtime(0.5f);
        }

        SetUpWaveText();
        for (int i = 0; i < enemiesCount; i++)
        {
            EnemyManager.instance.SpawnEnemy();
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
            StartCoroutine(StartNewWave(wave + 2));
        }
    }

}
