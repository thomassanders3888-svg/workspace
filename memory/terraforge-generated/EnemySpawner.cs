using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour {
    public GameObject[] enemyPrefabs;
    public float spawnRadius = 30f;
    public float spawnInterval = 5f;
    public int maxEnemies = 20;
    public Transform player;
    
    private int currentEnemies;
    
    void Start() { StartCoroutine(SpawnLoop()); }
    
    IEnumerator SpawnLoop() {
        while (true) {
            if (currentEnemies < maxEnemies) SpawnEnemy();
            yield return new WaitForSeconds(spawnInterval);
        }
    }
    
    void SpawnEnemy() {
        if (player == null) return;
        Vector3 spawnPos = player.position + Random.insideUnitSphere * spawnRadius;
        spawnPos.y = GetGroundHeight(spawnPos);
        Instantiate(enemyPrefabs[Random.Range(0, enemyPrefabs.Length)], spawnPos, Quaternion.identity);
        currentEnemies++;
    }
    
    float GetGroundHeight(Vector3 pos) { RaycastHit hit; if (Physics.Raycast(pos + Vector3.up * 100, Vector3.down, out hit, 200)) return hit.point.y; return 0; }
    public void EnemyDied() { currentEnemies = Mathf.Max(0, currentEnemies - 1); }
}
