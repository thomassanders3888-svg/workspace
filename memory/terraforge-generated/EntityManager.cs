using UnityEngine;
using System.Collections.Generic;
using System;

public class EntityManager : MonoBehaviour {
    public static EntityManager Instance { get; private set; }
    
    private Dictionary<Guid, Entity> entities = new();
    private Dictionary<Guid, GameObject> entityObjects = new();
    
    void Awake() { Instance = this; }
    
    public Guid SpawnEntity(string type, Vector3 position, Quaternion rotation) {
        var id = Guid.NewGuid();
        var entity = new Entity {
            Id = id,
            Type = type,
            Position = position,
            Rotation = rotation,
            Health = GetEntityMaxHealth(type)
        };
        
        entities[id] = entity;
        
        var prefab = Resources.Load<GameObject>($"Entities/{type}");
        if (prefab != null) {
            var obj = Instantiate(prefab, position, rotation);
            entityObjects[id] = obj;
        }
        
        NetworkClient.Instance.SendEntitySpawn(id, type, position, rotation);
        return id;
    }
    
    public void DestroyEntity(Guid id) {
        if (entityObjects.TryGetValue(id, out var obj)) {
            Destroy(obj);
            entityObjects.Remove(id);
        }
        entities.Remove(id);
        
        NetworkClient.Instance.SendEntityDestroy(id);
    }
    
    public void UpdateEntityState(Guid id, Vector3 pos, Quaternion rot, float health) {
        if (entities.TryGetValue(id, out var entity)) {
            entity.Position = pos;
            entity.Rotation = rot;
            entity.Health = health;
        }
        
        if (entityObjects.TryGetValue(id, out var obj)) {
            obj.transform.SetPositionAndRotation(pos, rot);
        }
    }
    
    public void DamageEntity(Guid id, float damage, Guid attackerId) {
        if (!entities.TryGetValue(id, out var entity)) return;
        
        entity.Health -= damage;
        
        if (entity.Health <= 0) {
            entity.Health = 0;
            OnEntityDeath(id, attackerId);
        }
        
        NetworkClient.Instance.SendEntityDamage(id, damage, attackerId);
    }
    
    void OnEntityDeath(Guid id, Guid killerId) {
        // Drop loot
        if (entities.TryGetValue(id, out var entity)) {
            var drops = GetLootDrops(entity.Type);
            foreach (var drop in drops) {
                SpawnItem(drop.itemType, entity.Position + Vector3.up * 0.5f, drop.count);
            }
        }
        
        DestroyEntity(id);
        
        if (killerId != Guid.Empty) {
            SkillsSystem.Instance?.Combat.AddXP(GetKillXP(entity.Type));
        }
    }
    
    void SpawnItem(string type, Vector3 position, int count) {
        // Drop item pickup on ground
    }
    
    float GetEntityMaxHealth(string type) {
        return type switch {
            "Goblin" => 50,
            "Orc" => 100,
            "Boss" => 1000,
            _ => 20
        };
    }
    
    int GetKillXP(string type) {
        return type switch {
            "Goblin" => 10,
            "Orc" => 25,
            "Boss" => 500,
            _ => 5
        };
    }
    
    System.Collections.Generic.List<LootDrop> GetLootDrops(string type) {
        var drops = new System.Collections.Generic.List<LootDrop>();
        // Determine loot based on type
        return drops;
    }
}

public class Entity {
    public Guid Id { get; set; }
    public string Type { get; set; }
    public Vector3 Position { get; set; }
    public Quaternion Rotation { get; set; }
    public float Health { get; set; }
}

public class LootDrop {
    public string itemType;
    public int count;
}
