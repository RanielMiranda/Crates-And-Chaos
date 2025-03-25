using UnityEngine;

public class ArmFollowMouse : MonoBehaviour
{
    public Camera mainCamera; // Assign the main camera
    public Transform arm; // Assign the arm's transform

    void Update()
    {
        if (arm == null || mainCamera == null) return;

        // Get mouse position in world space
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = Mathf.Abs(mainCamera.transform.position.z - arm.position.z);
        Vector3 worldMousePos = mainCamera.ScreenToWorldPoint(mousePosition);

        // Calculate direction from arm to mouse
        Vector3 direction = (worldMousePos - arm.position).normalized;

        // Rotate the arm
        float angle = (Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg)- 90;  // Adjust the angle as needed
        arm.rotation = Quaternion.Euler(0, 0, angle);
    }
}

