using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameObject magnetBoxPrefab;
    public static Color NormalColor = new Color(0.9838573f, 1f, 0.485849f, 1f);
    public static Color HighlightColor = new Color(1f, 1f, 1f, 1f);
    private Stack<GameState> undoStack = new Stack<GameState>();
    private Stack<GameState> redoStack = new Stack<GameState>();

    private int totalGoalsCovered = 0;
    private GameObject[] pressurePlates;

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
    }

    public void UpdateGoalCount(int value)
    {
        totalGoalsCovered += value;
        var goals = GameObject.FindGameObjectsWithTag("Goal");
        Debug.Log("Total Goals Covered: " + totalGoalsCovered + " out of " + pressurePlates.Length);
        
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
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        totalGoalsCovered = 0;
        UpdateGoalCount(0);
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
}

