using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BuildMenu : MonoBehaviour {
    public static BuildMenu Instance { get; private set; }
    
    public GameObject menuPanel;
    public Transform categoriesContainer;
    public Transform blocksContainer;
    public GameObject categoryButtonPrefab;
    public GameObject blockButtonPrefab;
    public BlockSelector blockPreview;
    
    private List<BlockCategory> categories;
    private BlockCategory selectedCategory;
    
    public class BlockCategory {
        public string name;
        public Sprite icon;
        public List<BlockData> blocks = new();
    }
    
    public class BlockData {
        public BlockType type;
        public string displayName;
        public Sprite icon;
    }
    
    void Awake() { Instance = this; }
    void Start() { InitializeCategories(); }
    
    void InitializeCategories() {
        categories = new List<BlockCategory> {
            new BlockCategory { name = "Building", blocks = new() },
            new BlockCategory { name = "Natural", blocks = new() },
            new BlockCategory { name = "Ores", blocks = new() },
            new BlockCategory { name = "Decor", blocks = new() }
        };
        
        // Populate blocks
        PopulateBlocks();
        
        // Create category buttons
        foreach (var cat in categories) {
            var btn = Instantiate(categoryButtonPrefab, categoriesContainer);
            btn.GetComponentInChildren<TextMeshProUGUI>().text = cat.name;
            btn.GetComponent<Button>().onClick.AddListener(() => ShowCategory(cat));
        }
        
        ShowCategory(categories[0]);
    }
    
    void PopulateBlocks() {
        categories[0].blocks.Add(new BlockData { type = BlockType.Stone, displayName = "Stone" });
        categories[0].blocks.Add(new BlockData { type = BlockType.Wood, displayName = "Wood" });
        categories[0].blocks.Add(new BlockData { type = BlockType.Sand, displayName = "Sand" });
        categories[1].blocks.Add(new BlockData { type = BlockType.Grass, displayName = "Grass" });
        categories[1].blocks.Add(new BlockData { type = BlockType.Dirt, displayName = "Dirt" });
        categories[1].blocks.Add(new BlockData { type = BlockType.Leaves, displayName = "Leaves" });
        categories[2].blocks.Add(new BlockData { type = BlockType.OreIron, displayName = "Iron Ore" });
        categories[2].blocks.Add(new BlockData { type = BlockType.OreGold, displayName = "Gold Ore" });
        categories[2].blocks.Add(new BlockData { type = BlockType.OreDiamond, displayName = "Diamond Ore" });
    }
    
    void ShowCategory(BlockCategory category) {
        selectedCategory = category;
        
        // Clear current buttons
        foreach (Transform child in blocksContainer) Destroy(child.gameObject);
        
        // Create block buttons
        foreach (var block in category.blocks) {
            var btn = Instantiate(blockButtonPrefab, blocksContainer);
            btn.GetComponentInChildren<TextMeshProUGUI>().text = block.displayName;
            btn.GetComponent<Button>().onClick.AddListener(() => SelectBlock(block.type));
        }
    }
    
    void SelectBlock(BlockType type) {
        BlockPlacement.Instance.SelectBlock(type);
        blockPreview.UpdateVisuals(type);
    }
    
    public void ToggleBuildMenu() {
        menuPanel.SetActive(!menuPanel.activeSelf);
        Cursor.lockState = menuPanel.activeSelf ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = menuPanel.activeSelf;
    }
}
