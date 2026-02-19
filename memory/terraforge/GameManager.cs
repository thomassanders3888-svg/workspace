using System;
using UnityEngine;

namespace TerraForge
{
    /// <summary>
    /// GameManager - Singleton controller for managing game state and lifecycle.
    /// Handles transitions between different game states and broadcasts state changes.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        #region Singleton Pattern
        
        /// <summary>
        /// Static instance for global access to the GameManager.
        /// </summary>
        private static GameManager _instance;
        
        /// <summary>
        /// Public accessor for the GameManager singleton instance.
        /// Creates the instance if it doesn't exist.
        /// </summary>
        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<GameManager>();
                    
                    if (_instance == null)
                    {
                        GameObject singletonObject = new GameObject("GameManager");
                        _instance = singletonObject.AddComponent<GameManager>();
                    }
                }
                return _instance;
            }
        }
        
        /// <summary>
        /// Ensures only one GameManager exists and persists across scene loads.
        /// </summary>
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        #endregion
        
        #region Game State Enum
        
        /// <summary>
        /// Enumeration of possible game states.
        /// </summary>
        public enum GameState
        {
            /// <summary>Player is in the main menu.</summary>
            MainMenu,
            
            /// <summary>Game is currently loading.</summary>
            Loading,
            
            /// <summary>Gameplay is active and running.</summary>
            Playing,
            
            /// <summary>Game is paused.</summary>
            Paused
        }
        
        #endregion
        
        #region Properties
        
        /// <summary>
        /// Current state of the game.
        /// Triggers OnGameStateChanged event when modified.
        /// </summary>
        [SerializeField]
        private GameState _currentGameState = GameState.MainMenu;
        
        /// <summary>
        /// Gets or sets the current game state.
        /// Setting this value will trigger the OnGameStateChanged event.
        /// </summary>
        public GameState CurrentGameState
        {
            get => _currentGameState;
            private set
            {
                if (_currentGameState != value)
                {
                    GameState previousState = _currentGameState;
                    _currentGameState = value;
                    OnGameStateChanged?.Invoke(previousState, _currentGameState);
                }
            }
        }
        
        #endregion
        
        #region Events
        
        /// <summary>
        /// Event invoked when the game state changes.
        /// Parameters: previous state, new state
        /// </summary>
        public event Action<GameState, GameState> OnGameStateChanged;
        
        /// <summary>
        /// Event invoked when the player is spawned.
        /// Parameter: the spawned player GameObject
        /// </summary>
        public event Action<GameObject> OnPlayerSpawned;
        
        #endregion
        
        #region Game Control Methods
        
        /// <summary>
        /// Loads the game scene and transitions to Loading state.
        /// Use this to initiate the game loading process.
        /// </summary>
        public void LoadGame()
        {
            Debug.Log("[GameManager] Loading game...");
            CurrentGameState = GameState.Loading;
            
            // TODO: Implement scene loading logic
            // SceneManager.LoadSceneAsync("GameScene");
        }
        
        /// <summary>
        /// Starts the game and transitions to Playing state.
        /// Called after loading is complete.
        /// </summary>
        public void StartGame()
        {
            Debug.Log("[GameManager] Starting game...");
            CurrentGameState = GameState.Playing;
            
            // TODO: Implement game start logic
            // - Spawn player
            // - Initialize game systems
            // - Start gameplay systems
        }
        
        /// <summary>
        /// Pauses the game and transitions to Paused state.
        /// Freezes gameplay but keeps UI interactive.
        /// </summary>
        public void PauseGame()
        {
            if (CurrentGameState == GameState.Playing)
            {
                Debug.Log("[GameManager] Game paused.");
                CurrentGameState = GameState.Paused;
                
                // TODO: Implement pause logic
                // Time.timeScale = 0f;
                // Show pause menu UI
            }
        }
        
        /// <summary>
        /// Resumes the game and transitions back to Playing state.
        /// </summary>
        public void ResumeGame()
        {
            if (CurrentGameState == GameState.Paused)
            {
                Debug.Log("[GameManager] Game resumed.");
                CurrentGameState = GameState.Playing;
                
                // TODO: Implement resume logic
                // Time.timeScale = 1f;
                // Hide pause menu UI
            }
        }
        
        #endregion
        
        #region Player Management
        
        /// <summary>
        /// Spawns the player and invokes the OnPlayerSpawned event.
        /// Call this after the game scene is loaded.
        /// </summary>
        /// <param name="playerPrefab">The player prefab to spawn.</param>
        /// <param name="spawnPosition">The position to spawn the player at.</param>
        /// <returns>The spawned player GameObject.</returns>
        public GameObject SpawnPlayer(GameObject playerPrefab, Vector3 spawnPosition)
        {
            if (playerPrefab == null)
            {
                Debug.LogError("[GameManager] Player prefab is null!");
                return null;
            }
            
            GameObject player = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
            Debug.Log($"[GameManager] Player spawned at {spawnPosition}");
            
            OnPlayerSpawned?.Invoke(player);
            return player;
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Returns to the main menu.
        /// Transitions state to MainMenu and unloads game scenes.
        /// </summary>
        public void ReturnToMainMenu()
        {
            Debug.Log("[GameManager] Returning to main menu...");
            CurrentGameState = GameState.MainMenu;
            
            // TODO: Implement return to menu logic
            // SceneManager.LoadScene("MainMenuScene");
        }
        
        /// <summary>
        /// Checks if the game is currently in gameplay state.
        /// </summary>
        /// <returns>True if game state is Playing or Paused.</returns>
        public bool IsInGameplay()
        {
            return CurrentGameState == GameState.Playing || 
                   CurrentGameState == GameState.Paused;
        }
        
        #endregion
    }
}