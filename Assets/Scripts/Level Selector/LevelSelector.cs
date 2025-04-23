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
        GameManager.SelectedLevelName = levelName;
        Debug.Log("Selected Level: " + levelName);
        SceneManager.LoadScene("GameLevel");
    }

    void LoadCustomLevel()
    {
        // Define the initial directory for the file browser
        string initialPath = Application.persistentDataPath + "/CustomLevels/";

        // Ensure the CustomLevels directory exists
        if (!Directory.Exists(initialPath))
        {
            Directory.CreateDirectory(initialPath);
        }

        // Open file browser starting at the CustomLevels folder
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Select Custom Level File", initialPath, "json", false);
        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            string sourcePath = paths[0];
            string fileName = Path.GetFileName(sourcePath);
            string destinationPath = Path.Combine(Application.persistentDataPath, "CustomLevels", fileName);

            try
            {
                // Check if the source is already in the destination
                if (sourcePath != destinationPath)
                {
                    // Copy the file only if it’s different or has changed
                    if (File.Exists(destinationPath))
                    {
                        // Optionally, compare file contents to avoid overwriting unchanged files
                        string sourceContent = File.ReadAllText(sourcePath);
                        string destContent = File.ReadAllText(destinationPath);
                        if (sourceContent == destContent)
                        {
                            Debug.Log($"File {fileName} is identical, skipping copy.");
                        }
                        else
                        {
                            File.Copy(sourcePath, destinationPath, true);
                            Debug.Log($"Custom level copied to: {destinationPath}");
                        }
                    }
                    else
                    {
                        File.Copy(sourcePath, destinationPath, true);
                        Debug.Log($"Custom level copied to: {destinationPath}");
                    }
                }
                else
                {
                    Debug.Log($"Selected file is already in CustomLevels: {destinationPath}");
                }

                // Load the level
                string levelName = Path.GetFileNameWithoutExtension(fileName);
                GameManager.SelectedLevelName = levelName;
                Debug.Log("Selected Level: " + levelName);
                SceneManager.LoadScene("GameLevel");
            }
            catch (IOException e)
            {
                Debug.LogError($"IOException: Failed to copy custom level file: {e.Message}");
                // Optionally, notify the user via UI
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Unexpected error: Failed to copy custom level file: {e.Message}");
            }
        }
    }
}