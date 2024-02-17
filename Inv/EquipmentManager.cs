using System;
using UnityEngine;
using System.Collections.Generic;

public class EquipmentManager : MonoBehaviour
{
    [System.Serializable]
    public class EquipmentSlot
    {
        public string slotType;
        public EquipmentDefinition equippedItem;
        public ItemContainer storageContainer;
    }

    [SerializeField]
    private List<EquipmentSlot> equipmentSlots;

    public event Action<EquipmentDefinition, string> OnEquipmentChanged;

    private void Awake()
    {
        // Initialize equipmentSlots with default values
        equipmentSlots = new List<EquipmentSlot>
        {
            new EquipmentSlot { slotType = "PrimarySlot" },
            new EquipmentSlot { slotType = "PouchSlot" },
            new EquipmentSlot { slotType = "HeadSlot" },
            new EquipmentSlot { slotType = "ChestSlot" },
            new EquipmentSlot { slotType = "LegsSlot" },
            new EquipmentSlot { slotType = "BagSlot" },
            new EquipmentSlot { slotType = "RingSlot" }
        };
        hotbarSlots = new List<HotbarSlot>
    {
        new HotbarSlot { slotId = 0 },
        new HotbarSlot { slotId = 1 },
        new HotbarSlot { slotId = 2 },
        new HotbarSlot { slotId = 3 }
    };
    }

    [System.Serializable]
    public class HotbarSlot
    {
        public int slotId;
        public Item item; // Assuming 'Item' is your base item class
    }

    [SerializeField]
    private List<HotbarSlot> hotbarSlots = new List<HotbarSlot>();

    public List<HotbarSlot> GetHotbarSlots()
    {
        return hotbarSlots;
    }

    public void AddItemToHotbar(Item item, int slotId)
    {
        var slot = hotbarSlots.Find(s => s.slotId == slotId);
        if (slot != null)
        {
            slot.item = item;
        }
        else
        {
            hotbarSlots.Add(new HotbarSlot { slotId = slotId, item = item });
        }

        // Notify InventoryUI to update
        OnHotbarChanged?.Invoke();
    }

    public event Action OnHotbarChanged;


    public bool TryAddItemToHotbar(Item item)
    {
        // Find the first empty hotbar slot
        var emptySlot = hotbarSlots.Find(slot => slot.item == null);
        if (emptySlot != null)
        {
            emptySlot.item = item;
            OnHotbarChanged?.Invoke(); // Invoke hotbar update event
            return true; // Successfully added to hotbar
        }
        return false; // No available slot in hotbar
    }


    public bool EquipItem(EquipmentDefinition newItem)
    {
        if (newItem == null)
        {
            return false;
        }

        EquipmentSlot slot = GetEquipmentSlot(newItem.slotType);

        if (slot != null)
        {
            if (slot.equippedItem != null)
            {
                UnequipItem(slot.slotType);
            }

            slot.equippedItem = newItem;
            OnEquipmentChanged?.Invoke(newItem, slot.slotType);

            if (newItem is StorageItem storageItem)
            {
                slot.storageContainer = new ItemContainer(storageItem.MaxStorageSpace);
            }

            // Update the storage display whenever an item is equipped
            FindObjectOfType<InventoryUI>().UpdateStorageDisplay();

            return true;
        }

        return false; // Return false if slot is null.
    }


    public void UnequipItem(string slotType)
    {
        EquipmentSlot slot = GetEquipmentSlot(slotType);
        if (slot != null && slot.equippedItem != null)
        {
            EquipmentDefinition oldItem = slot.equippedItem;
            slot.equippedItem = null;
            OnEquipmentChanged?.Invoke(null, slotType);

            if (oldItem is StorageItem)
            {
                slot.storageContainer = null;
                FindObjectOfType<InventoryUI>().UpdateStorageDisplay();
            }
        }
    }

    public EquipmentSlot GetEquipmentSlot(string slotType)
    {
        EquipmentSlot slot = equipmentSlots.Find(slot => slot.slotType == slotType);

        if (slot != null)
        {
            MessageDisplayManager.Instance.DisplayMessage("Equipment slot found for slotType: " + slotType);
        }
        else
        {
            Debug.LogWarning("Equipment slot not found for slotType: " + slotType);
        }

        return slot;
    }


    public List<EquipmentSlot> GetEquipmentSlots()
    {
        return equipmentSlots;
    }

    public void UnequipItemInstance(EquipmentDefinition equipmentInstance)
    {
        EquipmentSlot slot = equipmentSlots.Find(slot => slot.equippedItem == equipmentInstance);
        if (slot != null)
        {
            slot.equippedItem = null;
            OnEquipmentChanged?.Invoke(slot.equippedItem, slot.slotType);

            if (equipmentInstance is StorageItem)
            {
                slot.storageContainer = null;
            }
        }
        else
        {
            Debug.Log("Slot not found");
        }
        InventoryUI inventoryUI = FindObjectOfType<InventoryUI>();
        if (inventoryUI != null)
        {
            inventoryUI.UpdateStorageDisplay();
        }
    }

    public List<EquipmentSlot> GetStorageEquipments()
    {
        // This will filter out all equipment slots which have a StorageItem equipped
        return equipmentSlots.FindAll(slot => slot.equippedItem is StorageItem);
    }



}

