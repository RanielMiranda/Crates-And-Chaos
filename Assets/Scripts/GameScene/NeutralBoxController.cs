using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeutralBoxController : MonoBehaviour
{
    public LayerMask blockingLayer;
    private bool isMoving = false;
    private Material boxMaterial;

    private void Start() {
        boxMaterial = GetComponent<Renderer>().material;

        //Level Editor
        if (GameObject.FindFirstObjectByType<LevelManager>() != null)
        {
            Destroy(this);
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

}
