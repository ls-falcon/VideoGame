using UnityEngine;
using UnityEngine.SceneManagement;

public class mainMenu : MonoBehaviour
{
    [Header("Difficulty Assets")]
    [SerializeField] private DifficultySettings easyDifficulty;
    [SerializeField] private DifficultySettings mediumDifficulty;
    [SerializeField] private DifficultySettings hardDifficulty;

    public GameObject MainMenu;
    public GameObject DifficultyMenu;
       
    public void QuitGame()
    {
        Application.Quit();
    }

    public void PlayEasy()
    {
        gameManager.Instance.SetDifficulty(easyDifficulty);
        SceneManager.LoadScene("Game");
    }

    public void PlayMedium()
    {
        gameManager.Instance.SetDifficulty(mediumDifficulty);
        SceneManager.LoadScene("Game");
    }

    public void PlayHard()
    {
        gameManager.Instance.SetDifficulty(hardDifficulty);
        SceneManager.LoadScene("Game");
    }

    public void OpenMainMenu()
    {
        DifficultyMenu.SetActive(false);
        MainMenu.SetActive(true);
    }

    public void OpenDifficultyMenu()
    {
        MainMenu.SetActive(false);
        DifficultyMenu.SetActive(true);
    }
}
