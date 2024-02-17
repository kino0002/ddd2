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

                // If there's no item equipped in the slot, or if it's an empty bag, try to equip the new item
                if (slot != null && (slot.equippedItem == null || (slot.equippedItem is StorageItem && slot.storageContainer.IsEmpty())))
                {
                    bool equipped = equipmentManager.EquipItem(equipment);

                    if (equipped)
                    {
                        // If the new item has stored items, transfer them to the newly equipped item's storage
                        if (StoredItems != null && StoredItems.Count > 0)
                        {
                            slot.storageContainer.AddItems(StoredItems);
                            StoredItems.Clear();
                        }

                        Destroy(gameObject); // Destroy the item pickup after successful equip
                        FindObjectOfType<InventoryUI>().UpdateStorageDisplay(); // Refresh UI
                        return;
                    }
                }
            }

            // If the item wasn't equipped (e.g., it's not an equipment item or the slot was occupied), try adding to the hotbar or storage
            HandleItemAddition(equipmentManager);
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
