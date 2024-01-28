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
                if (slot != null && slot.equippedItem == null)
                {
                    if (equipment is IEquippable equippableItem)
                    {
                        equippableItem.Equip(equipmentManager);

                        // If the item is a StorageItem and has stored items
                        if (StoredItems != null && StoredItems.Count > 0 && slot.storageContainer != null)
                        {
                            foreach (var itemStack in StoredItems)
                            {
                                // Ensure the item is added to the player's storage container
                                slot.storageContainer.AddItem(itemStack.Item, itemStack.Quantity);
                            }
                        }
                        Destroy(gameObject);
                    }
                    InventoryUI instance = FindObjectOfType<InventoryUI>();
                    if (instance != null)
                    {
                        instance.UpdateStorageDisplay();
                    }
                }
                else
                {
                    Debug.Log("Equipment slot is already in use.");
                }


            }
            else
            {
                bool canPickup = false;
                foreach (var checkSlot in equipmentManager.GetEquipmentSlots())
                {
                    if (checkSlot.equippedItem is StorageItem && checkSlot.storageContainer != null)
                    {
                        if (checkSlot.storageContainer.CanAddItem(item, 1))
                        {
                            canPickup = true;
                            break;
                        }
                    }
                }

                if (canPickup)
                {
                    foreach (var addSlot in equipmentManager.GetEquipmentSlots())
                    {
                        if (addSlot.equippedItem is StorageItem && addSlot.storageContainer != null)
                        {
                            if (addSlot.storageContainer.AddItem(item, 1))
                            {
                                Destroy(gameObject);

                                // Update the storage UI display
                                InventoryUI instance = FindObjectOfType<InventoryUI>();
                                if (instance != null)
                                {
                                    instance.UpdateStorageDisplay();
                                    Debug.Log("Attempted to update storage display after item pickup.");
                                }
                                else
                                {
                                    Debug.LogWarning("InventoryUI instance not found in the scene.");
                                }

                                break;
                            }
                        }
                    }
                }
                else
                {
                    Debug.Log("Not enough space to pick up the item.");
                }
            }
        }
    }
}
