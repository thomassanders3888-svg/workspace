using UnityEngine;

public class FlyCam : MonoBehaviour {
    public float normalSpeed = 10f;
    public float fastSpeed = 50f;
    public float slowSpeed = 1f;
    
    void Update() {
        if (!enabled) return;
        
        float speed = Input.GetKey(KeyCode.LeftShift) ? fastSpeed : 
                     Input.GetKey(KeyCode.LeftControl) ? slowSpeed : normalSpeed;
        
        Vector3 move = new Vector3(
            Input.GetAxis("Horizontal"),
            Input.GetKey(KeyCode.E) ? 1 : Input.GetKey(KeyCode.Q) ? -1 : 0,
            Input.GetAxis("Vertical")
        );
        
        transform.Translate(move * speed * Time.deltaTime, Space.Self);
        
        if (Input.GetMouseButton(1)) {
            transform.Rotate(Vector3.up, Input.GetAxis("Mouse X") * 2f, Space.World);
            transform.Rotate(Vector3.right, -Input.GetAxis("Mouse Y") * 2f, Space.Self);
        }
    }
}
