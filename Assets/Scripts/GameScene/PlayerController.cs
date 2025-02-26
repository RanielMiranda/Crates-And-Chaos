using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public LayerMask blockingLayer;
    private bool isMoving = false;
    private Vector3 lastDirection = Vector3.zero; // Store last movement direction
    private Animator animator;
    private List<Vector3> metalBoxFuturePositions = new List<Vector3>();

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        if (GameManager.Instance != null) {
        GameManager.Instance.SaveState();
        }

        //Level Editor
        if (GameObject.FindFirstObjectByType<LevelManager>() != null)
        {
            Destroy(this);
        }        
        
        foreach (var metalBox in GameManager.Instance.GetMetalBoxes())
        {
            metalBoxFuturePositions.Add(metalBox.GetFuturePosition());
        }
    }

    void Update()
    {
        if (isMoving) return;

        GetNonMovementInput();

        var movement = Vector3.zero;
        GetMovementInput(ref movement);

        //Animations
        CheckForBoxesAround();

        if (movement != Vector3.zero)
        {          
            TryToMove(movement);         
        }   
    }

    private void CheckForBoxesAround()
    {
        if (lastDirection == Vector3.zero) return;

        animator.SetBool("isBoxInFront", false); // Reset in front detection

        // Check in front
        if (Physics.Raycast(transform.position, lastDirection, out RaycastHit hitFront, 1f, blockingLayer))
        {
            if (hitFront.collider.CompareTag("Box") || hitFront.collider.CompareTag("Ember Box") ||
                hitFront.collider.CompareTag("Volt Box") || hitFront.collider.CompareTag("Frost Box") || 
                hitFront.collider.CompareTag("Magnet Box"))
            {
                animator.SetBool("isBoxInFront", true);
            }
        }
    }

    private void GetNonMovementInput()
    {
        if (Input.GetKeyDown(KeyCode.Z)) GameManager.Instance.Undo();
        if (Input.GetKeyDown(KeyCode.X)) GameManager.Instance.Redo();
        if (Input.GetKeyDown(KeyCode.R)) GameManager.Instance.Reset();
        if (Input.GetKeyDown(KeyCode.C)) GameManager.Instance.Cheat();
    }

    private void GetMovementInput(ref Vector3 movement)
    {
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) movement = Vector3.forward;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) movement = Vector3.back;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) movement = Vector3.left;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) movement = Vector3.right;

        if (movement != Vector3.zero)
        {
            lastDirection = movement; // Store the last movement direction
        }
    }

    private void TryToMove(Vector3 direction)
    {   
        GameManager.Instance.UpdateMetalBoxes();          
        var targetPosition = transform.position + direction;
        transform.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(0, -90, 0);

        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, 1f, blockingLayer))
        {
            // If the object hit is a wall or an unmovable object, do not proceed
            if (!hit.collider.CompareTag("Box") && 
                !hit.collider.CompareTag("Ember Box") &&
                !hit.collider.CompareTag("Volt Box") &&
                !hit.collider.CompareTag("Frost Box") &&
                !hit.collider.CompareTag("Magnet Box") &&
                !hit.collider.CompareTag("Goal"))
            {
                return; // Block movement
            }

            // Moving normal boxes
            if (hit.collider.CompareTag("Box"))
            {
                var box = hit.collider.GetComponent<NeutralBoxController>();
                if (box != null && box.TryToPushBox(direction, moveSpeed))
                {
                    animator.SetBool("isPushing", true);
                    StartCoroutine(MoveToPosition(targetPosition));
                }
                return;
            }

            // Moving elemental boxes
            if (hit.collider.CompareTag("Ember Box") || hit.collider.CompareTag("Volt Box") ||
                hit.collider.CompareTag("Frost Box") || hit.collider.CompareTag("Magnet Box"))
            {
                var box = hit.collider.GetComponent<ElementalBoxController>();

                if (box != null)
                {
                    animator.SetBool("isPushing", true);

                    // Empty space
                    if (box.TryToPushBox(direction, moveSpeed) && !box.GetIsReacting())
                    {
                        StartCoroutine(MoveToPosition(targetPosition));
                    }
                    // Reaction occurred
                    else
                    {
                        box.PerformReaction(box.CheckForReaction(hit), hit, targetPosition, direction, moveSpeed);
                    }
                }
                return;
            }

            // Moving to goal
            if (hit.collider.CompareTag("Goal") && GameManager.Instance.CheckWinCondition())
            {
                StartCoroutine(MoveToPosition(targetPosition));
                return;
            }
        }
        else
        {
        
        // No obstacles detected, move freely            
        foreach (var futurePosition in metalBoxFuturePositions)
        {
            Debug.Log("Checking against metal box future pos: " + futurePosition + " and target pos: " + targetPosition);
            if (Mathf.Round(futurePosition.x) == Mathf.Round(targetPosition.x) &&
                Mathf.Round(futurePosition.z) == Mathf.Round(targetPosition.z))
            {
                Debug.Log("Don't Move");
                return;                   
            }
        }

        animator.SetBool("isPushing", false);
        StartCoroutine(MoveToPosition(targetPosition));          
        }
    }

    private IEnumerator MoveToPosition(Vector3 target)
    {
        isMoving = true;
        AudioManager.MovePlayer();
        animator.SetBool("isMoving", true); // Set animation for movement

        while (Vector3.Distance(transform.position, target) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = target;
        GameManager.Instance.SaveState();
        ResetMovePosition();
    }

    private void ResetMovePosition()
    {
        isMoving = false;
        animator.SetBool("isMoving", false); // Reset animation for movement
        animator.SetBool("isPushing", false);

        metalBoxFuturePositions.Clear();
        foreach (var metalBox in GameManager.Instance.GetMetalBoxes())
        {
            metalBoxFuturePositions.Add(metalBox.GetFuturePosition());
        }        
    }
}