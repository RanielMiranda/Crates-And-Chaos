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
    private Color originalMaterial;

    private void Start() {
        boxMaterial = GetComponent<Renderer>().material;
        originalMaterial = boxMaterial.color;

        //Level Editor
        if (GameObject.FindFirstObjectByType<LevelManager>() != null)
        {
            enabled = false;
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
            
        if (Physics.Raycast(transform.position, directionToMagnet, out RaycastHit hit, 1f, blockingLayer))
        {

            // If the object hit is a wall or an unmovable object, do not proceed
            if (!hit.collider.CompareTag("Box") && 
                !hit.collider.CompareTag("Ember Box") &&
                !hit.collider.CompareTag("Volt Box") &&
                !hit.collider.CompareTag("Frost Box") &&
                !hit.collider.CompareTag("Player"))
            {
                return; // Block movement
            }

            //Player
            if (hit.collider.CompareTag("Player"))
            {
                var player = hit.collider.GetComponent<PlayerController>(); // Replace with your actual player script name
                if (player != null)
                {
                    if (player.MetalTryToMove(directionToMagnet)) // Check if the player can move
                    {
                        StartCoroutine(MoveToPosition(targetPosition));
                    }
                    return;
                }
            }            

            if (hit.collider.CompareTag("Box"))
            {
                var box = hit.collider.GetComponent<NeutralBoxController>();
                if (box != null && box.TryToPushBox(directionToMagnet, moveSpeed))
                {
                    StartCoroutine(MoveToPosition(targetPosition));
                }
                return;
            }

            // Moving elemental boxes
            if (hit.collider.CompareTag("Ember Box") || hit.collider.CompareTag("Volt Box") ||
                hit.collider.CompareTag("Frost Box"))
            {
                var box = hit.collider.GetComponent<ElementalBoxController>();

                if (box != null)
                {
                    // Empty space
                    Vector3 futureEBoxPosition = box.GetFuturePosition(directionToMagnet);
                    if (box.TryToPushBox(directionToMagnet, moveSpeed) && !box.GetIsReacting())
                    {
                        // EBox in front
                        if (targetPosition.x == futureEBoxPosition.x && targetPosition.z == futureEBoxPosition.z)
                        {
                            return;
                        }
                        StartCoroutine(MoveToPosition(targetPosition));
                    }
                    // Reaction occurred
                    else
                    {
                        box.PerformReaction(box.CheckForReaction(hit), hit, targetPosition, directionToMagnet, moveSpeed);
                    }
                }
                return;
            }
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
            if (magnet.transform.position.y != transform.position.y) continue;
            float distance;
            if (magnet.transform.position.x == transform.position.x)
            {
                distance = Mathf.Abs(magnet.transform.position.z - transform.position.z);
            }
            else if (magnet.transform.position.z == transform.position.z)
            {
                distance = Mathf.Abs(magnet.transform.position.x - transform.position.x);
            }
            else continue;
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = magnet;
            }
        }
        return nearest;
    }

    private IEnumerator MoveToPosition(Vector3 targetPosition)
    {
        isMoving = true;
        AudioManager.Instance.PlayMoveBox();
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
        if (!enabled) return;
        if (other.CompareTag("Pressure Plate"))
        {
            Debug.Log("Metal box entered pressure plate: " + other.name);
            boxMaterial.color = GameManager.HighlightColor;
            GameManager.Instance.UpdateGoalCount();
         }
    }   

    private void OnTriggerExit(Collider other)
    {
        if (!enabled) return;
        if (other.CompareTag("Pressure Plate"))
        {
            // Check if it is still under a pressure plate, else do below
            var colliders = Physics.OverlapBox(transform.position, transform.localScale / 2);
            bool isStillUnderPressurePlate = false;
            foreach (var collider in colliders)
            {
                if (collider.CompareTag("Pressure Plate"))
                {
                    isStillUnderPressurePlate = true;
                    break;
                }
            }
            if (!isStillUnderPressurePlate)
            {
                Debug.Log("Metal box entered pressure plate: " + other.name);
                boxMaterial.color = originalMaterial;
                GameManager.Instance.UpdateGoalCount();             
            }
        }        
    }  

    public bool IsMoving()
    {
        return isMoving;
    }      
}

