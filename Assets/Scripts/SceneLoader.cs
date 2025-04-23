using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadMainMenu()
    {
        AudioManager.Instance.PlayButtonSound();        
        SceneManager.LoadScene("MainMenu");
    }

    public void LoadLevelEditor()
    {
        AudioManager.Instance.PlayButtonSound();        
        SceneManager.LoadScene("LevelEditor");
    }

    public void LoadGameLevel()
    {
        AudioManager.Instance.PlayButtonSound();        
        SceneManager.LoadScene("GameLevel");
    }
    public void LoadLevelSelector()
    {
        AudioManager.Instance.PlayButtonSound();
        SceneManager.LoadScene("LevelSelector");
    }
    public void QuitGame()
    {
        AudioManager.Instance.PlayButtonSound();
        Application.Quit();
    }
}
