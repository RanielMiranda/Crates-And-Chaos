using UnityEngine;
using UnityEngine.EventSystems;

public class ObjectMover : MonoBehaviour
{
    public float gridSize = 0.25f * 1; // Grid snapping size
    private GameObject selectedObject;
    private LevelManager levelManager;

    void Start()
    {
        levelManager = FindFirstObjectByType<LevelManager>();
    }
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
            if (Input.GetKeyDown(KeyCode.W)) MoveSelected(Vector3.forward * 0.5f);
            if (Input.GetKeyDown(KeyCode.S)) MoveSelected(Vector3.back * 0.5f);
            if (Input.GetKeyDown(KeyCode.A)) MoveSelected(Vector3.left * 0.5f);
            if (Input.GetKeyDown(KeyCode.D)) MoveSelected(Vector3.right * 0.5f);
            if (Input.GetKeyDown(KeyCode.R)) MoveSelected(Vector3.up * 0.05f);
            if (Input.GetKeyDown(KeyCode.F)) MoveSelected(Vector3.down * 0.05f);

            if (Input.GetKeyDown(KeyCode.Delete)) DeleteObject();

            //change scale
            if (selectedObject != null && selectedObject.tag == "Wall")
            {
                if (Input.GetKeyDown(KeyCode.Q)) ScaleZ(1);
                if (Input.GetKeyDown(KeyCode.E)) ScaleX(1);

                if (Input.GetKeyDown(KeyCode.Z)) ScaleZ(-1);            
                if (Input.GetKeyDown(KeyCode.C)) ScaleX(-1);
            }
        levelManager.UpdateSelectedObject(selectedObject);            
        }
    }

    private void ScaleX(int value)
    {
        if (selectedObject == null) return;

        var localScale = selectedObject.transform.localScale;

        if (localScale.x + value <= 0) return;
        selectedObject.transform.localScale = new Vector3(localScale.x + value, localScale.y, localScale.z);
    }
    private void ScaleZ(int value)
    {
        if (selectedObject == null) return;

        var localScale = selectedObject.transform.localScale;

        if (localScale.z + value <= 0) return;
        selectedObject.transform.localScale = new Vector3(localScale.x, localScale.y, localScale.z + value);
    }

    public void SelectObject(GameObject obj)
    {
        selectedObject = obj;
    }

    public void DeselectObject()
    {
        selectedObject = null;
    }

    void MoveSelected(Vector3 direction)
    {
        if (selectedObject == null) return;

        Vector3 newPosition = selectedObject.transform.position + direction * gridSize;

        // Ensure proper grid snapping
        newPosition.x = Mathf.Round(newPosition.x / 0.5f) * 0.5f;  
        newPosition.y = Mathf.Round(newPosition.y / 0.05f) * 0.05f; 
        newPosition.z = Mathf.Round(newPosition.z / 0.5f) * 0.5f;  

        selectedObject.transform.position = newPosition;
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    private void DeleteObject()
    {
        if (selectedObject != null)
        {
            Destroy(selectedObject);
            DeselectObject();
        }
    }
}

