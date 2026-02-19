using System;
using System.Collections.Generic;
using UnityEngine;

namespace TerraForge
{
    public enum ItemType
    {
        None,
        Wood,
        Stone,
        Food,
        Tool,
        Ore,
        Herb,
        Potion,
        Weapon,
        Armor,
    }

    [Serializable]
    public struct InventorySlot
    {
        public ItemType ItemType;
        public int Count;
        public int MaxStack;

        public InventorySlot(ItemType itemType, int count, int maxStack)
        {
            ItemType = itemType;
            Count = count;
            MaxStack = maxStack;
        }

        public bool IsEmpty => ItemType == ItemType.None || Count <= 0;
        public bool IsFull => Count >= MaxStack;
        public int RemainingSpace => MaxStack - Count;

        public static InventorySlot CreateDefault(ItemType itemType, int count = 1)
        {
            int maxStack = GetDefaultMaxStack(itemType);
            return new InventorySlot(itemType, count, maxStack);
        }

        private static int GetDefaultMaxStack(ItemType itemType)
        {
            switch (itemType)
            {
                case ItemType.Wood: case ItemType.Stone: case ItemType.Ore: return 64;
                case ItemType.Food: case ItemType.Herb: return 32;
                case ItemType.Tool: case ItemType.Weapon: case ItemType.Armor: return 1;
                case ItemType.Potion: return 16;
                default: return 64;
            }
        }
    }

    public enum InventoryChangeType { Added, Removed, Swapped, Cleared }

    public class InventoryChangedEventArgs : EventArgs
    {
        public int SlotIndex { get; }
        public ItemType ItemType { get; }
        public int NewCount { get; }
        public int OldCount { get; }
        public InventoryChangeType ChangeType { get; }

        public InventoryChangedEventArgs(int slotIndex, ItemType itemType, int newCount, int oldCount, InventoryChangeType changeType)
        {
            SlotIndex = slotIndex; ItemType = itemType; NewCount = newCount; OldCount = oldCount; ChangeType = changeType;
        }
    }

    [Serializable]
    public class InventorySaveData
    {
        public List<InventorySlot> Slots = new List<InventorySlot>();
        public int Capacity;
    }

    public class PlayerInventorySystem : MonoBehaviour
    {
        [SerializeField] private int defaultCapacity = 24;
        private List<InventorySlot> slots;
        private int capacity;

        public event EventHandler<InventoryChangedEventArgs> InventoryChanged;

        public IReadOnlyList<InventorySlot> Slots => slots;
        public int Capacity => capacity;

        public int OccupiedSlotCount
        {
            get
            {
                int count = 0;
                foreach (var slot in slots) if (!slot.IsEmpty) count++;
                return count;
            }
        }

        public bool IsEmpty => OccupiedSlotCount == 0;
        public bool IsFull => OccupiedSlotCount == capacity;

        private void Awake() { Initialize(defaultCapacity); }

        public void Initialize(int newCapacity)
        {
            capacity = newCapacity;
            slots = new List<InventorySlot>(capacity);
            for (int i = 0; i < capacity; i++) slots.Add(new InventorySlot(ItemType.None, 0, 64));
        }

        public int AddItem(ItemType itemType, int amount)
        {
            if (amount <= 0 || itemType == ItemType.None) return amount;
            int remaining = amount;

            for (int i = 0; i < slots.Count && remaining > 0; i++)
            {
                if (slots[i].ItemType == itemType && !slots[i].IsFull)
                {
                    int space = slots[i].RemainingSpace;
                    int toAdd = Mathf.Min(space, remaining);
                    var oldSlot = slots[i];
                    slots[i] = new InventorySlot(itemType, oldSlot.Count + toAdd, oldSlot.MaxStack);
                    remaining -= toAdd;
                    OnInventoryChanged(i, itemType, slots[i].Count, oldSlot.Count, InventoryChangeType.Added);
                }
            }

            for (int i = 0; i < slots.Count && remaining > 0; i++)
            {
                if (slots[i].IsEmpty)
                {
                    int maxStack = InventorySlot.CreateDefault(itemType, 0).MaxStack;
                    int toAdd = Mathf.Min(maxStack, remaining);
                    slots[i] = new InventorySlot(itemType, toAdd, maxStack);
                    remaining -= toAdd;
                    OnInventoryChanged(i, itemType, toAdd, 0, InventoryChangeType.Added);
                }
            }

            return remaining;
        }

        public int AddItem(InventorySlot slot) { return AddItem(slot.ItemType, slot.Count); }

        public int RemoveItem(ItemType itemType, int amount)
        {
            if (amount <= 0 || itemType == ItemType.None) return 0;
            int toRemove = amount, actuallyRemoved = 0;

            for (int i = slots.Count - 1; i >= 0 && toRemove > 0; i--)
            {
                if (slots[i].ItemType == itemType)
                {
                    var oldSlot = slots[i];
                    int removeFromSlot = Mathf.Min(oldSlot.Count, toRemove);
                    int newCount = oldSlot.Count - removeFromSlot;
                    slots[i] = newCount <= 0 ? new InventorySlot(ItemType.None, 0, oldSlot.MaxStack) : new InventorySlot(itemType, newCount, oldSlot.MaxStack);
                    toRemove -= removeFromSlot;
                    actuallyRemoved += removeFromSlot;
                    OnInventoryChanged(i, itemType, newCount, oldSlot.Count, InventoryChangeType.Removed);
                }
            }

            return actuallyRemoved;
        }

        public void RemoveItemAt(int slotIndex, int amount)
        {
            if (slotIndex < 0 || slotIndex >= slots.Count || amount <= 0) return;
            if (slots[slotIndex].IsEmpty) return;

            var oldSlot = slots[slotIndex];
            int newCount = Mathf.Max(0, oldSlot.Count - amount);
            ItemType itemType = oldSlot.ItemType;
            slots[slotIndex] = newCount <= 0 ? new InventorySlot(ItemType.None, 0, oldSlot.MaxStack) : new InventorySlot(itemType, newCount, oldSlot.MaxStack);
            OnInventoryChanged(slotIndex, itemType, newCount, oldSlot.Count, InventoryChangeType.Removed);
        }

        public int GetItemCount(ItemType itemType)
        {
            int count = 0;
            foreach (var slot in slots) if (slot.ItemType == itemType) count += slot.Count;
            return count;
        }

        public bool HasItem(ItemType itemType, int amount = 1)
        {
            return GetItemCount(itemType) >= amount;
        }

        public InventorySlot GetSlot(int index)
        {
            if (index < 0 || index >= slots.Count) return new InventorySlot();
            return slots[index];
        }

        public void SetSlot(int index, InventorySlot slot)
        {
            if (index < 0 || index >= slots.Count) return;
            var oldSlot = slots[index];
            slots[index] = slot;
            OnInventoryChanged(index, slot.ItemType, slot.Count, oldSlot.Count, InventoryChangeType.Swapped);
        }

        public void Clear()
        {
            for (int i = 0; i < slots.Count; i++)
            {
                var oldSlot = slots[i];
                if (!oldSlot.IsEmpty)
                {
                    slots[i] = new InventorySlot(ItemType.None, 0, oldSlot.MaxStack);
                    OnInventoryChanged(i, ItemType.None, 0, oldSlot.Count, InventoryChangeType.Cleared);
                }
            }
        }

        public string SaveToJson()
