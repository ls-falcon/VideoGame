using UnityEngine;

[CreateAssetMenu(
    fileName = "NewWaveData", 
    menuName = "Game/Wave Data"
)]
public class WaveData : ScriptableObject
{
    [Header("Wave Settings")]
    public int enemyCount = 10;

    public float spawnRate = 1f;

    [Header("Enemies")]
    public WaveEnemyEntry[] enemies;
}
