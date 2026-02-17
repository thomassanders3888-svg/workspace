using UnityEngine;
using TMPro;
using System.Collections;

public class DamageNumbers : MonoBehaviour {
    public TextMeshProUGUI text;
    public float moveSpeed = 1f;
    public float fadeDuration = 1.5f;
    
    public void Show(float damage) {
        text.text = damage.ToString("F0");
        StartCoroutine(Animate());
    }
    
    IEnumerator Animate() {
        float elapsed = 0;
        Vector3 startPos = transform.position;
        while (elapsed < fadeDuration) {
            transform.position = startPos + Vector3.up * (elapsed * moveSpeed);
            text.alpha = 1 - (elapsed / fadeDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }
}
