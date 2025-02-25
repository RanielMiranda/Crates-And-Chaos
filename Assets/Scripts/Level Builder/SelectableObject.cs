using UnityEngine;

public class SelectableObject : MonoBehaviour
{
    private bool isSelected = false;
    private Renderer objectRenderer;
    private Color originalColor;
    private Color anotherColor;
    private bool isLevelEditor = false;
    private ObjectMover objectMover;
    private LevelManager levelManager;    

    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            originalColor = objectRenderer.material.color;
        }

        if (GameObject.FindFirstObjectByType<LevelManager>() != null)
        {
            isLevelEditor = true;
        }

        levelManager = FindFirstObjectByType<LevelManager>();
        objectMover = FindFirstObjectByType<ObjectMover>(); // Get reference to ObjectMover
    }

    void OnMouseDown()
    {   
        if (isLevelEditor && objectMover != null)
        {
            isSelected = !isSelected;            
            if (isSelected)
            {
                SelectObject();
            }
            else
            {
                DeselectObject();
            }
        }
    }

    public void SelectObject()
    {
        isSelected = true;
        if (CompareTag("Player"))
        {
            Transform playerModel = transform.Find("PlayerModel");

            if (playerModel != null)
            {
                // Get all SkinnedMeshRenderers and MeshRenderers
                SkinnedMeshRenderer[] skinnedRenderers = playerModel.GetComponentsInChildren<SkinnedMeshRenderer>();
                MeshRenderer[] meshRenderers = playerModel.GetComponentsInChildren<MeshRenderer>();

                if (skinnedRenderers.Length > 0)
                {
                    foreach (SkinnedMeshRenderer renderer in skinnedRenderers)
                    {   
                        originalColor = renderer.material.color;
                        renderer.material.color = Color.green;
                    }
                }

                if (meshRenderers.Length > 0)
                {
                    foreach (MeshRenderer renderer in meshRenderers)
                    {
                        anotherColor = renderer.material.color;
                        renderer.material.color = Color.green; 
                    }
                }                
            }
        }
        else
        {
            objectRenderer.material.color = Color.green; 
        }
        levelManager.UpdateSelectedObject(gameObject);
        objectMover.SelectObject(gameObject);
    }

    public void DeselectObject()
    {
        isSelected = false;
        if (CompareTag("Player"))
        {
            Transform playerModel = transform.Find("PlayerModel");

            if (playerModel != null)
            {

                SkinnedMeshRenderer[] skinnedRenderers = playerModel.GetComponentsInChildren<SkinnedMeshRenderer>();
                MeshRenderer[] meshRenderers = playerModel.GetComponentsInChildren<MeshRenderer>();

                if (skinnedRenderers.Length > 0)
                {
                    foreach (SkinnedMeshRenderer renderer in skinnedRenderers)
                    {
                        renderer.material.color = originalColor;
                    }
                }

                if (meshRenderers.Length > 0)
                {
                    foreach (MeshRenderer renderer in meshRenderers)
                    {
                        renderer.material.color = anotherColor;
                    }
                }
            }
        }
        else
        {        
            objectRenderer.material.color = originalColor;
        }
        levelManager.UpdateSelectedObject(null);
        objectMover.DeselectObject();      
    }
}

