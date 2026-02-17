using UnityEngine;
using System.Collections.Generic;

public class ParticleManager : MonoBehaviour {
    public static ParticleManager Instance { get; private set; }
    
    [System.Serializable]
    public class ParticleEffect {
        public string effectId;
        public ParticleSystem prefab;
        public int poolSize = 10;
        public bool autoDestroy = true;
    }
    
    public ParticleEffect[] effects;
    private Dictionary<string, Queue<ParticleSystem>> pools = new();
    
    void Awake() {
        Instance = this;
        InitializePools();
    }
    
    void InitializePools() {
        foreach (var effect in effects) {
            Queue<ParticleSystem> pool = new();
            for (int i = 0; i < effect.poolSize; i++) {
                ParticleSystem ps = Instantiate(effect.prefab, transform);
                ps.gameObject.SetActive(false);
                pool.Enqueue(ps);
            }
            pools[effect.effectId] = pool;
        }
    }
    
    public void Play(string effectId, Vector3 position, Quaternion rotation) {
        if (!pools.ContainsKey(effectId)) return;
        
        var pool = pools[effectId];
        if (pool.Count == 0) return;
        
        ParticleSystem ps = pool.Dequeue();
        ps.transform.position = position;
        ps.transform.rotation = rotation;
        ps.gameObject.SetActive(true);
        ps.Play(true);
        
        StartCoroutine(ReturnToPoolCoroutine(effectId, ps));
    }
    
    public void PlayBlockBreak(BlockType blockType, Vector3 position) {
        string effectId = blockType switch {
            BlockType.Grass or BlockType.Dirt => "dirt_break",
            BlockType.Stone => "stone_break",
            BlockType.Wood => "wood_break",
            BlockType.Leaves => "leaves_break",
            _ => "generic_break"
        };
        Play(effectId, position, Quaternion.identity);
    }
    
    public void PlayBlockPlace(BlockType blockType, Vector3 position) {
        Play("block_place", position, Quaternion.identity);
    }
    
    public void PlayFootstep(Vector3 position, BlockType groundType) {
        string effectId = groundType switch {
            BlockType.Grass or BlockType.Dirt => "footstep_grass",
            BlockType.Stone or BlockType.Gravel => "footstep_stone",
            BlockType.Wood => "footstep_wood",
            BlockType.Sand => "footstep_sand",
            _ => "footstep_generic"
        };
        Play(effectId, position, Quaternion.identity);
    }
    
    System.Collections.IEnumerator ReturnToPoolCoroutine(string effectId, ParticleSystem ps) {
        yield return new WaitWhile(() => ps.isPlaying);
        ps.gameObject.SetActive(false);
        if (pools.ContainsKey(effectId)) pools[effectId].Enqueue(ps);
    }
}
