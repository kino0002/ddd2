using System.Collections;
using UnityEngine;

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

        if(this.droppedItemPrefab == null)
        {
            Debug.LogError("Dropped item prefab is null!");
        }
    }

    public GameObject CreateDroppedItemInstance(EquipmentDefinition itemToBeDropped, Vector2 mousePosition)
    {
        if(this.droppedItemPrefab == null)
        {
            Debug.LogError("Dropped item prefab is null!");
            return null;
        }

        GameObject droppedItemInstance = Object.Instantiate(droppedItemPrefab, playerCharacter.transform.position, Quaternion.identity);
        ItemPickup itemPickup = droppedItemInstance.GetComponent<ItemPickup>();

        if (itemPickup != null)
        {
            itemPickup.item = itemToBeDropped;

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
            itemPickup.StartCoroutine(DisablePickupTemporarily(itemPickup));
        }
        yield return null;
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
