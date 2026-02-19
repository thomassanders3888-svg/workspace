using UnityEngine;
using System;
using System.Collections;

namespace TerraForge
{
    /// <summary>
    /// Main entry point and game manager for TerraForge
    /// Handles initialization, game loop, and shutdown
    /// </summary>
    public class Main : MonoBehaviour
    {
        public static Main Instance { get; private set; }
        
        [Header("Managers")]
        public GameManager GameManager;
        public ChunkLoadingSystem ChunkSystem;
        public PlayerController Player;
        
        [Header("Server Settings")]
        public string serverAddress = "localhost";
        public int serverPort = 7777;
        public bool connectOnStart = false;
        
        [Header("Debug")]
        public bool enableDebugConsole = true;
        public bool logToFile = true;
        
        // State
        private bool isShuttingDown = false;
        private bool initialized = false;
        
        void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning("[Main] Another instance exists, destroying this one");
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            SetupLogging();
            
            Debug.Log($"[TerraForge] Main initialized at {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        }
        
        void Start()
        {
            StartCoroutine(InitializeAsync());
        }
        
        /// <summary>
        /// Async initialization sequence
        /// </summary>
        private IEnumerator InitializeAsync()
        {
            Debug.Log("[TerraForge] Starting initialization...");
            
            // Phase 1: Load player preferences
            yield return StartCoroutine(LoadPlayerPrefs());
            
            // Phase 2: Validate required systems
            if (!RequiredSystemsReady())
            {
                Debug.LogError("[TerraForge] Required systems missing! Cannot continue.");
                yield break;
            }
            
            // Phase 3: Initialize services
            yield return StartCoroutine(InitializeServices());
            
            // Phase 4: Connect or show main menu
            if (connectOnStart)
            {
                yield StartCoroutine(ConnectToServer());
            }
            else
            {
                GameManager?.LoadGame();
            }
            
            initialized = true;
            Debug.Log("[TerraForge] Initialization complete!");
        }
        
        private IEnumerator LoadPlayerPrefs()
        {
            Debug.Log("[Main] Loading player preferences...");
            
            Settings.LoadSettings();
            
            // Apply loaded settings
            Application.targetFrameRate = Settings.TargetFrameRate;
            QualitySettings.SetQualityLevel(Settings.GraphicsQuality);
            AudioListener.volume = Settings.MasterVolume;
            
            yield return null;
        }
        
        private bool RequiredSystemsReady()
        {
            if (GameManager == null)
            {
                Debug.LogError("[Main] GameManager not assigned!");
                return false;
            }
            
            if (ChunkSystem == null)
            {
                Debug.LogError("[Main] ChunkLoadingSystem not assigned!");
                return false;
            }
            
            return true;
        }
        
        private IEnumerator InitializeServices()
        {
            Debug.Log("[Main] Initializing services...");
            
            // Initialize event handlers
            SetupEventHandlers();
            
            // Initialize audio (commented out until AudioManager is ready)
            // InitializeAudioManager();
            
            yield return null;
        }
        
        private void SetupEventHandlers()
        {
            if (GameManager != null)
            {
                GameManager.OnGameStateChanged += HandleGameStateChanged;
                GameManager.OnPlayerSpawned += HandlePlayerSpawned;
            }
        }
        
        private void HandleGameStateChanged(GameState previousState, GameState newState)
        {
            Debug.Log($"[Main] Game state changed: {previousState} -> {newState}");
            
            switch (newState)
            {
                case GameState.Playing:
                    if (Player == null && connectOnStart)
                    {
                        SpawnLocalPlayer();
                    }
                    break;
                    
                case GameState.Paused:
                    // Pause music, reduce physics updates
                    Time.timeScale = 0;
                    break;
                    
                case GameState.Playing when Time.timeScale == 0:
                    Time.timeScale = 1;
                    break;
            }
        }
        
        private void HandlePlayerSpawned(PlayerController player)
        {
            Debug.Log($"[Main] Player spawned: {player.name}");
            Player = player;
        }
        
        private IEnumerator ConnectToServer()
        {
            Debug.Log($"[Main] Connecting to {serverAddress}:{serverPort}...");
            
            GameManager?.LoadGame();
            
            // Placeholder for actual connection logic
            yield return new WaitForSeconds(1f);
            
            Debug.Log("[Main] Connected to server");
            GameManager?.StartGame();
        }
        
        private void SpawnLocalPlayer()
        {
            Debug.Log("[Main] Spawning local player");
            // Delegated to GameManager
            GameManager?.SpawnPlayer(null, Vector3.zero);
        }
        
        void Update()
        {
            if (!initialized || isShuttingDown) return;
            
            // Debug console toggle
            if (enableDebugConsole && Input.GetKeyDown(KeyCode.BackQuote))
            {
                ToggleDebugConsole();
            }
            
            // Chunk updates based on player position
            if (Player != null && ChunkSystem != null)
            {
                ChunkSystem.UpdateChunks(Player.transform.position);
            }
        }
        
        void OnApplicationQuit()
        {
            Shutdown();
        }
        
        /// <summary>
        /// Graceful shutdown with save
        /// </summary>
        public void Shutdown()
        {
            if (isShuttingDown) return;
            isShuttingDown = true;
            
            Debug.Log("[TerraForge] Shutting down...");
            
            // Save before exit
            SaveGame();
            
            // Cleanup
            CleanupEventHandlers();
            Settings.SaveSettings();
            
            Debug.Log("[TerraForge] Goodbye!");
        }
        
        private void SaveGame()
        {
            if (!initialized) return;
            
            Debug.Log("[Main] Saving game state...");
            
            // Save player position
            if (Player != null)
            {
                PlayerPrefs.SetFloat("PlayerPosX", Player.transform.position.x);
                PlayerPrefs.SetFloat("PlayerPosY", Player.transform.position.y);
                PlayerPrefs.SetFloat("PlayerPosZ", Player.transform.position.z);
            }
            
            PlayerPrefs.Save();
        }
        
        private void CleanupEventHandlers()
        {
            if (GameManager != null)
