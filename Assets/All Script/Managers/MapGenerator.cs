using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    #region Fields
    [Header("Obstacle Settings")]
    [SerializeField] private GameObject[] obstaclePrefabs;
    [SerializeField] private float spawnInterval = 1.5f;
    [SerializeField] private float spawnY = 11f;

    [Header("Map Settings")]
    [SerializeField] private GameObject[] mapPrefabs;
    [SerializeField] private float mapChunkHeight = 10f;
    [SerializeField] private float mapScrollSpeed = 5f;
    [SerializeField] private float mapResetY = -15f;
    [SerializeField] private int mapChunkCount = 3;

    [Header("Difficulty")]
    [SerializeField] private bool increaseDifficulty = true;
    [SerializeField] private float minSpawnInterval = 0.5f;
    [SerializeField] private float difficultyMultiplier = 0.99f;

    private bool isGenerating = false;
    private float currentSpawnInterval;
    private List<GameObject> activeMaps = new List<GameObject>();
    #endregion

    #region Unity Methods
    private void Update()
    {
        if (MenuManager.instance.currentState == GameState.Playing)
        {
            HandleMapScrolling();
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// เริ่มสร้าง Map และสิ่งกีดขวาง
    /// </summary>
    public void StartGeneration()
    {
        currentSpawnInterval = spawnInterval;
        isGenerating = true;

        SpawnInitialMaps();
        StartCoroutine(SpawnRoutine());
    }

    /// <summary>
    /// หยุดสร้างและล้าง Map ทั้งหมด
    /// </summary>
    public void StopAndClear()
    {
        isGenerating = false;
        StopAllCoroutines();

        foreach (Transform child in transform) Destroy(child.gameObject);
        activeMaps.Clear();
    }
    #endregion

    #region Map Logic (Recycling)
    /// <summary>
    /// สร้าง Map เริ่มต้นตามจำนวน Chunk
    /// </summary>
    private void SpawnInitialMaps()
    {
        if (activeMaps.Count > 0) activeMaps.Clear();
        Vector3 spawnPosition = transform.position;

        for (int i = 0; i < mapChunkCount; i++)
        {
            SpawnMapChunk(spawnPosition);
            spawnPosition.y += mapChunkHeight;
        }
    }

    private void SpawnMapChunk(Vector3 position)
    {
        if (mapPrefabs.Length == 0) return;
        int index = Random.Range(0, mapPrefabs.Length);
        GameObject newMap = Instantiate(mapPrefabs[index], position, Quaternion.identity, transform);
        activeMaps.Add(newMap);
    }

    /// <summary>
    /// จัดการเลื่อน Map ลงมาและย้ายชิ้นที่ตกขอบกลับไปข้างบน (Infinite Scrolling)
    /// </summary>
    private void HandleMapScrolling()
    {
        if (activeMaps.Count == 0) return;

        // เลื่อนทุกชิ้นลง
        foreach (var map in activeMaps)
        {
            map.transform.Translate(Vector3.down * mapScrollSpeed * Time.deltaTime);
        }

        // Recycle ชิ้นล่างสุดไปไว้บนสุด
        GameObject bottomMap = activeMaps[0];
        if (bottomMap.transform.position.y <= mapResetY)
        {
            GameObject topMap = activeMaps[activeMaps.Count - 1];
            float newY = topMap.transform.position.y + mapChunkHeight;

            bottomMap.transform.position = new Vector3(0, newY, 0);

            // ย้าย Index ใน List
            activeMaps.RemoveAt(0);
            activeMaps.Add(bottomMap);
        }
    }
    #endregion

    #region Obstacle Logic
    private IEnumerator SpawnRoutine()
    {
        while (isGenerating)
        {
            SpawnObstacle();

            // เพิ่มความยากโดยลดระยะเวลาเกิด
            if (increaseDifficulty && currentSpawnInterval > minSpawnInterval)
            {
                currentSpawnInterval *= difficultyMultiplier;
            }

            yield return new WaitForSeconds(currentSpawnInterval);
        }
    }

    private void SpawnObstacle()
    {
        if (MenuManager.instance.currentState != GameState.Playing) return;
        if (obstaclePrefabs.Length == 0) return;

        // สุ่มจุดเกิดจาก EnemyManager แต่บังคับค่า Y ให้เริ่มจากข้างบน
        if (EnemyManager.instance != null && EnemyManager.instance.spawnPoints.Length > 0)
        {
            Transform[] points = EnemyManager.instance.spawnPoints;
            int pointIndex = Random.Range(0, points.Length);
            Vector3 finalSpawnPos = new Vector3(points[pointIndex].position.x, spawnY, 0);

            int randomObsIndex = Random.Range(0, obstaclePrefabs.Length);
            Instantiate(obstaclePrefabs[randomObsIndex], finalSpawnPos, Quaternion.identity, transform);
        }
    }
    #endregion
}