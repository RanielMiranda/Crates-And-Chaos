using UnityEngine;

public class GridSpawner : MonoBehaviour
{
    public GameObject boxPrefab;  // Assign your Box prefab
    public int gridSizeX = 10;  // Width of the grid
    public int gridSizeY = 10;  // Height of the grid
    public float spacing = 1.1f; // Space between boxes

    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 spawnPosition = new Vector3(x * spacing, 0, y * spacing);
                Instantiate(boxPrefab, spawnPosition, Quaternion.identity);
            }
        }
    }
}