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

        var boxPositions = new List<Vector3>();
        var boxes = GameObject.FindGameObjectsWithTag("Box");
        foreach (var box in boxes)
        {
            boxPositions.Add(box.transform.position);
        }

        state.boxPositions = boxPositions;

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
                boxPositions = new List<Vector3>()
            };

            var boxes = GameObject.FindGameObjectsWithTag("Box");
            foreach (var box in boxes)
            {
                currentState.boxPositions.Add(box.transform.position);
            }


            redoStack.Push(currentState);

            GameState lastState = undoStack.Pop();
            GameObject.FindWithTag("Player").transform.position = lastState.playerPosition;

            for (int i = 0; i < boxes.Length; i++)
            {
                boxes[i].transform.position = lastState.boxPositions[i];
            }

        }
    }

    public void Redo()
    {
        if (redoStack.Count > 0)
        {
            GameState redoState = redoStack.Pop();
            undoStack.Push(redoState);

            GameObject.FindWithTag("Player").transform.position = redoState.playerPosition;

            var boxes = GameObject.FindGameObjectsWithTag("Box");
            for (int i = 0; i < boxes.Length; i++)
            {
                boxes[i].transform.position = redoState.boxPositions[i];
            }
        }
    }

    public void Reset()
    {
        Debug.Log("Resetting");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Cheat()
    {
        totalGoalsCovered = pressurePlates.Length;
        UpdateGoalCount(0);
        Debug.Log("Cheat is working");
    }

    public struct GameState
    {
        public Vector3 playerPosition;
        public List<Vector3> boxPositions;

    }
}

