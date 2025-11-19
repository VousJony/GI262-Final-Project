using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [Header("Obstacle Settings")]
    [SerializeField] private GameObject[] obstaclePrefabs;
    [SerializeField] private float spawnInterval = 1.5f;
    // [SerializeField] private float xLimit = 5f; // ไม่ได้ใช้แล้วเพราะไปใช้ SpawnPoint แทน
    [SerializeField] private float spawnY = 11f;

    [Header("Map Settings (Background)")]
    [Tooltip("Prefab ของพื้นหลัง (ควรมีขนาดเท่ากันทุกชิ้น)")]
    [SerializeField] private GameObject[] mapPrefabs;
    [Tooltip("ความสูงของ Map Prefab 1 ชิ้น (หน่วย World Unit)")]
    [SerializeField] private float mapChunkHeight = 10f;
    [Tooltip("ความเร็วในการเลื่อนของ Map (ควรเท่ากับความเร็ว Obstacle เพื่อความสมจริง)")]
    [SerializeField] private float mapScrollSpeed = 5f;
    [Tooltip("จุดต่ำสุดที่ Map จะถูกย้ายกลับไปข้างบน (ควรต่ำกว่าขอบจอล่าง)")]
    [SerializeField] private float mapResetY = -15f;
    [Tooltip("จำนวนชิ้นส่วน Map ที่จะสร้างเพื่อวนลูป (ปกติ 3 ชิ้นก็พอสำหรับเต็มจอ)")]
    [SerializeField] private int mapChunkCount = 3;

    [Header("Difficulty")]
    [SerializeField] private bool increaseDifficulty = true;
    [SerializeField] private float minSpawnInterval = 0.5f;
    [SerializeField] private float difficultyMultiplier = 0.99f;

    private bool isGenerating = false;
    private float currentSpawnInterval;

    // เก็บรายการ Map ที่กำลังทำงานอยู่
    private List<GameObject> activeMaps = new List<GameObject>();

    public void StartGeneration()
    {
        currentSpawnInterval = spawnInterval;
        isGenerating = true;

        // 1. สร้าง Map เริ่มต้น
        SpawnInitialMaps();

        // 2. เริ่ม Spawn Obstacle
        StartCoroutine(SpawnRoutine());
    }

    public void StopAndClear()
    {
        isGenerating = false;
        StopAllCoroutines();

        // ลบ Obstacle ทั้งหมด (ลูกของ MapGenerator)
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        activeMaps.Clear();
    }

    private void Update()
    {
        // ถ้าเกมไม่ได้เล่นอยู่ (เช่น Pause หรือ Game Over) ไม่ต้องเลื่อน Map
        if (MenuManager.instance.currentState != GameState.Playing) return;

        // จัดการการเลื่อนของ Map
        HandleMapScrolling();
    }

    // --- Map Logic (Recycling System) ---

    private void SpawnInitialMaps()
    {
        // ล้างของเก่าก่อนเสมอ
        if (activeMaps.Count > 0) activeMaps.Clear();

        // สร้าง Map ตามจำนวนที่กำหนด เรียงต่อกันขึ้นไปข้างบน
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

        // สุ่ม Prefab (เผื่อมีพื้นหลายแบบ)
        int index = Random.Range(0, mapPrefabs.Length);
        GameObject newMap = Instantiate(mapPrefabs[index], position, Quaternion.identity, transform);

        activeMaps.Add(newMap);
    }

    private void HandleMapScrolling()
    {
        if (activeMaps.Count == 0) return;

        for (int i = 0; i < activeMaps.Count; i++)
        {
            // เลื่อนลง
            activeMaps[i].transform.Translate(Vector3.down * mapScrollSpeed * Time.deltaTime);
        }

        // ตรวจสอบชิ้นที่อยู่ล่างสุด (ชิ้นแรกใน List จะเป็นชิ้นล่างสุดเสมอตาม Logic การ Spawn)
        GameObject bottomMap = activeMaps[0];

        // ถ้าหลุดขอบล่างที่กำหนด
        if (bottomMap.transform.position.y <= mapResetY)
        {
            // หาตำแหน่งของชิ้นบนสุด
            GameObject topMap = activeMaps[activeMaps.Count - 1];
            float newY = topMap.transform.position.y + mapChunkHeight;

            // ย้ายชิ้นล่างสุด ไปต่อบนสุด (Recycle)
            bottomMap.transform.position = new Vector3(0, newY, 0);

            // ย้าย Reference ใน List: เอาตัวแรกออก ไปต่อท้าย
            activeMaps.RemoveAt(0);
            activeMaps.Add(bottomMap);
        }
    }

    // --- Obstacle Logic ---

    private IEnumerator SpawnRoutine()
    {
        while (isGenerating)
        {
            SpawnObstacle();

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

        // ตรวจสอบ Prefab ของ Obstacle
        bool hasObstacles = obstaclePrefabs != null && obstaclePrefabs.Length > 0;

        // ตรวจสอบ SpawnPoints จาก EnemyManager
        bool hasSpawnPoints = EnemyManager.instance != null &&
                              EnemyManager.instance.spawnPoints != null &&
                              EnemyManager.instance.spawnPoints.Length > 0;

        if (hasObstacles && hasSpawnPoints)
        {
            // 1. สุ่ม Obstacle
            int randomIndex = Random.Range(0, obstaclePrefabs.Length);

            // 2. สุ่ม Spawn Point จาก EnemyManager
            Transform[] points = EnemyManager.instance.spawnPoints;
            int pointIndex = Random.Range(0, points.Length);

            // 3. ใช้ตำแหน่ง X ของ SpawnPoint แต่ใช้ตำแหน่ง Y จากค่า spawnY ของ MapGenerator (เพื่อให้เริ่มตกจากข้างบนสุดเหมือนเดิม)
            float spawnPointX = points[pointIndex].position.x;
            Vector3 finalSpawnPos = new Vector3(spawnPointX, spawnY, 0);

            Instantiate(obstaclePrefabs[randomIndex], finalSpawnPos, Quaternion.identity, transform);
        }
        else if (!hasSpawnPoints)
        {
            Debug.LogWarning("MapGenerator: ไม่พบ SpawnPoints ใน EnemyManager หรือ EnemyManager ยังไม่ถูก Initialize");
        }
    }
}