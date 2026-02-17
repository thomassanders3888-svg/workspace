using UnityEngine;

public class PlayerSync : MonoBehaviour {
    public float syncInterval = 0.1f;
    public float interpolationSpeed = 15f;
    public bool isLocalPlayer = true;
    
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private Vector3 lastSentPosition;
    private Quaternion lastSentRotation;
    private float syncTimer;
    
    void Update() {
        if (isLocalPlayer) {
            LocalPlayerUpdate();
        } else {
            RemotePlayerUpdate();
        }
    }
    
    void LocalPlayerUpdate() {
        syncTimer += Time.deltaTime;
        
        if (syncTimer >= syncInterval) {
            if (HasMoved()) {
                SendPosition();
            }
            syncTimer = 0;
        }
    }
    
    bool HasMoved() {
        return Vector3.Distance(transform.position, lastSentPosition) > 0.01f ||
               Quaternion.Angle(transform.rotation, lastSentRotation) > 1f;
    }
    
    void SendPosition() {
        var data = new Vector3[] { transform.position, transform.rotation.eulerAngles };
        NetworkClient.Instance.Send("PlayerMove", new { pos = data[0], rot = data[1] });
        lastSentPosition = transform.position;
        lastSentRotation = transform.rotation;
    }
    
    void RemotePlayerUpdate() {
        transform.position = Vector3.Lerp(transform.position, targetPosition, interpolationSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, interpolationSpeed * Time.deltaTime);
    }
    
    public void ReceivePosition(Vector3 pos, Vector3 rot) {
        targetPosition = pos;
        targetRotation = Quaternion.Euler(rot);
    }
    
    public void Teleport(Vector3 pos) {
        targetPosition = pos;
        transform.position = pos;
        lastSentPosition = pos;
    }
}
