using UnityEngine;
using System.Collections.Generic;

public enum ResourceType { Wood, Stone, Ore, Clay, Sand }

[System.Serializable]
public class ResourceStack {
    public ResourceType type;
    public int amount;
    public int maxStack = 64;
    
    public bool CanAdd(int qty) => amount + qty <= maxStack;
    public void Add(int qty) => amount = Mathf.Min(amount + qty, maxStack);
    public bool Remove(int qty) { if (amount >= qty) { amount -= qty; return true; } return false; }
}

public class ResourceManager : MonoBehaviour {
    public static ResourceManager Instance { get; private set; }
    public Dictionary<ResourceType, int> globalResources = new();
    
    void Awake() { Instance = this; }
    
    public void GatherResource(ResourceType type, int amount) {
        if (!globalResources.ContainsKey(type)) globalResources[type] = 0;
        globalResources[type] += amount;
    }
}
