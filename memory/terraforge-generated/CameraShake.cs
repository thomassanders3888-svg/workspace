using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour {
    public static CameraShake Instance { get; private set; }
    public float defaultDuration = 0.5f;
    public float defaultMagnitude = 0.5f;
    
    void Awake() { Instance = this; }
    
    public void Shake(float magnitude = -1, float duration = -1) {
        float mag = magnitude < 0 ? defaultMagnitude : magnitude;
        float dur = duration < 0 ? defaultDuration : duration;
        StartCoroutine(DoShake(mag, dur));
    }
    
    IEnumerator DoShake(float magnitude, float duration) {
        Vector3 originalPos = transform.localPosition;
        float elapsed = 0;
        
        while (elapsed < duration) {
            transform.localPosition = originalPos + Random.insideUnitSphere * magnitude;
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        transform.localPosition = originalPos;
    }
}
