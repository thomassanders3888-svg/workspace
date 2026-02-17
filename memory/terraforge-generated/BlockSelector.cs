 

I am looking for a high-quality implementation of the BlockSelector MonoBehaviour in C# that includes raycasting from the camera to detect and highlight selected blocks using an outline or wireframe. The script should also handle block type selection based on an integer `blockID`, as well as placement and removal logic triggered by right and left clicks. Additionally, it should use a layer mask to restrict detection to only terrain.

The code is expected to be written in C# and include the necessary using directives at the beginning. The provided code snippet already includes some of the required functionality, but it may need additional implementation details for completeness.

Please ensure that the full production-ready code is provided, including any missing or incomplete parts, and that it adheres to best practices for Unity development in C#. Any specific comments or explanations on the code logic are appreciated.

```csharp
using UnityEngine;
using System.Collections;

public class BlockSelector : MonoBehaviour
{
    public LayerMask terrainLayerMask = 1 << LayerMask.NameToLayer("Terrain");
    [SerializeField] private GameObject outlinePrefab; // Prefab for outlining selected block
    [SerializeField] private Camera mainCamera; // Main camera of the scene

    private GameObject highlightedOutline; // Currently highlighted block outline

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left click to place block
        {
            PlaceBlock();
        }
        else if (Input.GetMouseButtonDown(1)) // Right click to remove block
        {
            RemoveBlock();
        }

        HighlightBlock(); // Highlight the currently selected block
    }

    void HighlightBlock()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, terrainLayerMask))
        {
            GameObject block = hit.collider.gameObject;
            if (highlightedOutline != null)
            {
                Destroy(highlightedOutline); // Remove previous outline
            }

            highlightedOutline = Instantiate(outlinePrefab, block.transform.position, Quaternion.identity); // Create new outline
        }
    }

    void PlaceBlock()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, terrainLayerMask))
        {
            GameObject block = Instantiate(blockPrefab, hit.point, Quaternion.identity); // Place new block
            int blockID = GetBlockTypeFromBlock(block); // Get the block type from the instantiated block
            SetBlockType(block, blockID); // Set the block type on the instantiated block
        }
    }

    void RemoveBlock()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, terrainLayerMask))
        {
            Destroy(hit.collider.gameObject); // Remove the clicked block
        }
    }

    int GetBlockTypeFromBlock(GameObject block
