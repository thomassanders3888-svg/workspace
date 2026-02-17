using UnityEngine;

public enum EquipmentSlot { Helmet, Chest, Legs, Boots, Weapon, Tool }

[System.Serializable]
public class EquipmentItem {
    public string itemId;
    public string name;
    public EquipmentSlot slot;
    public int defense;
    public int damage;
    public Sprite icon;
    public GameObject modelPrefab;
}

public class EquipmentManager : MonoBehaviour {
    public static EquipmentManager Instance { get; private set; }
    
    private EquipmentItem[] equippedItems = new EquipmentItem[6];
    public System.Action<EquipmentSlot> OnEquipmentChanged;
    
    void Awake() { Instance = this; }
    
    public bool Equip(EquipmentItem item) {
        if (item == null) return false;
        
        int slotIndex = (int)item.slot;
        if (equippedItems[slotIndex] != null) Unequip(item.slot);
        
        equippedItems[slotIndex] = item;
        
        if (item.modelPrefab != null) {
            AttachModel(item.slot, item.modelPrefab);
        }
        
        OnEquipmentChanged?.Invoke(item.slot);
        return true;
    }
    
    public void Unequip(EquipmentSlot slot) {
        int index = (int)slot;
        if (equippedItems[index] != null) {
            equippedItems[index] = null;
            RemoveModel(slot);
            OnEquipmentChanged?.Invoke(slot);
        }
    }
    
    public EquipmentItem GetEquipped(EquipmentSlot slot) => equippedItems[(int)slot];
    
    public int GetTotalDefense() {
        int total = 0;
        foreach (var item in equippedItems) {
            if (item != null) total += item.defense;
        }
        return total;
    }
    
    public int GetWeaponDamage() {
        return equippedItems[4]?.damage ?? 1;
    }
    
    void AttachModel(EquipmentSlot slot, GameObject prefab) {
        // Instantiate on appropriate bone
    }
    
    void RemoveModel(EquipmentSlot slot) {
        // Remove instantiated model
    }
}
