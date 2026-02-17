using UnityEngine;
using TMPro;

public class FpsCounter : MonoBehaviour {
    public TextMeshProUGUI fpsText;
    private float deltaTime = 0.0f;
    
    void Update() {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;
        fpsText.text = Mathf.Round(fps) + " FPS";
    }
}
