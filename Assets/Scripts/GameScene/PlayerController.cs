//playercontroller.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public LayerMask blockingLayer;
    private bool isMoving = false;
    private bool isWin = false;
    private Vector3 lastDirection = Vector3.zero; // Store last movement direction
    private Animator animator;
    private List<Vector3> metalBoxFuturePositions = new List<Vector3>(); // This variable seems unused in the current logic for blocking player movement
    private Vector3 futureEBoxPosition;


    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        if (GameManager.Instance != null) {
        GameManager.Instance.SaveState();
        }

        //Level Editor
        if (GameObject.FindFirstObjectByType<LevelManager>() != null)
        {
            enabled = false;
        }        
        
        // Initial population - not used in movement blocking logic currently
        if (GameManager.Instance != null) 
        foreach (var metalBox in GameManager.Instance.GetMetalBoxes())
        {
            metalBoxFuturePositions.Add(metalBox.GetFuturePosition());
        }
    }

    void Update()
    {
        if (isWin) return;
        if (isMoving) return;

        GetNonMovementInput();

        var movement = Vector3.zero;
        GetMovementInput(ref movement);

        //Animations (happens every frame even if not moving, might be okay)
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
            if (hitFront.collider.CompareTag("Box"))
                {animator.SetBool("isBoxInFront", true);}
            if (hitFront.collider.CompareTag("Ember Box") ||
                hitFront.collider.CompareTag("Volt Box") || hitFront.collider.CompareTag("Frost Box") || 
                hitFront.collider.CompareTag("Magnet Box"))
            {
                animator.SetBool("isBoxInFront", true);
                futureEBoxPosition = hitFront.collider.GetComponent<ElementalBoxController>().GetFuturePosition(lastDirection); // This line likely causes issues as GetFuturePosition needs context
            }
            
        }
    }

    private void GetNonMovementInput()
    {
        if (Input.GetKeyDown(KeyCode.Z)) GameManager.Instance.Undo();
        if (Input.GetKeyDown(KeyCode.X)) GameManager.Instance.Redo();
        if (Input.GetKeyDown(KeyCode.R)) GameManager.Instance.Reset();
        if (Input.GetKeyDown(KeyCode.Escape)) GameManager.Instance.toggleWinScreenUI();

    }

    private void GetMovementInput(ref Vector3 movement)
    {
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) movement = Vector3.forward;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) movement = Vector3.back;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) movement = Vector3.left;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) movement = Vector3.right;

        if (movement != Vector3.zero)
        {
            lastDirection = movement; 
        }
    }

    private void TryToMove(Vector3 direction)
    {        
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
                    StartCoroutine(MoveToPosition(targetPosition)); // Start player move
                }
                return; // Player interacted with box, either pushed or was blocked
            }

            // Moving elemental boxes
            if (hit.collider.CompareTag("Ember Box") || hit.collider.CompareTag("Volt Box") ||
                hit.collider.CompareTag("Frost Box") || hit.collider.CompareTag("Magnet Box"))
            {
                var box = hit.collider.GetComponent<ElementalBoxController>();

                if (box != null)
                {
                    animator.SetBool("isPushing", true);

                    // Empty space or pushable non-reacting box
                    if (box.TryToPushBox(direction, moveSpeed) && !box.GetIsReacting())
                    {
                        // The logic checking futureEBoxPosition here might need review
                        // as futureEBoxPosition is set in CheckForBoxesAround, not TryToMove.
                        // For now, keeping the original logic structure.
                        // A safer check might involve raycasting one unit beyond the box.
                        
                        if (targetPosition.x == futureEBoxPosition.x && targetPosition.z == futureEBoxPosition.z)
                        {
                            return;
                        }
                        
                        StartCoroutine(MoveToPosition(targetPosition)); // Start player move
                    }
                    // Reaction occurred (box was NOT pushed successfully)
                    else
                    {
                        // Perform reaction logic - player doesn't move, but box might react
                        box.PerformReaction(box.CheckForReaction(hit), hit, targetPosition, direction, moveSpeed);
                    }
                }
                return; // Player interacted with elemental box
            }

            // Moving to goal
            if (hit.collider.CompareTag("Goal") && GameManager.Instance.CheckWinCondition())
            {
                StartCoroutine(MoveToPosition(targetPosition)); // Start player move to goal
                GameManager.Instance.isWin = true;                
                // GameManager.Instance.toggleWinScreenUI(); // Toggle after move is complete?
                isWin = true;
                return; // Player moving to goal
            }
            // If hit something else (wall, blocked box, etc.), the 'return' at the top of the if block already handled it.
        }
        else
        {
        
        // No obstacles detected, move freely

        animator.SetBool("isPushing", false);
        StartCoroutine(MoveToPosition(targetPosition));          
        }
    }

    private IEnumerator MoveToPosition(Vector3 target)
    {
        isMoving = true;
        AudioManager.Instance.PlayMovePlayer();
        animator.SetBool("isMoving", true); // Set animation for movement

        while (Vector3.Distance(transform.position, target) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = target;

        // Player has finished their move, now update game state and trigger box movements
        GameManager.Instance.SaveState();
        GameManager.Instance.UpdateMetalBoxes();
        GameManager.Instance.CacheMetalAndMagnetBoxes();

        ResetMovePosition();

        if (isWin) // Check if win condition was set when moving to Goal
        {
            GameManager.Instance.toggleWinScreenUI();
        }
    }

    private void ResetMovePosition()
    {
        isMoving = false;
        animator.SetBool("isMoving", false); // Reset animation for movement
        animator.SetBool("isPushing", false);

        // This list is populated but not currently used to block player movement
        metalBoxFuturePositions.Clear();
        foreach (var metalBox in GameManager.Instance.GetMetalBoxes())
        {
            metalBoxFuturePositions.Add(metalBox.GetFuturePosition());
        }        
    }

    // This method is called by MetalBoxController when it hits the player
    public bool MetalTryToMove(Vector3 direction)
    {
        // Check if the player is already moving
        if (isMoving) return false;

        Vector3 targetPosition = transform.position + direction;

        // Check if the space the player would move into is empty
        // Note: This uses CheckSphere, which might be less precise than Raycast for grid-based movement
        if (!Physics.CheckSphere(targetPosition, 0.1f, blockingLayer)) 
        {
            // Player is free to move, start the player's move coroutine
            // This might trigger a nested move if called from within another movement coroutine
            StartCoroutine(MoveToPosition(targetPosition)); 
            return true; // Player started moving successfully
        }

        return false; // Player couldn't move (blocked)
    }
}