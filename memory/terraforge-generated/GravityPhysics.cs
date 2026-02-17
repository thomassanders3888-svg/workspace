using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class GravityPhysics : MonoBehaviour {
    public static float Gravity = -20f;
    public float terminalVelocity = -50f;
    public float groundCheckDistance = 0.1f;
    
    private CharacterController _controller;
    private Vector3 _velocity;
    private bool _isGrounded;
    
    void Awake() {
        _controller = GetComponent<CharacterController>();
    }
    
    void Update() {
        GroundCheck();
        ApplyGravity();
        ApplyVelocity();
    }
    
    void GroundCheck() {
        _isGrounded = _controller.isGrounded;
        
        if (_isGrounded && _velocity.y < 0) {
            _velocity.y = -2f; // Small negative to keep grounded
        }
    }
    
    void ApplyGravity() {
        if (_isGrounded) return;
        
        _velocity.y += Gravity * Time.deltaTime;
        _velocity.y = Mathf.Max(_velocity.y, terminalVelocity);
    }
    
    void ApplyVelocity() {
        _controller.Move(_velocity * Time.deltaTime);
    }
    
    public void AddVelocity(Vector3 velocity) {
        _velocity += velocity;
    }
    
    public void SetVelocity(Vector3 velocity) {
        _velocity = velocity;
    }
    
    public void Jump(float force) {
        if (_isGrounded) {
            _velocity.y = force;
        }
    }
    
    public Vector3 GetVelocity() => _velocity;
    public bool IsGrounded => _isGrounded;
    
    public void ApplyKnockback(Vector3 direction, float force) {
        _velocity += direction * force;
    }
}
