using UnityEngine;

public class CameraController : MonoBehaviour {
    public Transform target;
    public Vector3 offset = new Vector3(0, 2, -4);
    public float smoothSpeed = 0.125f;
    public float rotationSpeed = 2f;
    public bool lockCursor = true;
    
    public float minVerticalAngle = -60f;
    public float maxVerticalAngle = 60f;
    
    private float currentYaw;
    private float currentPitch;
    
    void Start() {
        if (lockCursor) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    
    void LateUpdate() {
        if (target == null) return;
        
        // Mouse input
        currentYaw += Input.GetAxis("Mouse X") * rotationSpeed;
        currentPitch -= Input.GetAxis("Mouse Y") * rotationSpeed;
        currentPitch = Mathf.Clamp(currentPitch, minVerticalAngle, maxVerticalAngle);
        
        // Calculate position
        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0);
        Vector3 desiredPosition = target.position + rotation * offset;
        
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.LookAt(target.position + Vector3.up * offset.y * 0.5f);
    }
}
