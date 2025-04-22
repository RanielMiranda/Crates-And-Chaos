using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using SFB;


public class LevelSelector : MonoBehaviour
{
    public GameObject buttonPrefab;
    public Transform gridParent;
    private int levelCount = 1;

    private void Start()
    {
        LoadLevelButtons();
    }

    void LoadLevelButtons()
    {
        string[] levels = GetLevelsFromFolder();

        foreach (string levelName in levels)
        {
            GameObject buttonObj = Instantiate(buttonPrefab, gridParent);
            Button button = buttonObj.GetComponent<Button>();
            button.onClick.AddListener(() => LoadLevel(levelName));

            TMP_Text buttonText = buttonObj.GetComponentInChildren<TMP_Text>();
            if (buttonText != null) buttonText.text = levelCount.ToString();
            levelCount++;
        }
    }

    string[] GetLevelsFromFolder()
    {
        TextAsset[] levelFiles = Resources.LoadAll<TextAsset>("Levels");
        string[] levelNames = new string[levelFiles.Length];

        for (int i = 0; i < levelFiles.Length; i++)
        {
            levelNames[i] = levelFiles[i].name;
        }

        return levelNames;
    }

    void LoadLevel(string levelName)
    {
        // Set the path in GameManager before loading the scene
        GameManager.SelectedLevelName = levelName; // Store the name only, not the full path
        Debug.Log("Selected Level: " + levelName);
        SceneManager.LoadScene("GameLevel");
    }

void LoadCustomLevel()
    {
        // Open file browser to select a JSON level file
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Select Custom Level File", "", "json", false);
        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            string sourcePath = paths[0];
            string fileName = Path.GetFileName(sourcePath);
            string destinationPath = Application.persistentDataPath + "/CustomLevels/" + fileName;

            try
            {
                // Ensure the CustomLevels directory exists
                string customLevelsPath = Application.persistentDataPath + "/CustomLevels/";
                if (!Directory.Exists(customLevelsPath))
                {
                    Directory.CreateDirectory(customLevelsPath);
                }

                // Copy the file to persistent data path
                File.Copy(sourcePath, destinationPath, true);
                Debug.Log($"Custom level copied to: {destinationPath}");

                // Load the level
                string levelName = Path.GetFileNameWithoutExtension(fileName);
                GameManager.SelectedLevelName = levelName;
                Debug.Log("Selected Level: " + levelName);
                SceneManager.LoadScene("GameLevel");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to copy custom level file: {e.Message}");
            }
        }
    }

}