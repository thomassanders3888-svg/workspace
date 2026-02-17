using System;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

namespace TerraForge.Steamworks
{
    /// <summary>
    /// Manages Steam lobbies - creation, joining, leaving, and member management.
    /// Handles all Steam callbacks and provides events for UI integration.
    /// </summary>
    public class SteamLobbyManager : MonoBehaviour
    {
        public static SteamLobbyManager Instance { get; private set; }

        // Current lobby state
        public CSteamID CurrentLobbyId { get; private set; }
        public bool IsInLobby => CurrentLobbyId.IsValid();
        public bool IsLobbyOwner => IsInLobby && SteamMatchmaking.GetLobbyOwner(CurrentLobbyId) == SteamUser.GetSteamID();
        
        // Lobby data cache
        public Dictionary<CSteamID, LobbyMemberData> LobbyMembers { get; private set; } = new();
        public int MaxPlayers { get; private set; }
        public string LobbyGameMode { get; private set; }
        
        // Events for UI
        public event Action<CSteamID> OnLobbyCreated;
        public event Action<CSteamID> OnLobbyJoined;
        public event Action OnLobbyLeft;
        public event Action<string> OnLobbyError;
        public event Action<CSteamID> OnMemberJoined;
        public event Action<CSteamID> OnMemberLeft;
        public event Action OnMemberDataUpdated;
        public event Action<LobbyChatUpdate_t> OnLobbyChatUpdate;
        public event Action<bool> OnReadyStateChanged;
        
        // Steam callback handles
        private Callback<LobbyCreated_t> _lobbyCreatedCallback;
        private Callback<LobbyEnter_t> _lobbyEnterCallback;
        private Callback<LobbyChatUpdate_t> _lobbyChatUpdateCallback;
        private Callback<LobbyDataUpdate_t> _lobbyDataUpdateCallback;
        private Callback<GameLobbyJoinRequested_t> _gameLobbyJoinRequestedCallback;
        
        // Ready states
        private Dictionary<CSteamID, bool> _memberReadyStates = new();
        
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
                OnLobbyError?.Invoke("Steam is not initialized");
                return;
            }
            
