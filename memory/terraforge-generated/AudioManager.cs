 

```
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Sound Effects")]
    public Dictionary<string, AudioClip> soundEffects;
    public AudioSource[] soundEffectPool;

    [Header("Ambient Music")]
    public AudioClip ambientMusic;
    public float musicVolume = 0.5f;

    private AudioSource musicSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        InitializeSoundEffectPool();
        PlayAmbientMusic();
    }

    void InitializeSoundEffectPool()
    {
        soundEffectPool = new AudioSource[soundEffects.Count];
        for (int i = 0; i < soundEffectPool.Length; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.clip = soundEffects.ElementAt(i).Value;
            source.transform.parent = transform;
            source.gameObject.SetActive(false);
            soundEffectPool[i] = source;
        }
    }

    void PlayAmbientMusic()
    {
        if (ambientMusic != null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.clip = ambientMusic;
            musicSource.volume = musicVolume;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void PlaySound(string name, Vector3 position)
    {
        if (!soundEffects.ContainsKey(name))
        {
            Debug.LogError("Sound effect not found: " + name);
            return;
        }

        AudioSource source = GetAvailableSoundEffectSource();
        if (source != null)
        {
            source.transform.position = position;
            source.PlayOneShot(soundEffects[name]);
        }
    }

    private AudioSource GetAvailableSoundEffectSource()
    {
        foreach (AudioSource source in soundEffectPool)
        {
            if (!source.gameObject.activeInHierarchy)
            {
                return source;
            }
        }
        Debug.LogWarning("No available sound effect sources.");
        return null;
    }

    public void SetVolume(float volume)
    {
        if (volume < 0 || volume > 1)
        {
            Debug.LogError("Volume must be between 0 and 1. Setting to default value of 0.5f.");
            musicVolume = 0.5f;
        }
        else
        {
            musicVolume = volume;
        }

        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }
    }

    public void PlayFootstep()
    {
        PlaySound("footstep", Vector3.zero);
    }

    public void PlayBlockPlace()
    {
        PlaySound("blockPlace", Vector3.zero);
    }

    public void PlayBlockBreak()
    {
        PlaySound("blockBreak", Vector3.zero);
    }
}
```

This code snippet provides a complete implementation of an AudioManager singleton in Unity C#. It includes methods for managing sound effects, ambient music, and volume control. The `PlaySound` method allows you to play a specific sound effect at a given position. Additionally, there are convenience methods for playing specific types of sounds like footsteps, block placements, and breaks. Error handling is included to manage cases where sound effects are not found or volume values are out of range.

To use this script, you need to create an empty GameObject in your scene and
