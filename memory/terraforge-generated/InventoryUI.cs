using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour {
    public static InventoryUI Instance { get; private set; }
    public GameObject inventoryPanel;
    public Transform slotsParent;
    public GameObject slotPrefab;
    public int inventorySize = 36;
    private InventorySlot[] slots;
    
    void Awake() { Instance = this; }
    void Start() { InitializeSlots(); }
    
    void InitializeSlots() {
        slots = new InventorySlot[inventorySize];
        for (int i = 0; i < inventorySize; i++) {
            var slotObj = Instantiate(slotPrefab, slotsParent);
            slots[i] = slotObj.GetComponent<InventorySlot>();
            slots[i].slotIndex = i;
        }
    }
    
    public void ToggleInventory() {
        inventoryPanel.SetActive(!inventoryPanel.activeSelf);
        Cursor.lockState = inventoryPanel.activeSelf ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = inventoryPanel.activeSelf;
    }
    
    public void UpdateSlot(int slotIndex, Sprite icon, int count) {
        if (slotIndex >= 0 && slotIndex < slots.Length) {
            slots[slotIndex].SetItem(icon, count);
        }
    }
}

public class InventorySlot : MonoBehaviour {
    public int slotIndex;
    public Image iconImage;
    public TextMeshProUGUI countText;
    public Button slotButton;
    
    void Start() {
        slotButton.onClick.AddListener(OnSlotClick);
    }
    
    void OnSlotClick() {
        Debug.Log($"Clicked slot {slotIndex}");
    }
    
    public void SetItem(Sprite icon, int count) {
        iconImage.sprite = icon;
        iconImage.gameObject.SetActive(icon != null);
        countText.text = count > 1 ? count.ToString() : "";
    }
}