            _lobbyCreatedCallback = Callback<LobbyCreated_t>.Create(OnLobbyCreatedCallback);
            _lobbyEnterCallback = Callback<LobbyEnter_t>.Create(OnLobbyEnterCallback);
            _lobbyChatUpdateCallback = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdateCallback);
            _lobbyDataUpdateCallback = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataUpdateCallback);
            _gameLobbyJoinRequestedCallback = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        }
        
        private void OnDisable()
        {
            _lobbyCreatedCallback?.Dispose();
            _lobbyEnterCallback?.Dispose();
            _lobbyChatUpdateCallback?.Dispose();
            _lobbyDataUpdateCallback?.Dispose();
            _gameLobbyJoinRequestedCallback?.Dispose();
        }
        
        private void OnDestroy()
        {
            LeaveLobby();
            if (Instance == this) Instance = null;
        }
        
        #region Lobby Creation
        
        public void CreateLobby(int maxPlayers, ELobbyType lobbyType = ELobbyType.k_ELobbyTypePublic, string gameMode = "default")
        {
            if (!SteamManager.Initialized)
            {
                OnLobbyError?.Invoke("Steam is not initialized");
                return;
            }
            
            if (IsInLobby)
            {
                OnLobbyError?.Invoke("Already in a lobby. Leave current lobby first.");
                return;
            }
            
            if (maxPlayers < 2 || maxPlayers > 250)
            {
                OnLobbyError?.Invoke("Max players must be between 2 and 250");
                return;
            }
            
            MaxPlayers = maxPlayers;
            LobbyGameMode = gameMode;
            
            SteamMatchmaking.CreateLobby(lobbyType, maxPlayers);
            Debug.Log($"Creating lobby with max {maxPlayers} players...");
        }
        
        private void OnLobbyCreatedCallback(LobbyCreated_t callback)
        {
            if (callback.m_eResult != EResult.k_EResultOK)
            {
                Debug.LogError($"Lobby creation failed: {callback.m_eResult}");
                OnLobbyError?.Invoke($"Failed to create lobby: {callback.m_eResult}");
                return;
            }
            
            CurrentLobbyId = new CSteamID(callback.m_ulSteamIDLobby);
            Debug.Log($"Lobby created: {CurrentLobbyId}");
            
            SteamMatchmaking.SetLobbyData(CurrentLobbyId, "game", Application.productName);
            SteamMatchmaking.SetLobbyData(CurrentLobbyId, "version", Application.version);
            SteamMatchmaking.SetLobbyData(CurrentLobbyId, "gamemode", LobbyGameMode);
            SteamMatchmaking.SetLobbyData(CurrentLobbyId, "host", SteamUser.GetSteamPersonaName());
            
            LobbyMembers.Clear();
            _memberReadyStates.Clear();
            AddMember(SteamUser.GetSteamID(), true);
            
            OnLobbyCreated?.Invoke(CurrentLobbyId);
        }
        
        #endregion
        
        #region Join Lobby
        
        public void JoinLobby(CSteamID lobbyId)
        {
            if (!SteamManager.Initialized)
            {
                OnLobbyError?.Invoke("Steam is not initialized");
                return;
            }
            
            if (IsInLobby)
            {
                if (CurrentLobbyId == lobbyId)
                {
                    OnLobbyError?.Invoke("Already in this lobby");
                    return;
                }
                LeaveLobby();
            }
            
            SteamMatchmaking.JoinLobby(lobbyId);
            Debug.Log($"Joining lobby {lobbyId}...");
        }
        
        private void OnLobbyEnterCallback(LobbyEnter_t callback)
        {
            if (callback.m_EChatRoomEnterResponse != (uint)EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
            {
                Debug.LogError($"Failed to enter lobby: {callback.m_EChatRoomEnterResponse}");
                OnLobbyError?.Invoke($"Failed to join lobby: {callback.m_EChatRoomEnterResponse}");
                return;
            }
            
            CurrentLobbyId = new CSteamID(callback.m_ulSteamIDLobby);
            MaxPlayers = SteamMatchmaking.GetLobbyMemberLimit(CurrentLobbyId);
            LobbyGameMode = SteamMatchmaking.GetLobbyData(CurrentLobbyId, "gamemode");
            
            Debug.Log($"Entered lobby {CurrentLobbyId}");
            
            RefreshMemberList();
            OnLobbyJoined?.Invoke(CurrentLobbyId);
        }
        
        #endregion
        
        #region Leave Lobby
        
        public void LeaveLobby()
        {
            if (!IsInLobby) return;
            
            SteamMatchmaking.LeaveLobby(CurrentLobbyId);
            Debug.Log($"Left lobby {CurrentLobbyId}");
            
            CurrentLobbyId = CSteamID.Nil;
            LobbyMembers.Clear();
            _memberReadyStates.Clear();
            
            OnLobbyLeft?.Invoke();
        }
        
        #endregion
        
        #region Invite Friends
        
        public void InviteFriend(CSteamID friendSteamId)
        {
            if (!IsInLobby)
            {
                OnLobbyError?.Invoke("Not in a lobby");
                return;
            }
            
            bool success = SteamMatchmaking.InviteUserToLobby(CurrentLobbyId, friendSteamId);
            if (success)
            {
                Debug.Log($"Invited {friendSteamId} to lobby");
            }
            else
            {
                OnLobbyError?.Invoke("Failed to send invitation");
            }
        }
        
        public void InviteFriend(string friendName)
        {
            int friendCount = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate);
            for (int i = 0; i < friendCount; i++)
            {
                CSteamID friendId = SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate);
                string name = SteamFriends.GetFriendPersonaName(friendId);
                if (name.Equals(friendName, StringComparison.OrdinalIgnoreCase))
                {
                    InviteFriend(friendId);
                    return;
                }
            }
            OnLobbyError?.Invoke($"Friend '{friendName}' not found");
        }
        
        #endregion
        
        #region Ready System
        
        public void SetReady(bool ready)
        {
            if (!IsInLobby)
            {
                OnLobbyError?.Invoke("Not in a lobby");
                return;
            }
            
            CSteamID localSteamId = SteamUser.GetSteamID();
            _memberReadyStates[localSteamId] = ready;
            SteamMatchmaking.SetLobbyMemberData(CurrentLobbyId, "ready", ready ? "1" : "0");
            OnReadyStateChanged?.Invoke(ready);
        }
        
        public bool IsPlayerReady(CSteamID playerId)
        {
            return _memberReadyStates.TryGetValue(playerId, out bool ready) && ready;
        }
        
        public bool GetLocalReadyState()
        {
            return _memberReadyStates.TryGetValue(SteamUser.GetSteamID(), out bool ready) && ready;
        }
        
        public bool AreAllPlayersReady()
        {
            foreach (var member in LobbyMembers.Keys)
            {
                if (!IsPlayerReady(member))
                    return false;
            }
            return LobbyMembers.Count > 0;
        }
        
        public int GetReadyCount()
        {
            int count = 0;
            foreach (var member in LobbyMembers.Keys)
            {
                if (IsPlayerReady(member))
                    count++;
            }
            return count;
        }
        
        #endregion
        
        #region Member Management
        
        private void RefreshMemberList()
        {
            LobbyMembers.Clear();
            int memberCount = SteamMatchmaking.GetNumLobbyMembers(CurrentLobbyId);
            
            for (int i = 0; i < memberCount; i++)
            {
                CSteamID memberId = SteamMatchmaking.GetLobbyMemberByIndex(CurrentLobbyId, i);
                AddMember(memberId, false);
            }
        }
        
        private void AddMember(CSteamID steamId, bool isHost)
        {
            string name = SteamFriends.GetFriendPersonaName(steamId);
            var data = new LobbyMemberData
            {
                SteamId = steamId,
                MemberName = name,
                IsHost = isHost || SteamMatchmaking.GetLobbyOwner(CurrentLobbyId) == steamId,
                JoinTime = DateTime.UtcNow
            };
            
            LobbyMembers[steamId] = data;
            _memberReadyStates[steamId] = false;
            OnMemberJoined?.Invoke(steamId);
            OnMemberDataUpdated?.Invoke();
        }
        
        private void RemoveMember(CSteamID steamId)
        {
            if (LobbyMembers.Remove(steamId))
            {
                _memberReadyStates.Remove(steamId);
                OnMemberLeft?.Invoke(steamId);
                OnMemberDataUpdated?.Invoke();
            }
        }
        
        private void OnLobbyChatUpdateCallback(LobbyChatUpdate_t callback)
        {
            CSteamID changedId = new CSteamID(callback.m_ulSteamIDUserChanged);
            CSteamID makingChangeId = new CSteamID(callback.m_ulSteamIDMakingChange);
            EChatMemberStateChange stateChange = (EChatMemberStateChange)callback.m_rgfChatMemberStateChange;
            
            Debug.Log($"Lobby chat update: {changedId} - {stateChange}");
            
            switch (stateChange)
            {
                case EChatMemberStateChange.k_EChatMemberStateChangeEntered:
                    if (changedId != SteamUser.GetSteamID())
                        AddMember(changedId, false);
                    break;
                    
                case EChatMemberStateChange.k_EChatMemberStateChangeLeft:
                case EChatMemberStateChange.k_EChatMemberStateChangeDisconnected:
                case EChatMemberStateChange.k_EChatMemberStateChangeKicked:
                case EChatMemberStateChange.k_EChatMemberStateChangeBanned:
                    RemoveMember(changedId);
                    break;
            }
            
            OnLobbyChatUpdate?.Invoke(callback);
        }
        
        private void OnLobbyDataUpdateCallback(LobbyDataUpdate_t callback)
        {
            CSteamID memberId = new CSteamID(callback.m_ulSteamIDMember);
            
            if (callback.m_bSuccess != 0 && IsInLobby)
            {
                if (memberId == CurrentLobbyId)
                {
                    // Lobby metadata updated
                    LobbyGameMode = SteamMatchmaking.GetLobbyData(CurrentLobbyId, "gamemode");
                }
                else
                {
                    // Member data updated
                    string readyStr = SteamMatchmaking.GetLobbyMemberData(CurrentLobbyId, memberId, "ready");
                    if (!string.IsNullOrEmpty(readyStr))
                    {
                        _memberReadyStates[memberId] = readyStr == "1";
                        OnMemberDataUpdated?.Invoke();
                    }
                }
            }
        }
        
        private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
        {
            Debug.Log($"Game lobby join requested from {callback.m_steamIDFriend}");
            JoinLobby(callback.m_steamIDLobby);
        }
        
        #endregion
        
        #region Utility
        
        public void SetLobbyData(string key, string value)
        {
            if (IsLobbyOwner)
            {
                SteamMatchmaking.SetLobbyData(CurrentLobbyId, key, value);
            }
        }
        
        public string GetLobbyData(string key)
        {
            if (IsInLobby)
            {
                return SteamMatchmaking.GetLobbyData(CurrentLobbyId, key);
            }
            return "";
        }
        
        public void SetLobbyJoinable(bool joinable)
        {
            if (IsLobbyOwner)
            {
                SteamMatchmaking.SetLobbyJoinable(CurrentLobbyId, joinable);
            }
        }
        
        /// <summary>
        /// Called by host to start the game when everyone is ready.
        /// </summary>
        public void StartGame()
        {
            if (!IsLobbyOwner)
            {
                OnLobbyError?.Invoke("Only the host can start the game");
                return;
            }
            
            if (!AreAllPlayersReady())
            {
                OnLobbyError?.Invoke("Not all players are ready");
                return;
            }
            
            // Mark lobby as not joinable while game is in progress
            SetLobbyJoinable(false);
            SetLobbyData("game_started", "1");
            
            Debug.Log("Starting game!");
            // TODO: Transition to gameplay scene
        }
        
        /// <summary>
        /// Send a chat message to the lobby.
        /// </summary>
        public bool SendChatMessage(string message)
        {
            if (!IsInLobby || string.IsNullOrEmpty(message))
                return false;
                
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(message);
            return SteamMatchmaking.SendLobbyChatMsg(CurrentLobbyId, bytes, bytes.Length);
        }
        
        #endregion
    }
    
    /// <summary>
    /// Data structure for lobby member information.
    /// </summary>
    [Serializable]
    public class LobbyMemberData
    {
        public CSteamID SteamId;
        public string MemberName;
        public bool IsHost;
        public bool IsReady;
        public DateTime JoinTime;
        public int Ping;
    }
}