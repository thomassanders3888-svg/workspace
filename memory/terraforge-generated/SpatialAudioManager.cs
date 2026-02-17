using UnityEngine;
using UnityEngine.Audio;

public class SpatialAudioManager : MonoBehaviour {
    public static SpatialAudioManager Instance { get; private set; }
    public AudioMixerGroup spatialMixer;
    public int maxSimultaneousSounds = 32;
    public float maxHearingDistance = 50f;
    
    private Transform listener;
    private ObjectPool<AudioSource> audioSourcePool;
    
    void Awake() {
        Instance = this;
        listener = Camera.main.transform;
        InitializePool();
    }
    
    void InitializePool() {
        audioSourcePool = new ObjectPool<AudioSource>(maxSimultaneousSounds, CreateAudioSource);
    }
    
    AudioSource CreateAudioSource() {
        GameObject obj = new GameObject("PooledAudioSource");
        obj.transform.SetParent(transform);
        AudioSource source = obj.AddComponent<AudioSource>();
        source.outputAudioMixerGroup = spatialMixer;
        source.spatialBlend = 1f; // Full 3D
        source.rolloffMode = AudioRolloffMode.Linear;
        source.maxDistance = maxHearingDistance;
        return source;
    }
    
    public void PlaySound(string soundId, Vector3 position) {
        float distance = Vector3.Distance(listener.position, position);
        if (distance > maxHearingDistance) return;
        
        AudioClip clip = AudioManager.Instance.GetClip(soundId);
        if (clip == null) return;
        
        AudioSource source = audioSourcePool.Get();
        if (source == null) return; // Pool exhausted
        
        source.transform.position = position;
        source.clip = clip;
        source.volume = Mathf.InverseLerp(maxHearingDistance, 0, distance);
        source.Play();
        
        StartCoroutine(ReturnToPool(source));
    }
    
    System.Collections.IEnumerator ReturnToPool(AudioSource source) {
        yield return new WaitWhile(() => source.isPlaying);
        audioSourcePool.Release(source);
    }
}

public class ObjectPool<T> where T : Component {
    private System.Collections.Generic.Queue<T> available = new();
    private System.Collections.Generic.HashSet<T> inUse = new();
    private int maxSize;
    private System.Func<T> factory;
    
    public ObjectPool(int maxSize, System.Func<T> factory) {
        this.maxSize = maxSize;
        this.factory = factory;
        for (int i = 0; i < maxSize / 2; i++) available.Enqueue(factory());
    }
    
    public T Get() {
        if (available.Count > 0) {
            T item = available.Dequeue();
            inUse.Add(item);
            return item;
        } else if (inUse.Count < maxSize) {
            T item = factory();
            inUse.Add(item);
            return item;
        }
        return null;
    }
    
    public void Release(T item) {
        if (inUse.Remove(item)) available.Enqueue(item);
    }
}
