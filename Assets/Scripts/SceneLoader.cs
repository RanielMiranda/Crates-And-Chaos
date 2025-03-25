using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void LoadLevelEditor()
    {
        SceneManager.LoadScene("LevelEditor");
    }

    public void LoadGameLevel()
    {
        SceneManager.LoadScene("GameLevel");
    }
    public void LoadLevelSelector()
    {
        SceneManager.LoadScene("LevelSelector");
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
