using UnityEngine;

[CreateAssetMenu(
    fileName = "NewEnemyData",
    menuName = "Game/Enemy Data"
)]
public class EnemyData : ScriptableObject
{
    public string enemyName;

    public GameObject enemyPrefab;
}
