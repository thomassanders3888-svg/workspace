using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour {
    public Image fillImage;
    public Transform target;
    public Vector3 offset = new Vector3(0, 2f, 0);
    private Camera mainCamera;
    
    void Start() { mainCamera = Camera.main; }
    
    void LateUpdate() {
        if (target != null) {
            transform.position = target.position + offset;
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward);
        }
    }
    
    public void UpdateHealth(float current, float max) {
        fillImage.fillAmount = current / max;
    }
}
