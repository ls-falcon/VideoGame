using UnityEngine;

public class gameManager : MonoBehaviour
{
    public static gameManager Instance;

    public DifficultySettings currentDifficulty {  get; private set; }

    private void Awake()
    {
        //Singleton
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public void SetDifficulty(DifficultySettings difficulty)
    {
        currentDifficulty = difficulty;
    }
}
