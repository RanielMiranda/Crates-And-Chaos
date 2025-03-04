using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeutralBoxController : MonoBehaviour
{
    public LayerMask blockingLayer;
    private bool isMoving = false;
    private Material boxMaterial;
    private Color originalMaterial;

    private void Start() {
        boxMaterial = GetComponent<Renderer>().material;
        originalMaterial = boxMaterial.color;

        //Level Editor
        if (GameObject.FindFirstObjectByType<LevelManager>() != null)
        {
            enabled = false;
            Debug.Log("Enabled = false");
        }              
    }

    public bool TryToPushBox(Vector3 direction, float moveSpeed) 
    {   
        if (isMoving) return false;

        var targetPosition = transform.position + direction;

        if(!Physics.Raycast(transform.position, direction, out RaycastHit hit, 1f, blockingLayer))
        {
            StartCoroutine(MoveToPosition(targetPosition, moveSpeed));
            return true;
        }

        return false;
    }

    private IEnumerator MoveToPosition(Vector3 target, float moveSpeed)
    {
        isMoving = true;
        AudioManager.MoveBox();
        
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
            GameManager.Instance.UpdateGoalCount(1);
            GameManager.Instance.RecalculateGoalsCovered();
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
                boxMaterial.color = originalMaterial;
                GameManager.Instance.UpdateGoalCount(-1);
                GameManager.Instance.RecalculateGoalsCovered();                
            }
        }        
    }   

}
