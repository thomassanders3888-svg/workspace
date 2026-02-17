using UnityEngine;

public class SoundEmitter : MonoBehaviour {
    public AudioClip[] clips;
    public float volume = 1f;
    public float pitch = 1f;
    public bool playOnAwake;
    public bool loop;
    
    private AudioSource source;
    
    void Awake() {
        source = gameObject.AddComponent<AudioSource>();
        source.spatialBlend = 1f;
    }
    
    void Start() { if (playOnAwake) Play(); }
    
    public void Play() {
        if (clips.Length == 0) return;
        source.clip = clips[Random.Range(0, clips.Length)];
        source.volume = volume;
        source.pitch = pitch + Random.Range(-0.1f, 0.1f);
        source.loop = loop;
        source.Play();
    }
    
    public void PlayOneShot(AudioClip clip) { source.PlayOneShot(clip, volume); }
}
