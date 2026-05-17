using UnityEngine;

[System.Serializable]
public class WaveEnemyEntry
{
    public EnemyData enemyData;

    [Range(0f, 1f)]
    public float spawnWeight = 1f;
}