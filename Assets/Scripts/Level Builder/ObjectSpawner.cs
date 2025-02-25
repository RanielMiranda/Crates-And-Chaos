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
            if (prefab.name == "Player")
            {
                y = .25f;
            }
            else if (prefab.name == "Wall" || prefab.name == "Goal")
            {
                y = .5f;
            }
            else if (prefab.name == "Pressure Plate")
            {
                y = 0.05f;
            }
            else
            {
                y = 0.5f;
            }

            spawnPoint.position = new Vector3(spawnPoint.position.x, y, spawnPoint.position.z);  
            GameObject instance = Instantiate(prefab, spawnPoint.position, Quaternion.identity);  
                                            
        }
        else
        {
            Debug.LogError("Prefab not found: " + selected);
        }
    }
}
