using System.Collections.Generic;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public Item item;
    public float pickupDelay = 0.5f;
    private float timeSinceThrown;
    public int playerLayer = 8;

    public void SetTimeSinceThrown(float time)
    {
        timeSinceThrown = time;
    }

    private void Start()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && item != null)
        {
            spriteRenderer.sprite = item.Icon;
            UpdateColliderShape();
        }
        playerLayer = LayerMask.NameToLayer("Player");
    }

    private void UpdateColliderShape()
    {
        PolygonCollider2D polygonCollider = GetComponent<PolygonCollider2D>();
        if (polygonCollider != null)
        {
            Sprite sprite = GetComponent<SpriteRenderer>().sprite;
            if (sprite != null)
            {
                polygonCollider.pathCount = sprite.GetPhysicsShapeCount();
                List<Vector2> path = new List<Vector2>();

                for (int i = 0; i < polygonCollider.pathCount; i++)
                {
                    path.Clear();
                    sprite.GetPhysicsShape(i, path);
                    polygonCollider.SetPath(i, path);
                }
            }
        }
    }

    public List<ItemStack> StoredItems { get; set; }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && Time.time > timeSinceThrown + pickupDelay)
        {
            EquipmentManager equipmentManager = other.GetComponent<EquipmentManager>();
            if (equipmentManager == null) return;

            if (item is EquipmentDefinition equipment)
            {
                EquipmentManager.EquipmentSlot slot = equipmentManager.GetEquipmentSlot(equipment.slotType);

                // Check if the slot is for a bag and if there's already a bag equipped
                if (slot != null && equipment is StorageItem && slot.equippedItem != null)
                {
                    // Prevent picking up a bag with items if there's already a bag equipped
                    if (StoredItems != null && StoredItems.Count > 0)
                    {
                        Debug.Log("Cannot pick up a bag with items when another bag is already equipped.");
                        return; // Exit without picking up the bag
                    }
                }

                // If there's no item equipped in the slot, or if it's an empty bag, try to equip the new item
                if (slot != null && slot.equippedItem == null)
                {
                    bool equipped = equipmentManager.EquipItem(equipment);
                    if (equipped)
                    {
                        TransferStoredItems(equipmentManager, slot);
                        Destroy(gameObject); // Destroy the item pickup after successful equip
                        return;
                    }
                }
            }

            // If the item wasn't equipped (e.g., it's not an equipment item or the slot was occupied), try adding to the hotbar or storage
            HandleItemAddition(equipmentManager);
        }
    }

    private void TransferStoredItems(EquipmentManager equipmentManager, EquipmentManager.EquipmentSlot slot)
    {
        if (StoredItems != null && StoredItems.Count > 0)
        {
            foreach (var itemStack in StoredItems)
            {
                slot.storageContainer.AddItems(new List<ItemStack> { itemStack }); // Ensure AddItems method exists
            }
            StoredItems.Clear(); // Clear StoredItems after transferring
            FindObjectOfType<InventoryUI>().UpdateStorageDisplay(); // Refresh UI
        }
    }


    private void HandleItemAddition(EquipmentManager equipmentManager)
    {
        if (!equipmentManager.TryAddItemToHotbar(item))
        {
            bool addedToStorage = AddItemToStorage(equipmentManager);
            if (!addedToStorage)
            {
                Debug.Log("No space in hotbar, equipment slots, or storage.");
            }
            else
            {
                Destroy(gameObject);  // Destroy the item pickup after adding to storage
            }
        }
        else
        {
            Destroy(gameObject);  // Destroy the item pickup after adding to the hotbar
        }
    }

    private bool AddItemToStorage(EquipmentManager equipmentManager)
    {
        foreach (var slot in equipmentManager.GetEquipmentSlots())
        {
            if (slot.equippedItem is StorageItem && slot.storageContainer != null)
            {
                bool added = slot.storageContainer.AddItem(item, 1);
                if (added)
                {
                    FindObjectOfType<InventoryUI>().UpdateStorageDisplay();
                    return true;  // Item added to storage
                }
            }
        }
        return false;  // No space found in storage
    }


}
