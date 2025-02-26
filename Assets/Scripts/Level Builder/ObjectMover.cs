using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ObjectMover : MonoBehaviour
{
    public float gridSize = 0.25f * 1; // Grid snapping size
    public List<GameObject> ObjArray = new List<GameObject>();
    private LevelManager levelManager;

    void Start()
    {
        levelManager = FindFirstObjectByType<LevelManager>();
    }
    void Update()
    {
        if (ObjArray.Count == 1)
        {
            levelManager.UpdateSelectedObject(ObjArray[0]); // Display info for the only selected object
        }
        else if (ObjArray.Count == 0)
        {
            levelManager.UpdateSelectedObject(null); // No object selected
        }

        if (ObjArray.Count > 0)
        {
            
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) MoveSelected(Vector3.forward * 0.5f);
            if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) MoveSelected(Vector3.back * 0.5f);
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) MoveSelected(Vector3.left * 0.5f);
            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) MoveSelected(Vector3.right * 0.5f);
            if (Input.GetKeyDown(KeyCode.R)) MoveSelected(Vector3.up * 0.05f);
            if (Input.GetKeyDown(KeyCode.F)) MoveSelected(Vector3.down * 0.05f);

            if (Input.GetKeyDown(KeyCode.Q)) ScaleX(1);
            if (Input.GetKeyDown(KeyCode.E)) ScaleZ(1);

            if (Input.GetKeyDown(KeyCode.Z)) ScaleX(-1);
            if (Input.GetKeyDown(KeyCode.C)) ScaleZ(-1);
            

            if (Input.GetKeyDown(KeyCode.Delete)) DeleteSelectedObjects();
        }
    }

    private void ScaleX(int value)
    {
        if (ObjArray.Count == 0) return;

        for (int i = ObjArray.Count - 1; i >= 0; i--)
        {
            GameObject obj = ObjArray[i];
            var localScale = obj.transform.localScale;

            if (localScale.x + value <= 0) continue;
            obj.transform.localScale = new Vector3(localScale.x + value, localScale.y, localScale.z);
        }
    }
    private void ScaleZ(int value)
    {
        if (ObjArray.Count == 0) return;

        for (int i = ObjArray.Count - 1; i >= 0; i--)
        {
            GameObject obj = ObjArray[i];
            var localScale = obj.transform.localScale;

            if (localScale.z + value <= 0) continue;
            obj.transform.localScale = new Vector3(localScale.x, localScale.y, localScale.z + value);
        }
    }

    public void SelectObject(GameObject obj)
    {
        ObjArray.Add(obj);
    }

    public void DeselectObject(GameObject obj)
    {
        ObjArray.Remove(obj);
    }

    void MoveSelected(Vector3 direction)
    {
        if (ObjArray.Count == 0) return;

        for (int i = ObjArray.Count - 1; i >= 0; i--)
        {
            GameObject obj = ObjArray[i];
            Vector3 newPosition = obj.transform.position + direction * gridSize;
            newPosition.x = Mathf.Round(newPosition.x / 0.5f) * 0.5f;
            newPosition.y = Mathf.Round(newPosition.y / 0.05f) * 0.05f;
            newPosition.z = Mathf.Round(newPosition.z / 0.5f) * 0.5f;

            obj.transform.position = newPosition;
        }
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    private void DeleteSelectedObjects()
    {
        for (int i = ObjArray.Count - 1; i >= 0; i--)
        {
            GameObject obj = ObjArray[i];
            Destroy(obj);
        }
        ObjArray.Clear();
    }

    public void ClearSelection()
    {
        for (int i = ObjArray.Count - 1; i >= 0; i--)
        {
            GameObject obj = ObjArray[i];
            obj.GetComponent<SelectableObject>().DeselectObject();
        }
        ObjArray.Clear();
    }
}

