using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using System.Collections;

public class InventoryDragAndDrop : MonoBehaviour
{
    private VisualElement root;
    private VisualElement currentSlot;
    private Image draggedItem;
    private bool isDraggingOutside;
    private EquipmentManager equipmentManager;
    private ItemDropping itemDropping;

    public GameObject playerCharacter;
    public Movement playerMovementScript;
    public float throwForce = 4f;
    [SerializeField] private GameObject droppedItemPrefab;

    private void Awake()
    {
        equipmentManager = Object.FindObjectOfType<EquipmentManager>();
        if (equipmentManager == null)
        {
            Debug.LogError("EquipmentManager not found.");
            return;
        }

        root = GetComponent<UIDocument>().rootVisualElement;
        root.RegisterCallback<MouseUpEvent>(OnGlobalMouseUp);
        root.RegisterCallback<MouseMoveEvent>(OnGlobalMouseMove);

        itemDropping = new ItemDropping(playerCharacter, droppedItemPrefab, playerMovementScript, throwForce);
    }

    public void RegisterSlotForDragging(VisualElement slot, string slotId)
    {
        slot.userData = slotId;
        slot.RegisterCallback<MouseMoveEvent>(OnSlotMouseMove);
    }

    private void OnSlotMouseMove(MouseMoveEvent evt)
    {
        if (Input.GetMouseButton(0) && currentSlot == null)
        {
            currentSlot = evt.currentTarget as VisualElement;
            Image slotIcon = currentSlot.Q<Image>();

            if (slotIcon != null)
            {
                slotIcon.style.opacity = 0.5f;
                draggedItem = new Image
                {
                    sprite = slotIcon.sprite,
                    style =
                    {
                        position = Position.Absolute,
                        width = slotIcon.layout.width,
                        height = slotIcon.layout.height,
                        opacity = 0f
                    }
                };
                root.Add(draggedItem);
                UpdateDraggedItemPosition(evt.mousePosition);
                StartCoroutine(FadeIn(draggedItem, 0.6f));
            }
        }
    }

    private void OnGlobalMouseUp(MouseUpEvent evt)
    {
        if (currentSlot != null && draggedItem != null)
        {
            if (isDraggingOutside)
            {
                Item itemToBeDropped = GetDraggedItem(currentSlot);
                if (itemToBeDropped != null)
                {
                    itemDropping.CreateDroppedItemInstanceGeneric(itemToBeDropped, evt.mousePosition, equipmentManager);
                    RemoveItemFromInventory(itemToBeDropped);
                }
            }
            CleanupDragState();
        }
    }


    private void CleanupDragState()
    {
        if (draggedItem != null)
        {
            root.Remove(draggedItem);
            draggedItem = null;
        }
        if (currentSlot != null)
        {
            Image slotIcon = currentSlot.Q<Image>();
            if (slotIcon != null)
            {
                slotIcon.style.opacity = 1.0f;
            }
            currentSlot = null;
        }
    }

    private void OnGlobalMouseMove(MouseMoveEvent evt)
    {
        if (draggedItem != null)
        {
            UpdateDraggedItemPosition(evt.mousePosition);

            VisualElement inventoryPanel = root.Q<VisualElement>("InvContainer");
            Rect panelRect = new Rect(inventoryPanel.layout.position, inventoryPanel.layout.size);
            Vector2 mousePos = evt.mousePosition;
            isDraggingOutside = !panelRect.Contains(mousePos);

            if (isDraggingOutside)
            {
                if (!draggedItem.Children().Any(child => child.name == "DroppingLabel"))
                {
                    var droppingLabel = new Label("Dropping")
                    {
                        name = "DroppingLabel",
                        style =
                        {
                            unityTextAlign = TextAnchor.MiddleCenter,
                            fontSize = 16,
                            color = Color.white,
                            position = Position.Absolute,
                            top = -30,
                            left = draggedItem.layout.width / 2 - 50,
                            width = 100
                        }
                    };
                    draggedItem.Add(droppingLabel);
                }
            }
            else
            {
                draggedItem.Q<Label>("DroppingLabel")?.RemoveFromHierarchy();
            }
        }
    }

    private void UpdateDraggedItemPosition(Vector2 mousePosition)
    {
        if (draggedItem != null)
        {
            draggedItem.style.left = mousePosition.x - draggedItem.layout.width / 2;
            draggedItem.style.top = mousePosition.y - draggedItem.layout.height / 2;
        }
    }

    private Item GetDraggedItem(VisualElement slot)
    {
        // Implement the logic to retrieve the Item instance based on the slot's user data or other identifiers
        // This is a placeholder, you'll need to adapt it to your inventory system
        return null;
    }

    private void RemoveItemFromInventory(Item item)
    {
        // Implement the logic to remove the item from the inventory data structure
        // This is a placeholder, you'll need to adapt it to your inventory system
    }

    IEnumerator FadeIn(Image img, float duration)
    {
        float startTime = Time.time;
        while (img.style.opacity.value < 1f)
        {
            float t = (Time.time - startTime) / duration;
            img.style.opacity = Mathf.SmoothStep(0f, 6f, t);
            yield return null;
        }
    }
}
