using UnityEngine;
using TMPro;

public class ObjectSpawner : MonoBehaviour
{
    public TMP_Dropdown objectDropdown;
    public Transform spawnPoint;

    public void SpawnSelectedObject()
    {
        string selected = objectDropdown.options[objectDropdown.value].text;
        GameObject prefab = Resources.Load<GameObject>("Prefabs/" + selected);

        // Set the y position based on the prefab
        float y;
        if (prefab != null)
        {
            switch (prefab.name)
            {
                case "Player":
                    y = .25f;
                    break;
                case "Wall":
                case "Goal":
                    y = .5f;
                    break;                
                case "Pressure Plate":
                    y = 0.05f;
                    break;
                default:
                    y = spawnPoint.position.y;
                    break;
            }
        }
        else
        {
            y = spawnPoint.position.y;
        }
        spawnPoint.position = new Vector3(spawnPoint.position.x, y, spawnPoint.position.z);


        if (prefab != null)
        {
            GameObject instance = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
        }
        else
        {
            Debug.LogError("Prefab not found: " + selected);
        }
    }
}
