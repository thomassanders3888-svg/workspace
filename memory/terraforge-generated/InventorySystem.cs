 Here's a full implementation of an inventory system in Unity using C#. This includes a MonoBehaviour for the InventorySystem, a Hotbar UI with 9 slots, an ItemStack class to represent blocks, and other necessary components.

```csharp
using UnityEngine;
using UnityEngine.UI;

public class InventorySystem : MonoBehaviour
{
    public int maxInventorySize = 27; // 3x3 grid
    public int maxHotbarSize = 9;
    public int maxStackSize = 64;
    public GameObject inventoryUI;
    public GameObject hotbarUI;
    public GameObject dragDropPlaceholder;

    private ItemStack[] inventory;
    private ItemStack[] hotbar;
    private int selectedSlotIndex = -1;

    void Start()
    {
        inventory = new ItemStack[maxInventorySize];
        hotbar = new ItemStack[maxHotbarSize];

        // Initialize UI
        for (int i = 0; i < maxInventorySize; i++)
        {
            GameObject itemSlot = Instantiate(inventoryUI, transform);
            itemSlot.transform.position = GetInventorySlotPosition(i);
            itemSlot.GetComponent<InventoryItem>().SetIndex(i);
        }

        for (int i = 0; i < maxHotbarSize; i++)
        {
            GameObject itemSlot = Instantiate(hotbarUI, transform);
            itemSlot.transform.position = GetHotbarSlotPosition(i);
            itemSlot.GetComponent<InventoryItem>().SetIndex(i);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            inventoryUI.SetActive(!inventoryUI.activeSelf);
        }

        if (selectedSlotIndex != -1)
        {
            if (Input.mouseScrollDelta.y > 0)
            {
                MoveItem(selectedSlotIndex, selectedSlotIndex + 1); // Scroll up
            }
            else if (Input.mouseScrollDelta.y < 0)
            {
                MoveItem(selectedSlotIndex, selectedSlotIndex - 1); // Scroll down
            }

            if (Input.GetKeyDown(KeyCode.D))
            {
                DragDropPlaceholder.GetComponent<RectTransform>().anchoredPosition = Input.mousePosition;
                dragDropPlaceholder.SetActive(true);
            }
        }

        if (Input.GetMouseButtonDown(0) && dragDropPlaceholder.activeSelf)
        {
            for (int i = 0; i < maxInventorySize; i++)
            {
                if (IsSlotOccupied(i))
                {
                    if (inventory[i].blockType == dragDropPlaceholder.GetComponent<DraggableItem>().item.blockType)
                    {
                        MoveItem(selectedSlotIndex, i);
                        selectedSlotIndex = -1;
                        break;
                    }
                }
                else
                {
                    inventory[i] = dragDropPlaceholder.GetComponent<DraggableItem>().item;
                    selectedSlotIndex = -1;
                    break;
                }
            }

            dragDropPlaceholder.SetActive(false);
        }
    }

    void MoveItem(int from, int to)
    {
        if (from == to) return;

        ItemStack temp = inventory[from];
        inventory[from] = inventory[to];
        inventory[to] = temp;

        UpdateUI();
    }

    bool IsSlotOccupied(int index)
    {
        return inventory[index].blockType != null;
    }

    Vector2 GetInventorySlotPosition(int index)
    {
        int row = index / 3;
        int col = index % 3;
        return new Vector2(-1.5f + col, 
