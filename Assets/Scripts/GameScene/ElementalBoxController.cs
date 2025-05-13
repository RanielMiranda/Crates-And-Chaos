using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementalBoxController : MonoBehaviour
{
    public LayerMask blockingLayer;
    private bool isMoving = false;
    private Material boxMaterial;
    private Color originalMaterial;
    private bool isReacting = false;
    //private int elemental level = 1;

    public enum ElementalReactions
    {
        None,
        Overload,
        Melt,
        Magnet,
    }
    private void Start() {
        boxMaterial = GetComponent<Renderer>().material;
        originalMaterial = boxMaterial.color;

        //Level Editor
        if (GameObject.FindFirstObjectByType<LevelManager>() != null)
        {
            enabled = false;
        }              
    }
    public bool TryToPushBox(Vector3 direction, float moveSpeed) 
    {   
        if (isMoving) return false;

        Vector3 targetPosition = transform.position + direction;

        // No collision
        if (!Physics.Raycast(transform.position, direction, out RaycastHit hit, direction.magnitude, blockingLayer))
        {
            AudioManager.Instance.PlayMoveBox();       
            StartCoroutine(MoveToPosition(targetPosition, moveSpeed));
            return true;
        }

        // Collision case
        Vector3 adjustedPosition = hit.point - (direction.normalized * 0.5f); // Stop before the hit
        adjustedPosition = new Vector3(
            Mathf.Round(adjustedPosition.x),
            transform.position.y,
            Mathf.Round(adjustedPosition.z)
        );
        
        StartCoroutine(MoveToPosition(adjustedPosition, moveSpeed));
        
        ElementalReactions reactions = CheckForReaction(hit);
        if (reactions != ElementalReactions.None) 
        {
            PerformReaction(reactions, hit, adjustedPosition, direction, moveSpeed);
            isReacting = true;
        }
        return true;
    }

    public Vector3 GetFuturePosition(Vector3 direction)
    {
        Vector3 targetPosition = transform.position + direction;

        // If there's no obstacle, return targetPosition
        if (!Physics.Raycast(transform.position, direction, out RaycastHit hit, direction.magnitude, blockingLayer))
        {
            return targetPosition;
        }
        
        // Stop before the obstacle
        Vector3 adjustedPosition = hit.point - (direction.normalized * 0.5f);
        adjustedPosition = new Vector3(
            Mathf.Round(adjustedPosition.x),
            transform.position.y,
            Mathf.Round(adjustedPosition.z)
        );

        return adjustedPosition;
    }

    public bool GetIsReacting() { return isReacting; }
    public ElementalReactions CheckForReaction(RaycastHit hit)
    {
        string currentBoxTag = gameObject.tag;
        string otherBoxTag = hit.collider.tag;

        if (currentBoxTag == "Ember Box" && otherBoxTag == "Volt Box")
            return ElementalReactions.Overload;
        if (currentBoxTag == "Ember Box" && otherBoxTag == "Frost Box")
            return ElementalReactions.Melt;
        if (currentBoxTag == "Volt Box" && otherBoxTag == "Frost Box")
            return ElementalReactions.Magnet;
        if (currentBoxTag == "Volt Box" && otherBoxTag == "Ember Box")
            return ElementalReactions.Overload;
        if (currentBoxTag == "Frost Box" && otherBoxTag == "Ember Box")
            return ElementalReactions.Melt;
        if (currentBoxTag == "Frost Box" && otherBoxTag == "Volt Box")
            return ElementalReactions.Magnet;

        return ElementalReactions.None;
    }
    
    public void PerformReaction(ElementalReactions reaction, RaycastHit hit, Vector3 targetPosition, Vector3 direction, float moveSpeed)
    {
        switch (reaction)
        {
                case ElementalReactions.Overload:
                    var otherBox = GetOtherBox(hit);

                    if (otherBox != null)
                    {
                        Vector3 futurePosition = otherBox.GetFuturePosition(direction);

                        // If future position is occupied, push the next box
                        if (Physics.Raycast(otherBox.transform.position, direction, out RaycastHit nextHit, 1f, blockingLayer))
                        {
                            var nextBox = nextHit.collider.GetComponent<ElementalBoxController>();
                            if (nextBox == null)
                            {
                                var neutralBox = GetOtherNeutralBox(nextHit);
                                if (neutralBox != null)
                                {
                                    neutralBox.TryToPushBox(direction * 2f, moveSpeed);
                                }
                            }
                            else if (nextBox != null)
                            {
                                nextBox.TryToPushBox(direction * 2f, moveSpeed); // Transfer energy to next box                                
                            }
                        }
                        // Move the box normally
                        otherBox.TryToPushBox(direction, moveSpeed);
                    }
                    break;

            case ElementalReactions.Melt:
                Debug.Log("Melt has occurred");
                // Destroy current box and collided box based on elemental level, if else statement and decrement elemental level
                DestroyReaction(hit.collider.gameObject);
                DestroyReaction(gameObject);      
                AudioManager.Instance.PlayMoveBox();          
                break;

            case ElementalReactions.Magnet:
                Debug.Log("A Magnet was made");
                var otherbox = GetOtherBox(hit);

                DestroyReaction(gameObject);

                // Create new magnet box at the position of the other box
                GameObject newMagnetBox = Instantiate(GameManager.Instance.magnetBoxPrefab, otherbox.transform.position, Quaternion.identity);
                newMagnetBox.GetComponent<ElementalBoxController>().enabled = true;

                DestroyReaction(otherbox.gameObject);                  
                AudioManager.Instance.PlayMagnetSound();                        
                break;

            case ElementalReactions.None:
                // No reaction, do nothing
                break;
        }

        isReacting = false;
    }

    private IEnumerator MoveToPosition(Vector3 target, float moveSpeed)
    {
        isMoving = true;

        while (Vector3.Distance(transform.position, target) >0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = target;
        isMoving = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!enabled) return;
        if (other.CompareTag("Pressure Plate"))
        {
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
                    GameManager.Instance.UpdateGoalCount();                         
                    break;
                }
            }
            if (!isStillUnderPressurePlate)
            {
                boxMaterial.color = originalMaterial;
                GameManager.Instance.UpdateGoalCount();               
            }
        }        
    }   

    public ElementalBoxController GetOtherBox(RaycastHit hit)
    {
    return hit.collider.GetComponent<ElementalBoxController>();
    }

    public NeutralBoxController GetOtherNeutralBox(RaycastHit hit)
    {
    return hit.collider.GetComponent<NeutralBoxController>();
    }

    private void DestroyReaction(GameObject box)
    {
        // Get the box's current position
        Vector3 currentPosition = box.transform.position;

        // Calculate a new position under the map
        Vector3 newPosition = new Vector3(currentPosition.x, -10f, currentPosition.z);

        // Teleport the box to the new position
        box.transform.position = newPosition;
    }
}