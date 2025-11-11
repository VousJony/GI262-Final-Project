using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager instance;

    public GameObject[] enemiesPrefab;

    public Transform[] spawnPoints;

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

    public void SpawnEnemy()
    {
        int spawnIndex = Random.Range(0, spawnPoints.Length);
        int enemyIndex = Random.Range(0, enemiesPrefab.Length);

        Instantiate(enemiesPrefab[enemyIndex], spawnPoints[spawnIndex].position, Quaternion.identity ,transform);
    }

}
