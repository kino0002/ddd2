using UnityEngine;

public class DroppedStorageEquipment : ItemPickup
{
    private ItemContainer storedItems;

    public void SetItems(ItemContainer items)
    {
        storedItems = items;
    }

    public ItemContainer GetStoredItems()
    {
        return storedItems;
    }
}
