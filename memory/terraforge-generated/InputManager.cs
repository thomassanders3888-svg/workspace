using UnityEngine;
using System;

public class InputManager : MonoBehaviour {
    public static InputManager Instance { get; private set; }
    public event Action<Vector2> OnMove;
    public event Action OnJump;
    public event Action OnAttack;
    public event Action OnInteract;
    
    void Awake() { Instance = this; }
    
    void Update() {
        Vector2 moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (moveInput.magnitude > 0.1f) OnMove?.Invoke(moveInput.normalized);
        
        if (Input.GetButtonDown("Jump")) OnJump?.Invoke();
        if (Input.GetMouseButtonDown(0)) OnAttack?.Invoke();
        if (Input.GetKeyDown(KeyCode.E)) OnInteract?.Invoke();
    }
}
