using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour {
    public GameObject enemyPrefab;
    public Transform[] spawnPoints;
    public float spawnInterval = 30f;
    public int maxEnemies = 10;
    public AnimationCurve difficultyCurve;
    
    private int waveNumber = 1;
    private int enemiesSpawned;
    
    void Start() {
        StartCoroutine(SpawnWave());
    }
    
    IEnumerator SpawnWave() {
        while (true) {
            int enemiesThisWave = Mathf.FloorToInt(5 * difficultyCurve.Evaluate(waveNumber / 10f));
            
            for (int i = 0; i < enemiesThisWave && enemiesSpawned < maxEnemies; i++) {
                SpawnEnemy();
                yield return new WaitForSeconds(0.5f);
            }
            
            waveNumber++;
            yield return new WaitForSeconds(spawnInterval);
        }
    }
    
    void SpawnEnemy() {
        if (spawnPoints.Length == 0) return;
        Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Instantiate(enemyPrefab, point.position, Quaternion.identity);
        enemiesSpawned++;
    }
    
    public void OnEnemyDeath() { enemiesSpawned--; }
}
