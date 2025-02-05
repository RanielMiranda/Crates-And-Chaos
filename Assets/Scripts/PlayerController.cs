using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public LayerMask blockingLayer;
    private bool isMoving = false;  

    void Start ()
    {
        GameManager.Instance.SaveState();
    }

    void Update()
    {
        if (isMoving) return; //don't do anything if the player is already moving

        GetNonMovementInput();

        var movement = Vector3.zero;        

        GetMovementInput(ref movement);
        
        if (movement != Vector3.zero) 
        {
            TryToMove(movement);
        }
    }

    private void GetNonMovementInput()
    {
        if (Input.GetKeyDown(KeyCode.Z)) GameManager.Instance.Undo();
        if (Input.GetKeyDown(KeyCode.X)) GameManager.Instance.Redo();
        if (Input.GetKeyDown(KeyCode.C)) GameManager.Instance.Reset();
        if (Input.GetKeyDown(KeyCode.V)) GameManager.Instance.Cheat();
    }

    private void GetMovementInput(ref Vector3 movement) 
    {
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) movement = Vector3.forward;  // Move forward
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) movement = Vector3.back;     // Move backward
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) movement = Vector3.left;     // Move left
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) movement = Vector3.right;    // Move right        
    }

    private void TryToMove(Vector3 direction) 
    {   
        var targetPosition = transform.position + direction;

        //moving to empty spaces
        if(!Physics.Raycast(transform.position, direction, out RaycastHit hit, 1f, blockingLayer))
        {
            StartCoroutine(MoveToPosition(targetPosition));
        }

        //moving normal boxes
        else if (hit.collider.CompareTag("Box"))
        {
            var box = hit.collider.GetComponent<NeutralBoxController>();
            if (box!=null && box.TryToPushBox(direction, moveSpeed)) 
            {
                StartCoroutine(MoveToPosition(targetPosition));
            }
        }

        //moving to goal
        else if (hit.collider.CompareTag("Goal") && GameManager.Instance.CheckWinCondition()) 
        {
            StartCoroutine(MoveToPosition(targetPosition));
        }
    }

    private IEnumerator MoveToPosition(Vector3 target)
    {
        isMoving = true;

        while (Vector3.Distance(transform.position, target) >0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = target;
        GameManager.Instance.SaveState();        
        isMoving = false;
    }   
}
