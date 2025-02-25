using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public string levelName = "MyNewLevel"; // Default level name
    public int gridX = 10, gridY = 10, gridZ = 5; // Default grid size
    
    private string path;

    void Start()
    {
        path = Application.persistentDataPath + "/Levels/";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    public void SaveLevel()
    {
        LevelData level = new LevelData
        {
            levelName = levelName,
            gridSize = new int[] { gridX, gridY, gridZ },
            objects = new List<LevelObject>()
        };

        // Find all objects tagged with "Wall", "Box", etc.
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("LevelObject"))
        {
            level.objects.Add(new LevelObject(
                obj.tag, 
                obj.transform.position.x, 
                obj.transform.position.y, 
                obj.transform.position.z
            ));
        }

        // Convert to JSON and save
        string json = JsonUtility.ToJson(level, true);
        File.WriteAllText(path + levelName + ".json", json);
        Debug.Log("Level saved: " + path + levelName + ".json");
    }

    public void LoadLevel(string fileName)
    {
        string filePath = path + fileName + ".json";
        if (!File.Exists(filePath))
        {
            Debug.LogError("File not found: " + filePath);
            return;
        }

        string json = File.ReadAllText(filePath);
        LevelData level = JsonUtility.FromJson<LevelData>(json);
        
        // Clear current scene objects
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("LevelObject"))
        {
            Destroy(obj);
        }

        // Rebuild the level
        foreach (LevelObject obj in level.objects)
        {
            GameObject prefab = Resources.Load<GameObject>("Prefabs/" + obj.type);
            if (prefab != null)
            {
                Instantiate(prefab, new Vector3(obj.position[0], obj.position[1], obj.position[2]), Quaternion.identity);
            }
        }

        Debug.Log("Level Loaded: " + fileName);
    }
}

