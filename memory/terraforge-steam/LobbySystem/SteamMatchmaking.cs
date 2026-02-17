using System;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

namespace TerraForge.Steamworks
{
    /// <summary>
    /// Handles Steam lobby search and matchmaking functionality.
    /// Filters by game mode, player count, distance, and custom properties.
    /// </summary>
    public class SteamMatchmaking : MonoBehaviour
    {
        public static SteamMatchmaking Instance { get; private set; }
        
        // Search results
        public List<LobbyInfo> FoundLobbies { get; private set; } = new();
        public bool IsSearching { get; private set; }
        
        // Search parameters
        [Header("Search Settings")]
        [Tooltip("Maximum number of results to return")]
        public int maxResults = 50;
        
        [Tooltip("Search distance for lobbies")]
        public ELobbyDistanceFilter distanceFilter = ELobbyDistanceFilter.k_ELobbyDistanceFilterWorldwide;
        
        // Events
        public event Action<List<LobbyInfo>> OnLobbiesFound;
        public event Action<int> OnSearchComplete;
        public event Action<string> OnSearchError;
        public event Action OnSearchStarted;
        
        // Steam callbacks
        private Callback<LobbyMatchList_t> _lobbyMatchListCallback;
        private Callback<LobbyDataUpdate_t> _lobbyDataUpdateCallback;
        private CallResult<LobbyMatchList_t> _lobbyMatchListCallResult;
        
        private SteamAPICall_t _currentSearchHandle;
        private int _pendingDataUpdates = 0;
        private string _targetGameMode = "";
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        private void OnEnable()
        {
            if (!SteamManager.Initialized)
            {
                Debug.LogError("Steam not initialized!");
                return;
            }
            
            _lobbyMatchListCallback = Callback<LobbyMatchList_t>.Create(OnLobbyMatchList);
            _lobbyDataUpdateCallback = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataUpdate);
        }
        
        private void OnDisable()
        {
            _lobbyMatchListCallback?.Dispose();
            _lobbyDataUpdateCallback?.Dispose();
        }
        
        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
        
        #region Lobby Search
        
        /// <summary>
        /// Searches for available lobbies.
        /// </summary>
        public void SearchLobbies(string gameModeFilter = "", int minSlotsAvailable = 1)
        {
            if (!SteamManager.Initialized)
            {
                OnSearchError?.Invoke("Steam is not initialized");
                return;
            }
            
            if (IsSearching)
            {
                Debug.LogWarning("Already searching for lobbies");
                return;
            }
            
            IsSearching = true;
            FoundLobbies.Clear();
            _targetGameMode = gameModeFilter;
            
            // Set distance filter
            SteamMatchmaking.SetLobbyDistanceFilter(distanceFilter);
            
            // Add filters if specified
            if (!string.IsNullOrEmpty(gameModeFilter))
            {
                SteamMatchmaking.AddRequestLobbyListStringFilter("gamemode", 
                    gameModeFilter, ELobbyComparison.k_ELobbyComparisonEqual);
            }
            
            // Filter for lobbies with available slots
            SteamMatchmaking.AddRequestLobbyListFilterSlotsAvailable(minSlotsAvailable);
            
            // Set result limit
            SteamMatchmaking.AddRequestLobbyListResultCountFilter(maxResults);
            
            OnSearchStarted?.Invoke();
            
            SteamAPICall_t handle = SteamMatchmaking.RequestLobbyList();
            _lobbyMatchListCallResult = CallResult<LobbyMatchList_t>.Create(OnLobbyMatchListResult);
            _lobbyMatchListCallResult.Set(handle);
            
            Debug.Log("Searching for lobbies...");
        }
        
        /// <summary>
        /// Searches for lobbies hosting a specific map or level.
        /// </summary>
        public void SearchLobbiesByMap(string mapName, string gameMode = "")
        {
            if (!SteamManager.Initialized) return;
            
            IsSearching = true;
            FoundLobbies.Clear();
            
            SteamMatchmaking.AddRequestLobbyListStringFilter("map", mapName, 
                ELobbyComparison.k_ELobbyComparisonEqual);
            
            if (!string.IsNullOrEmpty(gameMode))
            {
                SteamMatchmaking.AddRequestLobbyListStringFilter("gamemode", 
                    gameMode, ELobbyComparison.k_ELobbyComparisonEqual);
            }
            
            SteamMatchmaking.AddRequestLobbyListResultCountFilter(maxResults);
            
            OnSearchStarted?.Invoke();
            
            SteamAPICall_t handle = SteamMatchmaking.RequestLobbyList();
            _lobbyMatchListCallResult = CallResult<LobbyMatchList_t>.Create(OnLobbyMatchListResult);
            _lobbyMatchListCallResult.Set(handle);
        }
        
        /// <summary>
        /// Searches for lobbies with friends in them.
        /// </summary>
        public void SearchLobbiesWithFriends()
        {
            if (!SteamManager.Initialized) return;
            
            IsSearching = true;
            FoundLobbies.Clear();
            
            // This queries lobbies where friends are present
            OnSearchStarted?.Invoke();
            
            int friendCount = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate);
            Debug.Log($"Checking {friendCount} friends for lobbies...");
            
            // Steam doesn't have a direct " lobby with friends" query, 
            // so we do a regular search and filter later
            SteamAPICall_t handle = SteamMatchmaking.RequestLobbyList();
            _lobbyMatchListCallResult = CallResult<LobbyMatchList_t>.Create(OnLobbyMatchListResult);
            _lobbyMatchListCallResult.Set(handle);
            
            // We'll filter for friends after results come in
            _pendingDataUpdates = 0;
        }
        
        private void OnLobbyMatchList(LobbyMatchList_t result)
        {
            // Legacy callback - not used with CallResult
        }
        
        private void OnLobbyMatchListResult(LobbyMatchList_t result, bool bIOFailure)
        {
            if (bIOFailure)
            {
                Debug.LogError("Lobby search failed");
                OnSearchError?.Invoke("Search failed due to IO error");
                IsSearching = false;
                return;
            }
            
            int lobbyCount = (int)result.m_nLobbiesMatching;
            Debug.Log($"Found {lobbyCount} lobbies");
            
            FoundLobbies.Clear();
            _pendingDataUpdates = 0;
            
            for (int i = 0; i < lobbyCount; i++)
            {
                CSteamID lobbyId = SteamMatchmaking.GetLobbyByIndex(i);
                
                // Request full data for this lobby
                if (SteamMatchmaking.RequestLobbyData(lobbyId))
                {
                    _pendingDataUpdates++;
                }
                
                // Create initial info from available data
                var info = new LobbyInfo
                {
                    LobbyId =