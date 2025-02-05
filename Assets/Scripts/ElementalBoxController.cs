using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementalBoxController : MonoBehaviour
{
    public LayerMask blockingLayer;
    private bool isMoving = false;
    private Material boxMaterial;
    private bool isReacting = false;
    //private int elemental level = 1;

    public enum ElementalReactions
    {
        None,
        Overload,
        Melt,
        Magnet,
        ElementalIncrease,
    }
    private void Start() {
        boxMaterial = GetComponent<Renderer>().material;
    }
    public bool TryToPushBox(Vector3 direction, float moveSpeed) 
    {   
        if (isMoving) return false;

        var targetPosition = transform.position + direction;

        if (!Physics.Raycast(transform.position, direction, out RaycastHit hit, 1f, blockingLayer))
        {
            StartCoroutine(MoveToPosition(targetPosition, moveSpeed));
            return true;
        }  

        ElementalReactions reactions = CheckForReaction(hit);
        if(reactions != ElementalReactions.None) {
            PerformReaction(reactions, hit, targetPosition, direction, moveSpeed);
            isReacting = true;
            return true;
        }
        return false;
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

        //elemental increase    
        if (currentBoxTag == "Ember Box" && otherBoxTag == "Ember Box")
                return ElementalReactions.ElementalIncrease;
        if (currentBoxTag == "Volt Box" && otherBoxTag == "Volt Box")
                return ElementalReactions.ElementalIncrease;
        if (currentBoxTag == "Frost Box" && otherBoxTag == "Frost Box")
                return ElementalReactions.ElementalIncrease;
        

        return ElementalReactions.None;
    }
    

    public void PerformReaction(ElementalReactions reaction, RaycastHit hit, Vector3 targetPosition, Vector3 direction, float moveSpeed)
    {

        switch (reaction)
        {
            case ElementalReactions.Overload:
                Debug.Log("Overload Occured");
                var otherBox = GetOtherBox(hit);                
                /*
                replace 2f with the formula
                float traversed = getElementalLevel() + otherBox.getElementalLevel();

                */ 
                if (otherBox != null)
                {
                    otherBox.TryToPushBox(direction * 2f, moveSpeed);
                }
                break;

            case ElementalReactions.Melt:
                Debug.Log("Melt has occurred");
                // Destroy current box and collided box based on elemental level
                DestroyReaction(hit.collider.gameObject);
                DestroyReaction(gameObject);                
                break;

            case ElementalReactions.Magnet:
                Debug.Log("A Magnet was made");
                //destroy current box and replace collided box infront with magnet object
                /*
                    use DestroyReaction() to push the boxes under the map, then create new magnet box
                */
                break;

            case ElementalReactions.None:
                // No reaction, do nothing
                break;

            case ElementalReactions.ElementalIncrease:
                //Debug.Log("Elemental Increase has occurred");
                // Destroy current box and collided box's elemental level is increased by current box level
                //create a new method to increase elemental level of collided box
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

    public ElementalBoxController GetOtherBox(RaycastHit hit)
    {
    return hit.collider.GetComponent<ElementalBoxController>();
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