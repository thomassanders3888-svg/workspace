using UnityEngine;
using System.Collections.Generic;

public class InputManager : MonoBehaviour {
    public static InputManager Instance { get; private set; }
    
    [System.Serializable]
    public class KeyBinding {
        public string actionName;
        public KeyCode primaryKey;
        public KeyCode secondaryKey;
    }
    
    public List<KeyBinding> keyBindings = new();
    
    void Awake() {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadBindings();
    }
    
    public bool GetKeyDown(string action) {
        var binding = keyBindings.Find(k => k.actionName == action);
        if (binding == null) return false;
        return Input.GetKeyDown(binding.primaryKey) || Input.GetKeyDown(binding.secondaryKey);
    }
    
    public bool GetKey(string action) {
        var binding = keyBindings.Find(k => k.actionName == action);
        if (binding == null) return false;
        return Input.GetKey(binding.primaryKey) || Input.GetKey(binding.secondaryKey);
    }
    
    public bool GetKeyUp(string action) {
        var binding = keyBindings.Find(k => k.actionName == action);
        if (binding == null) return false;
        return Input.GetKeyUp(binding.primaryKey) || Input.GetKeyUp(binding.secondaryKey);
    }
    
    public void SetBinding(string action, KeyCode key) {
        var binding = keyBindings.Find(k => k.actionName == action);
        if (binding != null) binding.primaryKey = key;
        else keyBindings.Add(new KeyBinding { actionName = action, primaryKey = key });
        SaveBindings();
    }
    
    void SaveBindings() {
        // Save to PlayerPrefs or JSON
    }
    
    void LoadBindings() {
        // Load defaults if not saved
        if (keyBindings.Count == 0) {
            keyBindings.Add(new KeyBinding { actionName = "MoveForward", primaryKey = KeyCode.W });
            keyBindings.Add(new KeyBinding { actionName = "MoveBack", primaryKey = KeyCode.S });
            keyBindings.Add(new KeyBinding { actionName = "MoveLeft", primaryKey = KeyCode.A });
            keyBindings.Add(new KeyBinding { actionName = "MoveRight", primaryKey = KeyCode.D });
            keyBindings.Add(new KeyBinding { actionName = "Jump", primaryKey = KeyCode.Space });
            keyBindings.Add(new KeyBinding { actionName = "Inventory", primaryKey = KeyCode.I });
            keyBindings.Add(new KeyBinding { actionName = "Interact", primaryKey = KeyCode.E });
        }
    }
}
