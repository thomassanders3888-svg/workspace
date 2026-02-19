using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace TerraForge.UI
{
    /// <summary>
    /// Manages all in-game UI elements and HUD display
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("HUD Elements")]
        public Canvas mainCanvas;
        public RectTransform hudPanel;
        
        [Header("Health & Status")]
        public Slider healthBar;
        public Slider foodBar;
        public TextMeshProUGUI healthText;
        public TextMeshProUGUI foodText;
        
        [Header("Crosshair")]
        public Image crosshair;
        public Sprite crosshairNormal;
        public Sprite crosshairTarget;
        public Color crosshairDefaultColor = Color.white;
        public Color crosshairTargetColor = Color.red;
        
        [Header("Hotbar")]
        public HorizontalLayoutGroup hotbarContainer;
        public List<Image> hotbarSlots;
        public List<TextMeshProUGUI> hotbarCounts;
        public Image selectedHighlight;
        public Sprite emptySlotSprite;
        public Sprite selectedSlotSprite;
        
        [Header("Chat")]
        public ScrollRect chatScrollView;
        public RectTransform chatContent;
        public TextMeshProUGUI chatTextPrefab;
        public TMP_InputField chatInputField;
        public int maxChatMessages = 50;
        
        [Header("Pause Menu")]
        public GameObject pauseMenuPanel;
        public Button resumeButton;
        public Button settingsButton;
        public Button quitButton;
        
        [Header("Inventory")]
        public GameObject inventoryPanel;
        public GridLayoutGroup inventoryGrid;
        public List<Image> inventorySlots;
        public TextMeshProUGUI inventoryTitle;
        
        [Header("Debug")]
        public TextMeshProUGUI debugText;
        public GameObject debugPanel;
        
        private List<TextMeshProUGUI> chatMessages = new List<TextMeshProUGUI>();
        private int selectedHotbarIndex = 0;
        private bool isPaused = false;
        private bool chatActive = false;
        private bool inventoryActive = false;
        
        void Awake()
        {
            InitializeUI();
        }
        
        void Update()
        {
            HandleInput();
            UpdateCrosshair();
        }
        
        void InitializeUI()
        {
            // Setup pause menu callbacks
            if (resumeButton != null) resumeButton.onClick.AddListener(ResumeGame);
            if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
            if (quitButton != null) quitButton.onClick.AddListener(QuitToMenu);
            
            // Setup chat input
            if (chatInputField != null)
            {
                chatInputField.onSubmit.AddListener(OnChatSubmit);
            }
            
            // Hide panels initially
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
            if (inventoryPanel != null) inventoryPanel.SetActive(false);
            if (debugPanel != null) debugPanel.SetActive(false);
            
            // Initialize hotbar
            InitializeHotbar();
        }
        
        void InitializeHotbar()
        {
            for (int i = 0; i < hotbarSlots.Count; i++)
            {
                if (hotbarSlots[i] != null)
                {
                    hotbarSlots[i].sprite = emptySlotSprite;
                    hotbarSlots[i].color = Color.gray;
                }
                if (hotbarCounts[i] != null)
                {
                    hotbarCounts[i].text = "";
                }
            }
            UpdateHotbarSelection(0);
        }
        
        void HandleInput()
        {
            // Number keys for hotbar
            for (int i = 0; i < 9; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    UpdateHotbarSelection(i);
                }
            }
            
            // Scroll wheel
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                int newIndex = selectedHotbarIndex + (scroll > 0 ? -1 : 1);
                newIndex = Mathf.Clamp(newIndex, 0, 8);
                UpdateHotbarSelection(newIndex);
            }
            
            // Pause toggle
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (chatActive) CloseChat();
                else if (inventoryActive) CloseInventory();
                else TogglePause();
            }
            
            // Chat toggle
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.T))
            {
                ToggleChat();
            }
            
            // Inventory toggle
            if (Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.E))
            {
                ToggleInventory();
            }
            
            // Debug toggle
            if (Input.GetKeyDown(KeyCode.BackQuote) || Input.GetKeyDown(KeyCode.F3))
            {
                ToggleDebug();
            }
        }
        
        void HandleInput()
        {
            // Number keys for hotbar
            for (int i = 0; i < 9; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    UpdateHotbarSelection(i);
                }
            }
            
            // Scroll wheel
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                int newIndex = selectedHotbarIndex + (scroll > 0 ? -1 : 1);
                newIndex = Mathf.Clamp(newIndex, 0, 8);
                UpdateHotbarSelection(newIndex);
            }
            
            // Pause toggle
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (chatActive) CloseChat();
                else if (inventoryActive) CloseInventory();
                else TogglePause();
            }
            
            // Chat toggle
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.T))
            {
                ToggleChat();
            }
            
            // Inventory toggle
            if (Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.E))
            {
                ToggleInventory();
            }
            
            // Debug toggle
            if (Input.GetKeyDown(KeyCode.BackQuote) || Input.GetKeyDown(KeyCode.F3))
            {
                ToggleDebug();
            }
        }
        
        public void UpdateHotbarSelection(int index)
        {
            selectedHotbarIndex = index;
            
            if (selectedHighlight != null && hotbarContainer != null)
            {
                RectTransform slot = hotbarContainer.transform.GetChild(index) as RectTransform;
                if (slot != null)
                {
                    selectedHighlight.transform.position = slot.position;
                }
            }
            
            OnHotbarChanged?.Invoke(index);
        }
        
        public void SetHotbarSlot(int index, Sprite icon, int count)
        {
            if (index < 0 || index >= hotbarSlots.Count) return;
            
            if (hotbarSlots[index] != null)
            {
                hotbarSlots[index].sprite = icon != null ? icon : emptySlotSprite;
                hotbarSlots[index].color = icon != null ? Color.white : Color.gray;
            }
            
            if (hotbarCounts[index] != null)
            {
                hotbar