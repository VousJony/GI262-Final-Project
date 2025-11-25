using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    #region Singleton
    public static EnemyManager instance;
    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        _availableSpawnIndices = new List<int>();
    }
    #endregion

    #region Fields
    [Header("Configuration")]
    [Tooltip("จุดเกิดของศัตรู")]
    public Transform[] spawnPoints;

    // ตัวแปรช่วยคำนวณการสุ่มจุดเกิด
    private int lastSpawnIndex = -1;
    private List<int> _availableSpawnIndices;
    #endregion

    #region Public Methods
    /// <summary>
    /// ลบศัตรูทั้งหมดในฉาก (ใช้เมื่อจบเกม/เริ่มใหม่)
    /// </summary>
    public void ClearAllEnemies()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// Spawn ศัตรูแบบเจาะจงชนิดและจำนวน โดยสุ่มจุดเกิดไม่ให้ซ้ำจุดเดิมทันที
    /// </summary>
    public IEnumerator SpawnSpecificEnemies(GameObject specificPrefab, int count, float delayBetweenSpawns)
    {
        if (spawnPoints.Length == 0 || specificPrefab == null) yield break;

        // เตรียมรายชื่อ Index จุดเกิด
        _availableSpawnIndices.Clear();
        for (int i = 0; i < spawnPoints.Length; i++) _availableSpawnIndices.Add(i);

        // ป้องกันการเกิดจุดเดิมซ้ำซ้อน (Shuffle Logic)
        if (spawnPoints.Length > 1 && lastSpawnIndex != -1 && _availableSpawnIndices.Contains(lastSpawnIndex))
        {
            _availableSpawnIndices.Remove(lastSpawnIndex);
            _availableSpawnIndices.Add(lastSpawnIndex);
        }

        Shuffle(_availableSpawnIndices);

        for (int i = 0; i < count; i++)
        {
            if (MenuManager.instance.currentState != GameState.Playing) yield break;

            // เลือกจุดเกิดจาก List ที่สลับแล้ว
            int spawnIndex = _availableSpawnIndices[i % _availableSpawnIndices.Count];
            lastSpawnIndex = spawnIndex;

            Instantiate(specificPrefab, spawnPoints[spawnIndex].position, Quaternion.identity, transform);

            yield return new WaitForSeconds(delayBetweenSpawns);
        }
    }
    #endregion

    #region Helper Methods
    /// <summary>
    /// Fisher-Yates shuffle สำหรับสลับตำแหน่งใน List
    /// </summary>
    private void Shuffle<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }
    #endregion
}