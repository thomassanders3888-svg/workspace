 ```csharp
using UnityEngine;
using TMPro;

public class ChatSystem : MonoBehaviour
{
    public TMP_InputField chatInput;
    public ScrollRect chatHistory;
    public GameObject messagePrefab;
    public Transform messageContainer;

    private void SendChat()
    {
        if (!string.IsNullOrEmpty(chatInput.text))
        {
            DisplayMessage("You: " + chatInput.text);
            // Simulate sending chat to server
            chatInput.text = "";
        }
    }

    private void ReceiveChat(string message)
    {
        DisplayMessage(message);
    }

    private void DisplayMessage(string message)
    {
        GameObject newMessage = Instantiate(messagePrefab, messageContainer);
        TextMeshProUGUI textComponent = newMessage.GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = message;
        }
        ScrollToBottom();
    }

    private void ScrollToBottom()
    {
        chatHistory.verticalNormalizedPosition = 0f;
    }

    public void OnSendButtonClicked()
    {
        SendChat();
    }

    public void OnReceiveMessage(string message)
    {
        ReceiveChat(message);
    }
}
```
