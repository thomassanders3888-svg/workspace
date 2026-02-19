using System;
using System.Collections.Generic;
using UnityEngine;

namespace TerraForge.Equipment
{
    /// <summary>
    /// Player equipment system with armor, weapons, and accessories
    /// </summary>
    public class EquipmentSystem : MonoBehaviour
    {
        [Header("Equipment Slots")]
        public EquipmentSlot HeadSlot;
        public EquipmentSlot ChestSlot;
        public EquipmentSlot LegsSlot;
        public EquipmentSlot BootsSlot;
        public EquipmentSlot MainHandSlot;
        public EquipmentSlot OffHandSlot;
        public List<EquipmentSlot> AccessorySlots = new List<EquipmentSlot>(4);
        
        // Events
        public event Action<EquipmentSlot, EquipmentItem> OnEquipped;
        public event Action<EquipmentSlot> OnUnequipped;
        
        // Cached stats
        private int bonusHealth;
        private int bonusArmor;
        private float bonusSpeed;
        private float bonusDamage;
        
        void Awake()
        {
            InitializeSlots();
        }
        
        void InitializeSlots()
        {
            HeadSlot = new EquipmentSlot(EquipmentType.Head);
            ChestSlot = new EquipmentSlot(EquipmentType.Chest);
            LegsSlot = new EquipmentSlot(EquipmentType.Legs);
            BootsSlot = new EquipmentSlot(EquipmentType.Boots);
            MainHandSlot = new EquipmentSlot(EquipmentType.MainHand);
            OffHandSlot = new EquipmentSlot(EquipmentType.OffHand);
            
            for (int i = 0; i < 4; i++)
            {
                AccessorySlots.Add(new EquipmentSlot(EquipmentType.Accessory));
            }
        }
        
        /// <summary>
        /// Equip item to appropriate slot
        /// </summary>
        public bool Equip(EquipmentItem item, EquipmentSlot specificSlot = null)
        {
            EquipmentSlot targetSlot = specificSlot ?? GetSlotForType(item.EquipmentType);
            
            if (targetSlot == null) return false;
            if (targetSlot.EquippedItem != null)
            {
                Unequip(targetSlot);
            }
            
            targetSlot.EquippedItem = item;
            RecalculateStats();
            OnEquipped?.Invoke(targetSlot, item);
            return true;
        }
        
        /// <summary>
        /// Unequip from slot
        /// </summary>
        public EquipmentItem Unequip(EquipmentSlot slot)
        {
            if (slot?.EquippedItem == null) return null;
            
            var item = slot.EquippedItem;
            slot.EquippedItem = null;
            RecalculateStats();
            OnUnequipped?.Invoke(slot);
            return item;
        }
        
        /// <summary>
        /// Get total stats from all equipment
        /// </summary>
        public EquipmentStats GetTotalStats()
        {
            return new EquipmentStats
            {
                Health = bonusHealth,
                Armor = bonusArmor,
                Speed = bonusSpeed,
                Damage = bonusDamage
            };
        }
        
        void RecalculateStats()
        {
            bonusHealth = 0;
            bonusArmor = 0;
            bonusSpeed = 0;
            bonusDamage = 0;
            
            ProcessSlot(HeadSlot);
            ProcessSlot(ChestSlot);
            ProcessSlot(LegsSlot);
            ProcessSlot(BootsSlot);
            ProcessSlot(MainHandSlot);
            ProcessSlot(OffHandSlot);
            
            foreach (var slot in AccessorySlots)
            {
                ProcessSlot(slot);
            }
        }
        
        void ProcessSlot(EquipmentSlot slot)
        {
            if (slot?.EquippedItem == null) return;
            
            var stats = slot.EquippedItem.Stats;
            bonusHealth += stats.Health;
            bonusArmor += stats.Armor;
            bonusSpeed += stats.Speed;
            bonusDamage += stats.Damage;
        }
        
        EquipmentSlot GetSlotForType(EquipmentType type)
        {
            return type switch
            {
                EquipmentType.Head => HeadSlot,
                EquipmentType.Chest => ChestSlot,
                EquipmentType.Legs => LegsSlot,
                EquipmentType.Boots => BootsSlot,
                EquipmentType.MainHand => MainHandSlot,
                EquipmentType.OffHand => OffHandSlot,
                EquipmentType.Accessory => FindEmptyAccessorySlot(),
                _ => null
            };
        }
        
        EquipmentSlot FindEmptyAccessorySlot()
        {
            foreach (var slot in AccessorySlots)
            {
                if (slot.EquippedItem == null) return slot;
            }
            return AccessorySlots[0]; // Replace first if full
        }
    }
    
    public enum EquipmentType
    {
        Head,
        Chest,
        Legs,
        Boots,
        MainHand,
        OffHand,
        Accessory
    }
    
    [System.Serializable]
    public class EquipmentSlot
    {
        public EquipmentType Type { get; private set; }
        public EquipmentItem EquippedItem { get; set; }
        public bool IsEmpty => EquippedItem == null;
        
        public EquipmentSlot(EquipmentType type)
        {
            Type = type;
        }
    }
    
    [CreateAssetMenu(fileName = "Equipment", menuName = "TerraForge/Equipment")]
    public class EquipmentItem : ScriptableObject
    {
        public string ItemName;
        public Sprite Icon;
        public EquipmentType EquipmentType;
        public EquipmentStats Stats;
        public int Durability;
        public int MaxDurability;
    }
    
    [System.Serializable]
    public struct EquipmentStats
    {
        public int Health;
        public int Armor;
        public float Speed;
        public float Damage;
    }
    
    [System.Serializable]
    public struct EquipmentSaveData
    {
        public string HeadItemId;
        public string ChestItemId;
        public string LegsItemId;
        public string BootsItemId;
        public string MainHandItemId;
        public string OffHandItemId;
        public List<string> AccessoryItemIds;
    }
}