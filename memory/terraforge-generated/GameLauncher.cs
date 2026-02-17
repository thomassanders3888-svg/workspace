using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLauncher : MonoBehaviour {
    public static GameLauncher Instance { get; private set; }
    public Canvas launcherCanvas;
    public LauncherPanel[] panels;
    
    public enum GameState { Launcher, Connecting, Loading, Playing }
    public GameState state = GameState.Launcher;
    
    void Awake() { Instance = this; DontDestroyOnLoad(gameObject); }
    
    void Start() {
        ShowPanel("MainMenu");
    }
    
    public void StartSinglePlayer(string worldName) {
        state = GameState.Loading;
        WorldPersistence.Instance.worldName = worldName;
        LoadGameScene();
    }
    
    public void StartMultiplayer(string serverAddress) {
        state = GameState.Connecting;
        ShowPanel("Connecting");
        NetworkClient.Instance.Connect(serverAddress, OnConnected, OnConnectionFailed);
    }
    
    void OnConnected() {
        state = GameState.Playing;
        LoadGameScene();
    }
    
    void OnConnectionFailed(string error) {
        state = GameState.Launcher;
        ShowPanel("ConnectionFailed");
    }
    
    void LoadGameScene() {
        SceneManager.LoadScene("GameWorld");
    }
    
    public void ShowPanel(string panelName) {
        foreach (var panel in panels) {
            panel.gameObject.SetActive(panel.panelName == panelName);
        }
    }
    
    public void QuitGame() {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}

public class LauncherPanel : MonoBehaviour {
    public string panelName;
}
