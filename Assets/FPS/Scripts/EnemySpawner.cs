using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Spawn Settings")]
    public GameObject enemyPrefab;         // Prefab Zombie
    public Transform[] spawnPoints;        // Các vị trí spawn cố định
    public int maxEnemiesToSpawn = 10;     // Số lượng enemy spawn tối đa
    public float spawnInterval = 2f;       // Thời gian giữa các lần spawn

    private int enemiesSpawned = 0;        // Đếm số enemy đã spawn
    private int enemiesKilled = 0;         // Đếm số enemy bị tiêu diệt
    private List<GameObject> activeEnemies = new List<GameObject>();

    void Start()
    {
        StartCoroutine(SpawnEnemies());
    }

    IEnumerator<WaitForSeconds> SpawnEnemies()
    {
        while (enemiesSpawned < maxEnemiesToSpawn)
        {
            // Chọn spawn point ngẫu nhiên
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            // Spawn enemy
            GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
            activeEnemies.Add(enemy);

            // Gán callback khi enemy chết
            EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.onDeath += EnemyKilled;
            }

            enemiesSpawned++;
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void EnemyKilled(GameObject enemy)
    {
        enemiesKilled++;
        activeEnemies.Remove(enemy);

        // Kiểm tra điều kiện thắng
        if (enemiesKilled >= maxEnemiesToSpawn)
        {
            Debug.Log("Bạn đã thắng! Tiêu diệt đủ 10 zombie!");
            // Có thể gọi UI thắng hoặc dừng game ở đây
        }
    }
}
