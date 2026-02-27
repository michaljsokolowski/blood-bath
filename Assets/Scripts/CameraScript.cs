using UnityEngine;

public class CameraScript : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform player;
    public Vector3 offset = new Vector3(0, 0, 0); // Isometric offset

    [Header("Bounds")]
    public Vector2 minBounds; // Bottom-left corner of playable area
    public Vector2 maxBounds; // Top-right corner of playable area

    [Header("Camera Settings")]
    public Camera cam;
    public float cameraHalfWidth;
    public float cameraHalfHeight;

    void Start()
    {
        if (cam == null)
            cam = Camera.main;
    }

    void LateUpdate()
    {
        if (player == null) return;

        // Calculate desired position (player + offset)
        Vector3 desiredPosition = player.position + offset;

        // Clamp camera position to stay within bounds
        float clampedX = Mathf.Clamp(
            desiredPosition.x,
            minBounds.x + cameraHalfWidth,
            maxBounds.x - cameraHalfWidth
        );

        float clampedZ = Mathf.Clamp(
            desiredPosition.z,
            minBounds.y + cameraHalfHeight,
            maxBounds.y - cameraHalfHeight
        );

        // Apply clamped position (keep original Y for height)
        transform.position = new Vector3(clampedX, desiredPosition.y, clampedZ);
    }

    // Visualize bounds in editor
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 bottomLeft = new Vector3(minBounds.x, 0, minBounds.y);
        Vector3 bottomRight = new Vector3(maxBounds.x, 0, minBounds.y);
        Vector3 topLeft = new Vector3(minBounds.x, 0, maxBounds.y);
        Vector3 topRight = new Vector3(maxBounds.x, 0, maxBounds.y);

        Gizmos.DrawLine(bottomLeft, bottomRight);
        Gizmos.DrawLine(bottomRight, topRight);
        Gizmos.DrawLine(topRight, topLeft);
        Gizmos.DrawLine(topLeft, bottomLeft);
    }
}