using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager instance;

    public GameObject[] enemiesPrefab;

    public Transform[] spawnPoints;

    // ตัวแปรสำหรับเก็บ index ตำแหน่ง Spawn ล่าสุด
    private int lastSpawnIndex = -1;

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

    /// <summary>
    /// สั่งให้ Spawn ศัตรูเป็นชุดๆ พร้อมกำหนด Delay
    /// </summary>
    /// <param name="enemiesToSpawn">จำนวนศัตรูในชุดนี้</param>
    /// <param name="delayBetweenSpawns">เวลาหน่วงระหว่างการ Spawn ศัตรูแต่ละตัวในชุด</param>
    /// <returns>IEnumerator</returns>
    public IEnumerator SpawnEnemiesBatch(int enemiesToSpawn, float delayBetweenSpawns)
    {
        // 1. ตรวจสอบว่ามี Spawn Point หรือไม่
        if (spawnPoints.Length == 0)
        {
            Debug.LogError("SpawnPoints array is empty! Cannot spawn enemies.");
            yield break;
        }

        // 2. สร้างรายการของ Spawn Point Index ที่มีให้ใช้ในชุดนี้
        // และพยายามป้องกันไม่ให้ใช้ lastSpawnIndex เป็นจุดแรกของชุด
        List<int> availableSpawnIndices = Enumerable.Range(0, spawnPoints.Length).ToList();

        // ถ้ามีจุด Spawn มากกว่า 1 และจุด Spawn ล่าสุดยังอยู่ในรายการ ให้ย้ายจุดนั้นไปท้ายสุด
        if (spawnPoints.Length > 1 && lastSpawnIndex != -1 && availableSpawnIndices.Contains(lastSpawnIndex))
        {
            availableSpawnIndices.Remove(lastSpawnIndex);
            availableSpawnIndices.Add(lastSpawnIndex);
        }

        // 3. ทำการ Shuffle รายการทั้งหมด (ยกเว้นจุดล่าสุดที่ถูกย้ายไปท้ายสุด)
        // เพื่อให้เกิดความสุ่มในการวนใช้จุด Spawn
        Shuffle(availableSpawnIndices);

        // 4. เริ่มทำการ Spawn แต่ละตัวในชุดตามจำนวนที่ต้องการ
        for (int i = 0; i < enemiesToSpawn; i++)
        {
            // ใช้ Modulo Operator (%) เพื่อวนใช้ Spawn Point Indices ที่มีอยู่
            int spawnIndex = availableSpawnIndices[i % availableSpawnIndices.Count];

            // 5. บันทึก Index ของจุดที่ใช้ล่าสุด (สำหรับป้องกันการซ้ำในรอบถัดไป)
            lastSpawnIndex = spawnIndex;

            // 6. สุ่มประเภทศัตรู
            int enemyIndex = Random.Range(0, enemiesPrefab.Length);

            // 7. Instantiate
            Instantiate(enemiesPrefab[enemyIndex], spawnPoints[spawnIndex].position, Quaternion.identity, transform);

            // 8. หน่วงเวลาก่อน Spawn ตัวถัดไป
            yield return new WaitForSeconds(delayBetweenSpawns);
        }
    }

    // ฟังก์ชันช่วยในการสุ่ม Shuffle รายการ
    private void Shuffle<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}