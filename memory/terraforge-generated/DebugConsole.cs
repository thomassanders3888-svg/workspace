using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DebugConsole : MonoBehaviour {
    public GameObject consolePanel;
    public TMP_InputField inputField;
    public TextMeshProUGUI outputText;
    public ScrollRect scrollRect;
    private bool isVisible = false;
    
    void Update() {
        if (Input.GetKeyDown(KeyCode.BackQuote)) ToggleConsole();
    }
    
    void ToggleConsole() {
        isVisible = !isVisible;
        consolePanel.SetActive(isVisible);
        if (isVisible) inputField.ActivateInputField();
    }
    
    public void Log(string message) {
        outputText.text += "\n" + message;
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0;
    }
}
