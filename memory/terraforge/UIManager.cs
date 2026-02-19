// UIManager.cs - TerraForge HUD Manager
// Handles: Crosshair, Hotbar, Health/Food bars, Chat, Pause menu, Inventory

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text;

namespace TerraForge.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Crosshair")]
        [SerializeField] private Image crosshairImage;
        [SerializeField] private Sprite defaultCrosshair;
        [SerializeField] private Sprite interactionCrosshair;
        [SerializeField] private Color crosshairDefaultColor = new Color(1, 1, 1, 0.8f);
        [SerializeField] private Color crosshairHighlightColor = new Color(1, 1, 0, 0.9f);

        [Header("Hotbar")]
        [SerializeField] private Transform hotbarContainer;
        [SerializeField] private GameObject hotbarSlotPrefab;
        [SerializeField] private int hotbarSlotCount = 10;
        private System.Collections.Generic.List<Image> hotbarIcons = new System.Collections.Generic.List<Image>();
        private System.Collections.Generic.List<Image> hotbarHighlights = new System.Collections.Generic.List<Image>();

        [Header("Health/Food Bars")]
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Slider foodSlider;
        [SerializeField] private Image healthBarFill;
        [SerializeField] private Image foodBarFill;
        [SerializeField] private Text healthText;
        [SerializeField] private Text foodText;

        [Header("Chat Panel")]
        [SerializeField] private ScrollRect chatScrollRect;
        [SerializeField] private Transform chatMessageContainer;
        [SerializeField] private GameObject chatMessagePrefab;
        [SerializeField] private InputField chatInputField;
        [SerializeField] private int maxChatMessages = 50;
        private System.Collections.Generic.Queue<GameObject> chatMessageQueue = new System.Collections.Generic.Queue<GameObject>();

        [Header("Pause Menu")]
        [SerializeField] private GameObject pauseMenuPanel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button saveQuitButton;

        [Header("Inventory")]
        [SerializeField] private GameObject inventoryPanel;

        [Header("Canvas References")]
        [SerializeField] private Canvas hudCanvas;
        [SerializeField] private Canvas inventoryCanvas;
        [SerializeField] private Canvas pauseCanvas;
        [SerializeField] private Canvas chatCanvas;

        private int selectedSlot = 0;
        private bool isPaused = false;
        private bool isInventoryOpen = false;
        private bool isChatOpen = false;
        private StringBuilder sb = new StringBuilder(32);

        public event System.Action<int> OnHotbarSlotChanged;
        public event System.Action<string> OnChatMessageSent;
        public event System.Action<bool> OnPauseStateChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            InitHotbar();
            SetupButtons();
            ShowHUDOnly();
            LockCursor(true);
        }

        private void Update()
        {
            HandleInput();
        }

        private void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (isChatOpen) ToggleChat();
                else if (isInventoryOpen) ToggleInventory();
                else TogglePause();
            }

            if (Input.GetKeyDown(KeyCode.E) && !isPaused)
                ToggleInventory();

            if (Input.GetKeyDown(KeyCode.T) && !isPaused && !isInventoryOpen)
            {
                ToggleChat();
                if (isChatOpen && chatInputField != null)
                    chatInputField.ActivateInputField();
            }

            for (int i = 0; i < 9 && i < hotbarSlotCount; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                    SelectHotbarSlot(i);
            }
            if (Input.GetKeyDown(KeyCode.Alpha0))
                SelectHotbarSlot(9);

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                int newSlot = selectedSlot + (scroll > 0 ? -1 : 1);
                if (newSlot < 0) newSlot = hotbarSlotCount - 1;
                if (newSlot >= hotbarSlotCount) newSlot = 0;
                SelectHotbarSlot(newSlot);
            }

            if (isChatOpen && Input.GetKeyDown(KeyCode.Return) && chatInputField != null)
            {
                OnChatSubmit(chatInputField.text);
            }
        }

        private void InitHotbar()
        {
            if (hotbarContainer == null || hotbarSlotPrefab == null) return;

            foreach (Transform child in hotbarContainer)
                Destroy(child.gameObject);

            hotbarIcons.Clear();
            hotbarHighlights.Clear();

            for (int i = 0; i < hotbarSlotCount; i++)
            {
                GameObject slot = Instantiate(hotbarSlotPrefab, hotbarContainer);
                Image icon = slot.transform.Find("Icon")?.GetComponent<Image>();
                Image highlight = slot.transform.Find("Highlight")?.GetComponent<Image>();

                hotbarIcons.Add(icon);
                if (highlight != null)
                {
                    highlight.gameObject.SetActive(false);
                    hotbarHighlights.Add(highlight);
                }
            }
            UpdateHotbarSelection();
        }

        private void SetupButtons()
        {
            if (resumeButton != null)
                resumeButton.onClick.AddListener(TogglePause);
            if (settingsButton != null)
                settingsButton.onClick.AddListener(OpenSettings);
            if (saveQuitButton != null)
                saveQuitButton.onClick.AddListener(SaveAndQuit);
        }

        public void SetCrosshairHover(bool hovering)
        {
            if (crosshairImage == null) return;
            crosshairImage.sprite = hovering && interactionCrosshair != null ? interactionCrosshair : defaultCrosshair;
            crosshairImage.color = hovering ? crosshairHighlightColor : crosshairDefaultColor;
        }

        public void SetCrosshairColor(Color color)
        {
            if (crosshairImage != null)
                crosshairImage.color = color;
        }

        public void SelectHotbarSlot(int index)
        {
            if (index < 0 || index >= hotbarSlotCount) return;
            selectedSlot = index;
            UpdateHotbarSelection();
            OnHotbarSlotChanged?.Invoke(selectedSlot);
        }

        private void UpdateHotbarSelection()
        {
            for (int i = 0; i < hotbarHighlights.Count; i++)
            {
                if (hotbarHighlights[i] != null)
                    hotbarHighlights[i].gameObject.SetActive(i == selectedSlot);
            }
        }

        public int GetSelectedSlot() { return selectedSlot; }

        public void SetHotbarSlotIcon(int slot, Sprite icon)
        {
            if (slot >= 0 && slot < hotbarIcons.Count && hotbarIcons[slot] != null)
                hotbarIcons[slot].sprite = icon;
        }

        public void SetHealth(float current, float max)
        {
            if (healthSlider != null)
                healthSlider.value = current / max;

            if (healthBarFill != null)
                healthBarFill.color = Color.Lerp(Color.red, Color.green
