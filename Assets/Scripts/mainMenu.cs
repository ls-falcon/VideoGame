using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Difficulty Assets")]
    [SerializeField] private DifficultySettings easyDifficulty;
    [SerializeField] private DifficultySettings mediumDifficulty;
    [SerializeField] private DifficultySettings hardDifficulty;

    public GameObject mainMenu;
    public GameObject difficultyMenu;
       
    public void QuitGame()
    {
        Application.Quit();
    }

    public void PlayEasy()
    {
        GameManager.Instance.SetDifficulty(easyDifficulty);
        SceneManager.LoadScene("Game");
    }

    public void PlayMedium()
    {
        GameManager.Instance.SetDifficulty(mediumDifficulty);
        SceneManager.LoadScene("Game");
    }

    public void PlayHard()
    {
        GameManager.Instance.SetDifficulty(hardDifficulty);
        SceneManager.LoadScene("Game");
    }

    public void OpenMainMenu()
    {
        difficultyMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    public void OpenDifficultyMenu()
    {
        mainMenu.SetActive(false);
        difficultyMenu.SetActive(true);
    }
}
