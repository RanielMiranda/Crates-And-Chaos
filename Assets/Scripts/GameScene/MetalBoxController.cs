using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MetalBoxController : MonoBehaviour
{
    private float moveSpeed = 5f;
    private bool isMoving = false;
    private float magnetRange = 10f; // Max distance for magnet effect
    public LayerMask blockingLayer;
    public LayerMask defaultLayer;
    private Vector3 futurePosition;
    private Material boxMaterial;

    private void Start() {
        boxMaterial = GetComponent<Renderer>().material;

        //Level Editor
        if (GameObject.FindFirstObjectByType<LevelManager>() != null)
        {
            Destroy(this);
        }              
    }    

    public void MoveTowardsMagnetBox(List<ElementalBoxController> magnetBoxes)
    {
        if (isMoving || magnetBoxes.Count == 0) return;

        ElementalBoxController nearestMagnet = FindNearestMagnetBox(magnetBoxes);
        if (nearestMagnet == null) return;

        float distanceToMagnet = Vector3.Distance(transform.position, nearestMagnet.transform.position);
        if (distanceToMagnet <= magnetRange)
        {
            Vector3 directionToMagnet = (nearestMagnet.transform.position - transform.position).normalized;
            Vector3 targetPosition = transform.position + directionToMagnet; // Move by 1 unit
            
            // Check if there is a wall in the way
            if (Physics.Raycast(transform.position, directionToMagnet, out RaycastHit hit, 1f, blockingLayer))
            {
                return;
            }

            // Check if there is a player in the way
            if (Physics.Raycast(transform.position, directionToMagnet, out RaycastHit hit2, 2f, defaultLayer))
            {
                return;
            }
            StartCoroutine(MoveToPosition(targetPosition));
            futurePosition = targetPosition + directionToMagnet;
        }
    }

    public Vector3 GetFuturePosition()
    {
        return futurePosition;
    }

    private ElementalBoxController FindNearestMagnetBox(List<ElementalBoxController> magnetBoxes)
    {
        ElementalBoxController nearest = null;
        float minDistance = float.MaxValue;

        foreach (var magnet in magnetBoxes)
        {
            if (magnet.transform.position.x == transform.position.x)
            {
                float distance = Mathf.Abs(magnet.transform.position.z - transform.position.z);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = magnet;
                }
            }
            else if (magnet.transform.position.z == transform.position.z)
            {
                float distance = Mathf.Abs(magnet.transform.position.x - transform.position.x);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = magnet;
                }
            }
        }
        return nearest;
    }

    private IEnumerator MoveToPosition(Vector3 targetPosition)
    {
        isMoving = true;
        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPosition;
        isMoving = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Pressure Plate"))
        {
            boxMaterial.color = GameManager.HighlightColor;
            GameManager.Instance.UpdateGoalCount(1);
         }
    }   

   private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Pressure Plate"))
        {
            boxMaterial.color = GameManager.NormalColor;
            GameManager.Instance.UpdateGoalCount(-1);
        }        
    }

    public bool IsMoving()
    {
        return isMoving;
    }      
}

