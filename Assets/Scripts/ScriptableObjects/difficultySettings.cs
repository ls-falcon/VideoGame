using UnityEngine;

[CreateAssetMenu(
    fileName = "NewDifficultySettings",
    menuName = "Game/Difficulty Settings"
)]
public class DifficultySettings : ScriptableObject
{
    [Header("Player")]
    public int maxInitialHearts;

    [Header("Game")]
    public int numberOfRounds;

    [Header("Waves")]
    public WaveData[] waves;
}