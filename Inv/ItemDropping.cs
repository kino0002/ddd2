using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class ItemDropping
{
    private GameObject playerCharacter;
    private GameObject droppedItemPrefab;
    private Movement playerMovementScript;
    private float throwForce;

    public ItemDropping(GameObject playerCharacter, GameObject droppedItemPrefab, Movement playerMovementScript, float throwForce)
    {
        this.playerCharacter = playerCharacter;
        this.droppedItemPrefab = droppedItemPrefab;
        this.playerMovementScript = playerMovementScript;
        this.throwForce = throwForce;

        if (this.droppedItemPrefab == null)
        {
            Debug.LogError("Dropped item prefab is null!");
        }
    }

    public GameObject CreateDroppedItemInstance(EquipmentDefinition itemToBeDropped, Vector2 mousePosition, EquipmentManager equipmentManager)
    {
        if (droppedItemPrefab == null)
        {
            Debug.LogError("Dropped item prefab is null!");
            return null;
        }

        GameObject droppedItemInstance = Object.Instantiate(droppedItemPrefab, playerCharacter.transform.position, Quaternion.identity);
        ItemPickup itemPickup = droppedItemInstance.GetComponent<ItemPickup>();
        if (itemPickup != null)
        {
            itemPickup.item = itemToBeDropped;

            // Find the equipment slot for the item being dropped
            EquipmentManager.EquipmentSlot slot = equipmentManager.GetEquipmentSlot(itemToBeDropped.slotType);
            if (slot != null && slot.storageContainer != null && slot.storageContainer.Items.Count > 0)
            {
                // Pass the stored items to the dropped item instance
                itemPickup.StoredItems = new List<ItemStack>(slot.storageContainer.GetItems());
                // Clear the items from the original container to avoid duplication
                slot.storageContainer.Items.Clear();
            }

            // Calculate throw direction and apply force
            Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
            Vector2 throwDirection = (mouseWorldPosition - (Vector2)playerCharacter.transform.position).normalized;
            float halfItemWidth = itemPickup.GetComponent<SpriteRenderer>().bounds.extents.x;
            droppedItemInstance.transform.position += (Vector3)throwDirection * halfItemWidth;
            Vector3 force = new Vector3(playerMovementScript.side * throwForce, throwForce, 0);
            Throw(itemPickup, force, playerMovementScript.GetComponent<Rigidbody2D>().velocity);
        }
        else
        {
            Debug.LogError("ItemPickup component not found on the dropped item prefab.");
        }

        return droppedItemInstance;
    }
    public void CreateDroppedItemInstanceGeneric(Item item, Vector2 dropPosition, EquipmentManager equipmentManager)
    {
        // Ensure the droppedItemPrefab is set
        if (droppedItemPrefab == null)
        {
            Debug.LogError("Dropped item prefab is null!");
            return;
        }

        GameObject droppedItemInstance = GameObject.Instantiate(droppedItemPrefab, dropPosition, Quaternion.identity);

        // Use ItemPickup component as a generic handler for dropped items if DroppedItemComponent does not exist
        ItemPickup itemPickup = droppedItemInstance.GetComponent<ItemPickup>();
        if (itemPickup != null)
        {
            // Assuming you have a way to assign the 'Item' instance to the 'ItemPickup' component
            itemPickup.item = item; // This assignment might need adjustment based on your 'ItemPickup' implementation

            // Additional logic here if needed, such as setting item properties, initializing visuals, etc.
        }
        else
        {
            Debug.LogError("ItemPickup component not found on the dropped item prefab.");
        }
    }



    private void Throw(ItemPickup itemPickup, Vector3 force, Vector3 playerVelocity)
    {
        itemPickup.StartCoroutine(ApplyThrowForce(itemPickup, force, playerVelocity));
    }

    private IEnumerator ApplyThrowForce(ItemPickup itemPickup, Vector3 force, Vector3 playerVelocity)
    {
        Rigidbody2D rb = itemPickup.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.AddForce(force + playerVelocity, ForceMode2D.Impulse);
            itemPickup.SetTimeSinceThrown(Time.time);
            yield return new WaitForSeconds(itemPickup.pickupDelay); // Wait for pickup delay
        }
    }


    private IEnumerator DisablePickupTemporarily(ItemPickup itemPickup)
    {
        Collider2D collider = itemPickup.GetComponent<Collider2D>();
        if (collider != null)
        {
            Physics2D.IgnoreLayerCollision(itemPickup.gameObject.layer, itemPickup.playerLayer, true);
            yield return new WaitForSeconds(itemPickup.pickupDelay);
            Physics2D.IgnoreLayerCollision(itemPickup.gameObject.layer, itemPickup.playerLayer, false);
        }
    }
}
