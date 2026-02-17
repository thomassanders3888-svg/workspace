using System.Collections.Generic;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

namespace TerraForge.Steamworks
{
    /// <summary>
    /// UI Controller for the Steam lobby system.
    /// Handles display of lobby members, ready toggles, and game start.
    /// </summary>
    public class LobbyUI : MonoBehaviour
    {
        [Header("Panels")]
        public GameObject lobbyBrowserPanel;
        public GameObject lobbyRoomPanel;
        public GameObject searchPanel;
        
        [Header("Lobby Browser")]
        public Transform lobbyListContainer;
        public GameObject lobbyListItemPrefab;
        public Button refreshButton;
        public Button createLobbyButton;
        public Dropdown gameModeFilter;
        public Text statusText;
        
        [Header("Lobby Room")]
        public Transform memberListContainer;
        public GameObject memberItemPrefab;
        public Button readyButton;
        public Button startGameButton;
        public Button leaveLobbyButton;
        public Button inviteButton;
        public Text lobbyNameText;
        public Text lobbyInfoText;
        public Slider readyProgressBar;