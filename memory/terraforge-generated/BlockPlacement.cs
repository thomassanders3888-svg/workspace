using UnityEngine;

public class BlockPlacement : MonoBehaviour {
    public static BlockPlacement Instance { get; private set; }
    public float reachDistance = 5f;
    public LayerMask placementLayers;
    public BlockType selectedBlock = BlockType.Stone;
    
    private Camera playerCamera;
    private Vector3Int highlightedBlock;
    private bool isHighlightActive;
    
    void Awake() { Instance = this; }
    void Start() { playerCamera = Camera.main; }
    
    void Update() {
        RaycastCheck();
        HandleInput();
    }
    
    void RaycastCheck() {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        
        if (Physics.Raycast(ray, out RaycastHit hit, reachDistance, placementLayers)) {
            Vector3Int blockPos = Vector3Int.FloorToInt(hit.point - hit.normal * 0.1f);
            
            if (blockPos != highlightedBlock || !isHighlightActive) {
                highlightedBlock = blockPos;
                isHighlightActive = true;
                OnBlockHighlightChange(blockPos);
            }
        } else {
            isHighlightActive = false;
        }
    }
    
    void HandleInput() {
        if (!isHighlightActive) return;
        
        // Left click to remove
        if (Input.GetMouseButtonDown(0)) {
            RemoveBlock(highlightedBlock);
        }
        // Right click to place
        else if (Input.GetMouseButtonDown(1)) {
            Vector3Int placePos = highlightedBlock + Vector3Int.RoundToInt(
                (Camera.main.transform.position + Camera.main.transform.forward * reachDistance - (Vector3)highlightedBlock).normalized
            );
            PlaceBlock(placePos, selectedBlock);
        }
    }
    
    void PlaceBlock(Vector3Int pos, BlockType type) {
        if (TerrainManager.Instance.PlaceBlock(pos.x, pos.y, pos.z, selectedBlock)) {
            AudioManager.Instance.Play("place");
            NetworkClient.Instance.SendBlockPlace(pos, selectedBlock);
        }
    }
    
    void RemoveBlock(Vector3Int pos) {
        if (TerrainManager.Instance.RemoveBlock(pos.x, pos.y, pos.z)) {
            AudioManager.Instance.Play("break");
            NetworkClient.Instance.SendBlockRemove(pos);
        }
    }
    
    void OnBlockHighlightChange(Vector3Int pos) {
        BlockSelector.Instance.UpdateSelection(pos);
    }
    
    public void SelectBlock(BlockType type) {
        selectedBlock = type;
    }
}
