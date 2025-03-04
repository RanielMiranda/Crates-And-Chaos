using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class ObjectSpawner : MonoBehaviour
{
    public GameObject Player;
    public GameObject Box;
    public GameObject Goal;
    public GameObject Wall;
    public GameObject EmberBox;
    public GameObject VoltBox;
    public GameObject FrostBox;
    public GameObject MagnetBox;
    public GameObject MetalBox;
    public GameObject PressurePlate;
    public Transform spawnPoint;

    private GameObject prefab;

    public void SetPrefab(GameObject objectName) {
        prefab = objectName;
        SpawnSelectedObject();
    }

    public void SpawnPlayer()
    {
        int playerCount = GameObject.FindGameObjectsWithTag("Player").Length;
        if (playerCount >= 1)
        {
            Debug.Log("You already have a player in the scene.");
            return;
        }        
        SetPrefab(Player);
    }

    public void SpawnBox()
    {
        SetPrefab(Box);
    }
    
    public void SpawnGoal()
    {
        int goalCount = GameObject.FindGameObjectsWithTag("Goal").Length;
        if (goalCount >= 1)
        {
            Debug.Log("You already have a goal in the scene.");
            return;
        }        
        SetPrefab(Goal);
    }

    public void SpawnWall()
    {
        SetPrefab(Wall);
    }

    public void SpawnEmberBox()
    {
        SetPrefab(EmberBox);
    }

    public void SpawnVoltBox()
    {
        SetPrefab(VoltBox);
    }

    public void SpawnFrostBox()
    {
        SetPrefab(FrostBox);
    }

    public void SpawnMagnetBox()
    {
        SetPrefab(MagnetBox);
    }

    public void SpawnMetalBox()
    {
        SetPrefab(MetalBox);
    }

    public void SpawnPressurePlate()
    {
        SetPrefab(PressurePlate);
    }

    public void SpawnSelectedObject()
    {
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
            Debug.LogError("Prefab not found");
        }
    }
}

