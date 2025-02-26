using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameObject magnetBoxPrefab;
    public static Color NormalColor = new Color(0.9838573f, 1f, 0.485849f, 1f);
    public static Color HighlightColor = new Color(1f, 1f, 1f, 1f);
    private Stack<GameState> undoStack = new Stack<GameState>();
    private Stack<GameState> redoStack = new Stack<GameState>();

    private List<MetalBoxController> metalBoxes = new List<MetalBoxController>();
    private List<ElementalBoxController> magnetBoxes = new List<ElementalBoxController>();    

    //Audios
    public AudioClip MoveBox;
    public AudioClip MovePlayer;

    private int totalGoalsCovered = 0;
    private GameObject[] pressurePlates;
    private string path;

    private void Awake()
    {
        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
            pressurePlates = GameObject.FindGameObjectsWithTag("Pressure Plate");
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
        path = Application.persistentDataPath + "/Levels/";
        Reset();      
    }

    public void UpdateGoalCount(int value)
    {
        totalGoalsCovered += value;
        var goals = GameObject.FindGameObjectsWithTag("Goal");
        
        bool allGoalsCovered = totalGoalsCovered == pressurePlates.Length;

        foreach (var goal in goals)
        {
            goal.GetComponent<GateController>().IsOpen(allGoalsCovered);
        }
    }

    public bool CheckWinCondition()
    {
        return totalGoalsCovered == pressurePlates.Length;
    }

    public void SaveState()
    {
        GameState state = new GameState();
        state.playerPosition = GameObject.FindWithTag("Player").transform.position;
        state.playerRotation = GameObject.FindWithTag("Player").transform.rotation;

        var boxPositions = new List<Vector3>();
        var boxes = GameObject.FindGameObjectsWithTag("Box");
        foreach (var box in boxes)
        {
            boxPositions.Add(box.transform.position);
        }

        var elementalBoxPositions = new List<Vector3>();
        var elementalBoxes = GetAllElementalBoxes();
        foreach (var box in elementalBoxes)
        {
            elementalBoxPositions.Add(box.transform.position);
        }

        state.boxPositions = boxPositions;
        state.elementalBoxPositions = elementalBoxPositions;

        undoStack.Push(state);
        redoStack.Clear();
    }

    public void Undo()
    {
        if (undoStack.Count > 0)
        {
            GameState currentState = new GameState
            {
                playerPosition = GameObject.FindWithTag("Player").transform.position,
                playerRotation = GameObject.FindWithTag("Player").transform.rotation,
                boxPositions = new List<Vector3>(),
                elementalBoxPositions = new List<Vector3>()
            };

            var boxes = GameObject.FindGameObjectsWithTag("Box");
            foreach (var box in boxes)
            {
                currentState.boxPositions.Add(box.transform.position);
            }

            var elementalBoxes = GetAllElementalBoxes();
            foreach (var box in elementalBoxes)
            {
                currentState.elementalBoxPositions.Add(box.transform.position);
            }

            redoStack.Push(currentState);

            GameState lastState = undoStack.Pop();
            GameObject.FindWithTag("Player").transform.position = lastState.playerPosition;
            GameObject.FindWithTag("Player").transform.rotation = lastState.playerRotation;

            for (int i = 0; i < boxes.Length; i++)
            {
                boxes[i].transform.position = lastState.boxPositions[i];
            }

            for (int i = 0; i < elementalBoxes.Count; i++)
            {
                elementalBoxes[i].transform.position = lastState.elementalBoxPositions[i];
            }
        }
    }

    public void Redo()
    {
        if (redoStack.Count > 0)
        {
            GameState redoState = redoStack.Pop();
            undoStack.Push(redoState);
            var player = GameObject.FindWithTag("Player");
            player.transform.position = redoState.playerPosition;
            player.transform.rotation = redoState.playerRotation;

            var boxes = GameObject.FindGameObjectsWithTag("Box");
            for (int i = 0; i < boxes.Length; i++)
            {
                boxes[i].transform.position = redoState.boxPositions[i];
            }

            var elementalBoxes = GetAllElementalBoxes();
            for (int i = 0; i < elementalBoxes.Count; i++)
            {
                elementalBoxes[i].transform.position = redoState.elementalBoxPositions[i];
            }
        }
    }

    public void Reset()
    {
        Debug.Log("Resetting");
        StartCoroutine(ResetAfterLoad());
    }

    private IEnumerator ResetAfterLoad()
    {
        LoadLevel("Test Level");

        // Wait for the level to fully load
        yield return new WaitForSeconds(0.1f); 

        totalGoalsCovered = 0;
        UpdateGoalCount(0);      
        CacheMetalAndMagnetBoxes();           
    }

    public void Cheat()
    {
        totalGoalsCovered = pressurePlates.Length;
        UpdateGoalCount(0);
        Debug.Log("Cheat is working");
    }

    private List<GameObject> GetAllElementalBoxes()
    {
        var elementalBoxes = new List<GameObject>();
        elementalBoxes.AddRange(GameObject.FindGameObjectsWithTag("Ember Box"));
        elementalBoxes.AddRange(GameObject.FindGameObjectsWithTag("Volt Box"));
        elementalBoxes.AddRange(GameObject.FindGameObjectsWithTag("Frost Box"));
        elementalBoxes.AddRange(GameObject.FindGameObjectsWithTag("Magnet Box"));
        return elementalBoxes;
    }

    public struct GameState
    {
        public Vector3 playerPosition;
        public Quaternion playerRotation;
        public List<Vector3> boxPositions;
        public List<Vector3> elementalBoxPositions;
    }

    public void LoadLevel(string fileName)
    {
        string filePath = path + fileName + ".json";
        if (!File.Exists(filePath))
        {
            Debug.LogError(Application.persistentDataPath + "/Levels/");
            Debug.LogError("File not found: " + filePath);
            return;
        }

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

        // Rebuild the level
        foreach (LevelObject obj in level.objects)
        {
            GameObject prefab = Resources.Load<GameObject>("Prefabs/" + obj.type);
            if (prefab != null)
            {
                GameObject instance = Instantiate(prefab, new Vector3(obj.position[0], obj.position[1], obj.position[2]), Quaternion.identity);
                instance.transform.localScale = new Vector3(obj.scale[0], obj.scale[1], obj.scale[2]);
            }
        }
        pressurePlates = GameObject.FindGameObjectsWithTag("Pressure Plate");
        Debug.Log("Level Loaded: " + fileName);

    }

    private void CacheMetalAndMagnetBoxes()
    {
        Debug.Log("Caching Metal and Magnet Boxes");
        metalBoxes.Clear();
        magnetBoxes.Clear();

        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Metal Box"))
        {
            var metalBox = obj.GetComponent<MetalBoxController>();
            if (metalBox != null) metalBoxes.Add(metalBox);
        }

        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Magnet Box"))
        {
            var magnetBox = obj.GetComponent<ElementalBoxController>();
            if (magnetBox != null) magnetBoxes.Add(magnetBox);
        }
    }

    public void UpdateMetalBoxes()
    {
        foreach (var metalBox in metalBoxes)
        {
            metalBox.MoveTowardsMagnetBox(magnetBoxes);
        }
    }
    public List<MetalBoxController> GetMetalBoxes()
    {
        return metalBoxes;
    }

    public List<ElementalBoxController> GetMagnetBoxes()
    {
        return magnetBoxes;
    }

}