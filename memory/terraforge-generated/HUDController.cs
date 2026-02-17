using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDController : MonoBehaviour {
    public static HUDController Instance { get; private set; }
    
    [Header("HUD Elements")]
    public GameObject crosshair;
    public Image healthBar;
    public Image staminaBar;
    public TextMeshProUGUI coordinates;
    public TextMeshProUGUI fpsCounter;
    public TextMeshProUGUI networkStatus;
    public GameObject chatPanel;
    public Transform notificationContainer;
    
    [Header("Hotbar")]
    public Image[] hotbarSlots;
    public Image highlightSlot;
    
    [Header("Minimap")]
    public RectTransform minimapRect;
    public RawImage minimapImage;
    
    private int selectedSlot = 0;
    private float lastFpsUpdate;
    
    void Awake() { Instance = this; }
    
    void Update() {
        UpdateHealthDisplay();
        UpdateStaminaDisplay();
        UpdateCoordinates();
        UpdateFPS();
        HandleHotbarInput();
    }
    
    void UpdateHealthDisplay() {
        var player = FindObjectOfType<Player>();
        if (player != null) {
            healthBar.fillAmount = player.Health / player.MaxHealth;
        }
    }
    
    void UpdateStaminaDisplay() {
        var player = FindObjectOfType<PlayerController>();
        if (player != null) {
            staminaBar.fillAmount = player.Stamina / 100f;
        }
    }
    
    void UpdateCoordinates() {
        var player = FindObjectOfType<PlayerController>();
        if (player != null) {
            Vector3Int pos = Vector3Int.FloorToInt(player.transform.position);
            coordinates.text = $"X: {pos.x} Y: {pos.y} Z: {pos.z}";
        }
    }
    
    void UpdateFPS() {
        if (Time.time - lastFpsUpdate > 0.5f) {
            fpsCounter.text = $"{Mathf.Round(1f / Time.deltaTime)} FPS";
            lastFpsUpdate = Time.time;
        }
    }
    
    void UpdateNetworkStatus() {
        bool connected = NetworkClient.Instance?.IsConnected ?? false;
        networkStatus.text = connected ? $"Ping: {NetworkClient.Instance.Ping}ms" : "Disconnected";
        networkStatus.color = connected ? Color.green : Color.red;
    }
    
    void HandleHotbarInput() {
        for (int i = 0; i < 9; i++) {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i)) {
                SelectSlot(i);
            }
        }
        
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0) {
            selectedSlot = (selectedSlot + (int)Mathf.Sign(scroll) + 9) % 9;
            SelectSlot(selectedSlot);
        }
    }
    
    void SelectSlot(int slot) {
        selectedSlot = slot;
        highlightSlot.transform.position = hotbarSlots[slot].transform.position;
    }
    
    public void ShowNotification(string message, float duration = 3f) {
        // Create notification popup
        Debug.Log($"[Notification] {message}");
    }
    
    public void ToggleChat() {
        chatPanel.SetActive(!chatPanel.activeSelf);
    }
    
    public void UpdateMinimap(Texture texture) {
        minimapImage.texture = texture;
    }
}
