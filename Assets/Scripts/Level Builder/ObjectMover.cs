using UnityEngine;
using UnityEngine.EventSystems;

public class ObjectMover : MonoBehaviour
{
    public float gridSize = 0.25f * 50; // Grid snapping size
    private GameObject selectedObject;

    private void Update()
    {   
        if (selectedObject == null) return;
        if (Input.GetMouseButtonDown(0)) // Left-click to select
        {
            if (IsPointerOverUI()) return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                SelectObject(hit.collider.gameObject);
            }
        }

        if (selectedObject != null)
        {
            if (Input.GetKeyDown(KeyCode.W)) MoveSelected(Vector3.forward);
            if (Input.GetKeyDown(KeyCode.S)) MoveSelected(Vector3.back);
            if (Input.GetKeyDown(KeyCode.A)) MoveSelected(Vector3.left);
            if (Input.GetKeyDown(KeyCode.D)) MoveSelected(Vector3.right);
            if (Input.GetKeyDown(KeyCode.Q)) MoveSelected(Vector3.up);
            if (Input.GetKeyDown(KeyCode.E)) MoveSelected(Vector3.down);
        }
    }

    public void SelectObject(GameObject obj)
    {
        selectedObject = obj;
    }

    public void DeselectObject()
    {
        Debug.Log("Deselecting object: " + selectedObject.name);
        selectedObject = null;

    }

    void MoveSelected(Vector3 direction)
    {
    if (selectedObject == null) return;

    Vector3 newPosition = selectedObject.transform.position + direction * gridSize;

    // Ensure proper grid snapping
    newPosition.x = Mathf.Round(newPosition.x / gridSize) * gridSize;
    newPosition.y = Mathf.Round(newPosition.y / gridSize) * gridSize;
    newPosition.z = Mathf.Round(newPosition.z / gridSize) * gridSize;

    selectedObject.transform.position = newPosition;
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}

