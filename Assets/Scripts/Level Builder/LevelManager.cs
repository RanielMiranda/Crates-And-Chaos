using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
using SFB;

public class LevelManager : MonoBehaviour
{
    public string levelName = "Default Level"; // Default level name
    public int gridX = 10, gridY = 10, gridZ = 5; // Default grid size
    
    private string path;
    
    public TMP_InputField levelNameInputField; // Reference to the input field
    public TMP_Text selectedObjectText; // UI text for selected object name
    public TMP_Text objectInfoText; // UI text for object information
    public TMP_Text ShortcutInfoText;
    public GameObject helpUI; 

    private string[] shortcutPages;
    private int currentPageIndex = 0;

    void Start()
    {
        path = Application.persistentDataPath + "/Levels/";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        levelNameInputField.onValueChanged.AddListener(UpdateTitleText);
        helpUI.SetActive(false);

        shortcutPages = new string[]
        {
            "Movement:\n" +
            "W / ↑ : Move Forward\n" +
            "S / ↓ : Move Backward\n" +
            "A / ← : Move Left\n" +
            "D / → : Move Right\n" +
            "R : Move Up\n" +
            "F : Move Down",

            "Scaling:\n" +
            "Q : Increase X Scale\n" +
            "E : Increase Z Scale\n" +
            "Z : Decrease X Scale\n" +
            "C : Decrease Z Scale\n" +
            "Delete : Delete Selected"
        };
    }

    void UpdateTitleText(string input)
    {
        // selectedTitleText.text doesn't exist
        levelName = input;
    }    

    public void SaveLevel()
    {
        Debug.Log("Saving level: " + levelNameInputField.text);
        LevelData level = new LevelData
        {
            levelName = levelNameInputField.text,
            gridSize = new int[] { gridX, gridY, gridZ },
            objects = new List<LevelObject>()
        };
        Debug.Log("LevelData level's levelName: " + level.levelName);

        // Check if there is a player
        if (GameObject.FindGameObjectsWithTag("Player").Length == 0 || GameObject.FindGameObjectsWithTag("Goal").Length == 0)
        {
            Debug.LogError("No player or goal in the scene, level not saved.");
            return;
        }

        // Collect objects based on known types instead of a single tag
        string[] objectTags = { "Wall", "Box", "Goal", "Pressure Plate", "Player", "Ember Box", "Volt Box", "Frost Box", "Magnet Box", "Metal Box"};

        foreach (string tag in objectTags)
        {
            GameObject[] foundObjects = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject obj in foundObjects)
            {
                level.objects.Add(new LevelObject(
                    obj.tag, 
                    obj.transform.position.x, 
                    obj.transform.position.y, 
                    obj.transform.position.z,
                    obj.transform.localScale.x,
                    obj.transform.localScale.y,
                    obj.transform.localScale.z
                ));
            }
        }

        // Convert to JSON and save
        string json = JsonUtility.ToJson(level, true);
        File.WriteAllText(path + levelName + ".json", json);
        Debug.Log("Level saved: " + path + levelName + ".json");
    }

    public void LoadLevel()
    {
        // Open the file dialog to let the user select a file
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Select Level", "", "json", true);
        
        if (paths.Length > 0)
        {
            string filePath = paths[0];  // Get the selected file path

            if (!File.Exists(filePath))
            {
                Debug.LogError("File does not exist: " + filePath);
                return;
            }

            try
            {
                // Read the JSON file content
                string json = File.ReadAllText(filePath);
                LevelData level = JsonUtility.FromJson<LevelData>(json);

                // Clear current scene objects
                string[] objectTags = { "Wall", "Box", "Goal", "Pressure Plate", "Player", "Ember Box", "Volt Box", "Frost Box", "Magnet Box", "Metal Box" };
                foreach (string tag in objectTags)
                {
                    foreach (GameObject obj in GameObject.FindGameObjectsWithTag(tag))
                    {
                        Destroy(obj);
                    }
                }

                // Rebuild the level from the loaded data
                foreach (LevelObject obj in level.objects)
                {
                    // Load the prefab corresponding to the object type
                    GameObject prefab = Resources.Load<GameObject>("Prefabs/" + obj.type);
                    if (prefab != null)
                    {
                        // Instantiate the prefab at the correct position and scale
                        GameObject instance = Instantiate(prefab, new Vector3(obj.position[0], obj.position[1], obj.position[2]), Quaternion.identity);
                        instance.transform.localScale = new Vector3(obj.scale[0], obj.scale[1], obj.scale[2]);
                    }
                    else
                    {
                        Debug.LogWarning("Prefab not found for type: " + obj.type);
                    }
                }
                levelNameInputField.text = Path.GetFileNameWithoutExtension(filePath);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error loading level: " + e.Message);
            }
        }
        else
        {
            Debug.Log("No file selected.");
        }
    }

    public void UpdateSelectedObject(GameObject obj)
    {
        if (obj == null)
        {
            selectedObjectText.text = "No Object";
            objectInfoText.text = $"Position X: None\nPosition Y: None\nPosition Z: None\n\n" +
                                  $"Scale X: None\nScale Y: None\nScale Z: None";
            return;
        }

        selectedObjectText.text = obj.name;

        Vector3 pos = obj.transform.position;
        Vector3 scale = obj.transform.localScale;

        objectInfoText.text = $"Position X: {pos.x:F2}\nPosition Y: {pos.y:F2}\nPosition Z: {pos.z:F2}\n\n" +
                              $"Scale X: {scale.x:F2}\nScale Y: {scale.y:F2}\nScale Z: {scale.z:F2}";
    }

    public void ToggleHelp()
    {
        currentPageIndex = 0;
        if (helpUI.activeSelf)
        {
            helpUI.SetActive(false);
        }
        else
        {
            helpUI.SetActive(true);
        }
    }

    public void UpdateTextNextPage()
    {
        if (currentPageIndex == shortcutPages.Length - 1) return;

        currentPageIndex += 1;
        ShortcutInfoText.text = shortcutPages[currentPageIndex]; 
    }

    public void UpdateTextPreviousPage()
    {
        if (currentPageIndex == 0) return;

        currentPageIndex -= 1;
        ShortcutInfoText.text = shortcutPages[currentPageIndex]; 
    }

}

