using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using SFB;
using TMPro;

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
    private bool hasDestroy; 

    private int totalGoalsCovered = 0;
    private GameObject[] pressurePlates;
    public static string SelectedLevelName;
    private LevelData level; 

    //Help UI
    private string[] shortcutPages;
    private int currentPageIndex = 0;
    public bool isWin = false;
    public GameObject helpUI;        
    public GameObject winScreenUI;  
    public GameObject gameplayUI;
    public GameObject menuCloseButton;  
    public TMP_Text ShortcutInfoText;    
    public TMP_Text levelNameText; 
    public TMP_Text levelClearedText;    
    

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            pressurePlates = GameObject.FindGameObjectsWithTag("Pressure Plate");
        }
        else
        {
            Destroy(gameObject);
        }
        if (!string.IsNullOrEmpty(SelectedLevelName))
        {
            SelectedLevelName = SelectedLevelName;
        }
        Reset();
    }

    public void UpdateGoalCount()
    {
        RecalculateGoalsCovered();
        bool winConditionMet = CheckWinCondition(); // Call the method and store the result
            GameObject goal = GameObject.FindGameObjectWithTag("Goal"); // Singular, finds the first match

            GateController gate = goal.GetComponent<GateController>();
            if (gate != null)
            {
                gate.IsOpen(winConditionMet); // Update the gate's state
            }
                 
    }
    
    void Start() {
        shortcutPages = new string[]
        {
            "Movement:\n" +
            "W / ↑ : Move Up\n" +
            "S / ↓ : Move Down\n" +
            "A / ← : Move Left\n" +
            "D / → : Move Right\n",

            "Shortcuts:\n" +
            "Z : Undo Current Move\n" +
            "X : Redo Previous Move\n" +
            "R : Reset Level"
        };    
        helpUI.SetActive(false);    
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
            if (box != null) boxPositions.Add(box.transform.position);
        }

        var elementalBoxPositions = new List<Vector3>();
        var elementalBoxes = GetAllElementalBoxes();
        foreach (var box in elementalBoxes)
        {
            if (box != null) elementalBoxPositions.Add(box.transform.position);
        }

        var metalBoxPositions = new List<Vector3>();
        metalBoxes.RemoveAll(box => box == null); // Remove destroyed references
        foreach (var box in metalBoxes)
        {
            if (box != null) metalBoxPositions.Add(box.transform.position);
        }

        state.boxPositions = boxPositions;
        state.elementalBoxPositions = elementalBoxPositions;
        state.metalBoxPositions = metalBoxPositions;

        undoStack.Push(state);
        redoStack.Clear();
    }

    public void Undo()
    {
        if (undoStack.Count > 0)
        {   
            AudioManager.Instance.PlayButtonSound();
            //push to redo
            metalBoxes.RemoveAll(box => box == null);
            GameState currentState = new GameState
            {
                playerPosition = GameObject.FindWithTag("Player").transform.position,
                playerRotation = GameObject.FindWithTag("Player").transform.rotation,
                boxPositions = new List<Vector3>(),
                elementalBoxPositions = new List<Vector3>(),
                metalBoxPositions = new List<Vector3>()
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

            foreach (var box in metalBoxes)
            {
                currentState.metalBoxPositions.Add(box.transform.position);
            }

            redoStack.Push(currentState);

            //push to undo stack
            GameState lastState = undoStack.Pop();
            GameObject.FindWithTag("Player").transform.position = lastState.playerPosition;
            GameObject.FindWithTag("Player").transform.rotation = lastState.playerRotation;

            for (int i = 0; i < boxes.Length; i++)
            {
                boxes[i].transform.position = lastState.boxPositions[i];
            }

            for (int i = 0; i < elementalBoxes.Count; i++)
            {
                if (i < lastState.elementalBoxPositions.Count)
                {
                    elementalBoxes[i].transform.position = lastState.elementalBoxPositions[i];
                }
                else
                {
                    elementalBoxes[i].transform.position += Vector3.down * 10;
                }
            }

            for (int i = 0; i < metalBoxes.Count; i++)
            {
                metalBoxes[i].transform.position = lastState.metalBoxPositions[i];
            }
        }
    }

    public void Redo()
    {
        if (redoStack.Count > 0)
        {
            AudioManager.Instance.PlayButtonSound();
            metalBoxes.RemoveAll(box => box == null);
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

            for (int i = 0; i < metalBoxes.Count; i++)
            {
                metalBoxes[i].transform.position = redoState.metalBoxPositions[i];
            }
        }
    }

    public void Reset()
    {
        Debug.Log("Resetting");
        isWin = false;
        AudioManager.Instance.PlayResetSound();
        StartCoroutine(ResetAfterLoad());
    }

    public void Cheat()
    {
        totalGoalsCovered = pressurePlates.Length;
        Debug.Log("Cheat is working");
        UpdateGoalCount();
    }
    private IEnumerator ResetAfterLoad()
    {
        LoadLevel();
        totalGoalsCovered = 0;

        // Wait for the level to fully load
        yield return new WaitForSeconds(0.5f); 
        pressurePlates = GameObject.FindGameObjectsWithTag("Pressure Plate");
        CacheMetalAndMagnetBoxes();      
        RecalculateGoalsCovered();          
        SaveState();
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
        public List<Vector3> metalBoxPositions;
    }

    public void LoadLevel()
    {  
        string levelName = SelectedLevelName;
        if (!string.IsNullOrEmpty(levelName))
        {
            string json = null;
            // Try loading from Resources/Levels
            TextAsset levelAsset = Resources.Load<TextAsset>("Levels/" + levelName);
            string customLevelPath = Path.Combine(Application.persistentDataPath, "CustomLevels", levelName + ".json");

            if (levelAsset != null)
            {
                json = levelAsset.text;
                Debug.Log("Loading level from Resources: " + levelName);
            }
            else if (File.Exists(customLevelPath))
            {
                try
                {
                    json = File.ReadAllText(customLevelPath);
                    Debug.Log("Loading level from persistent path: " + customLevelPath);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to read custom level file {customLevelPath}: {e.Message}");
                    return;
                }
            }
            else
            {
                Debug.LogError("Level not found in Resources/Levels or persistent path: " + levelName);
                return;
            }

            try
            {
                // Deserialize JSON
                level = JsonUtility.FromJson<LevelData>(json);
                Debug.Log("Loaded JSON: " + json);

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
                    GameObject prefab = Resources.Load<GameObject>("Prefabs/" + obj.type);
                    if (prefab != null)
                    {
                        GameObject instance = Instantiate(prefab, new Vector3(obj.position[0], obj.position[1], obj.position[2]), Quaternion.identity);
                        instance.transform.localScale = new Vector3(obj.scale[0], obj.scale[1], obj.scale[2]);
                    }
                    else
                    {
                        Debug.LogWarning("Prefab not found for type: " + obj.type);
                    }
                }

                Debug.Log("Level Loaded: " + levelName);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error loading level: " + e.Message);
            }
        }
        else
        {
            Debug.LogError("No level selected!");
        }
    }

    public void LoadOtherLevel()
    {
        Debug.Log("Loading Other Level");
        Reset();
    }

    public void CacheMetalAndMagnetBoxes()
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

    private void RecalculateGoalsCovered()
    {
        totalGoalsCovered = 0;
        foreach (GameObject plate in pressurePlates)
        {
            Collider[] colliders = Physics.OverlapBox(plate.transform.position, plate.transform.localScale / 2);
            foreach (Collider col in colliders)
            {
                if (col.CompareTag("Box") || col.CompareTag("Ember Box") || col.CompareTag("Volt Box") || col.CompareTag("Frost Box") || col.CompareTag("Magnet Box") || col.CompareTag("Metal Box"))
                {                  
                    totalGoalsCovered++;
                }
            }
        }
        Debug.Log($"Recalculated: {totalGoalsCovered}/{pressurePlates.Length}");
    }

    public void NextHelpPage()
    {
        if (currentPageIndex == shortcutPages.Length - 1) return;
        AudioManager.Instance.PlayButtonSound();
        currentPageIndex += 1;
        ShortcutInfoText.text = shortcutPages[currentPageIndex]; 
    }

    public void PrevHelpPage()
    {
        if (currentPageIndex == 0) return;
        AudioManager.Instance.PlayButtonSound();
        currentPageIndex -= 1;
        ShortcutInfoText.text = shortcutPages[currentPageIndex]; 
    }

    public void ToggleHelp()
    {
        currentPageIndex = 0;
        AudioManager.Instance.PlayButtonSound();
        if (helpUI.activeSelf)
        {
            helpUI.SetActive(false);
        }
        else
        {
            helpUI.SetActive(true);
        }
    }

    public void winReset() {
        Reset();        
        winScreenUI.SetActive(false);
        gameplayUI.SetActive(true);
    }

    public void toggleWinScreenUI() {
        if (!CheckWinCondition()) 
        {
            AudioManager.Instance.PlayButtonSound();                        
            levelClearedText.text = "Pause Menu";
            menuCloseButton.SetActive(true);         
        }
        if (CheckWinCondition() && isWin) 
        {
            AudioManager.Instance.PlayWinSound();
            levelClearedText.text = "Level Cleared!";
            menuCloseButton.SetActive(false);
        } 
        Debug.Log(level.levelName.ToString());
        levelNameText.text = level.levelName;
        winScreenUI.SetActive(!winScreenUI.activeSelf);
        gameplayUI.SetActive(!gameplayUI.activeSelf);
    }
}