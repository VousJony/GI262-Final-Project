using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager instance;

    public GameObject[] enemiesPrefab;
    public Transform[] spawnPoints;

    private int lastSpawnIndex = -1;
    private List<int> _availableSpawnIndices;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        _availableSpawnIndices = new List<int>();
    }

    public void ClearAllEnemies()
    {
        // วิธีที่เร็วที่สุดถ้าเรา Instantiate ศัตรูไว้ใต้ transform ของ EnemyManager (ตามโค้ดเก่า)
        // คือการลบลูกๆ ทั้งหมดทิ้ง
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // หรือถ้าศัตรูอยู่นอก Parent นี้ ให้ใช้ FindObjectsByType<Enemy>() แล้วลบ
        // Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        // foreach(var e in enemies) Destroy(e.gameObject);
    }

    public IEnumerator SpawnEnemiesBatch(int enemiesToSpawn, float delayBetweenSpawns)
    {
        if (spawnPoints.Length == 0) yield break;

        _availableSpawnIndices.Clear();
        for (int i = 0; i < spawnPoints.Length; i++) _availableSpawnIndices.Add(i);

        if (spawnPoints.Length > 1 && lastSpawnIndex != -1 && _availableSpawnIndices.Contains(lastSpawnIndex))
        {
            _availableSpawnIndices.Remove(lastSpawnIndex);
            _availableSpawnIndices.Add(lastSpawnIndex);
        }

        Shuffle(_availableSpawnIndices);

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            // ตรวจสอบ State ก่อน Spawn เพื่อป้องกันการ Spawn ตอนจบเกมแล้ว
            if (MenuManager.instance.currentState != GameState.Playing) yield break;

            int spawnIndex = _availableSpawnIndices[i % _availableSpawnIndices.Count];
            lastSpawnIndex = spawnIndex;
            int enemyIndex = Random.Range(0, enemiesPrefab.Length);

            // Instantiate เป็นลูกของ transform (EnemyManager) เพื่อให้ง่ายต่อการ Clear
            Instantiate(enemiesPrefab[enemyIndex], spawnPoints[spawnIndex].position, Quaternion.identity, transform);

            yield return new WaitForSeconds(delayBetweenSpawns);
        }
    }

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
}