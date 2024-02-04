using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

public class InventoryUI : MonoBehaviour
{
    private EquipmentManager equipmentManager;
    private VisualElement root;
    private ScrollView invScrollView;
    private StyleSheet styleSheet;

    private InventoryDragAndDrop dragAndDropManager;

    private void Start()
    {
        equipmentManager = Object.FindObjectOfType<EquipmentManager>();
        if (equipmentManager == null)
        {
            Debug.LogError("EquipmentManager not found.");
            return;
        }

        root = GetComponent<UIDocument>().rootVisualElement;
        equipmentManager.OnEquipmentChanged += UpdateEquipmentSlot;

        invScrollView = root.Q<ScrollView>("Inv");
        Debug.Log("InventoryUI Start called.");
        styleSheet = Resources.Load<StyleSheet>("inve");

        dragAndDropManager = GetComponent<InventoryDragAndDrop>();
    }

    private void OnDestroy()
    {
        if (equipmentManager != null)
        {
            equipmentManager.OnEquipmentChanged -= UpdateEquipmentSlot;
        }
    }

    private void UpdateEquipmentSlot(EquipmentDefinition equipment, string slotType)
    {
        string slotID = slotType + "Icon";
        Image slotIcon = root.Q<Image>(slotID);
        VisualElement parentSlot = slotIcon?.parent;

        if (parentSlot != null)
        {
            if (equipment != null)
            {
                slotIcon.sprite = equipment.Icon;
                dragAndDropManager.RegisterSlotForDragging(parentSlot, slotType);  // Registering the parent slot with the slotType
                parentSlot.AddToClassList("filled");
            }
            else
            {
                slotIcon.sprite = null;
                parentSlot.RemoveFromClassList("filled");
            }
        }
        else
        {
            Debug.LogWarning($"Icon with ID '{slotID}' not found.");
        }
    }

    public void UpdateStorageDisplay()
    {
        Debug.Log("UpdateStorageDisplay called.");

        // Clear current display
        invScrollView.Clear();

        // Get storage equipment and items
        var storageEquipments = equipmentManager.GetStorageEquipments();

        foreach (var storageEquipment in storageEquipments)
        {
            var storageContainer = CreateStorageItemContainer();
            storageContainer.Add(CreateTinyEquipmentView(storageEquipment.equippedItem.Icon));

            var itemsGrid = CreateItemsGridContainer();

            if (storageEquipment.storageContainer != null)
            {
                var items = storageEquipment.storageContainer.GetItems();
                int currentItemCount = items.Count;

                for (int i = 0; i < ((StorageItem)storageEquipment.equippedItem).MaxStorageSpace; i++)
                {
                    if (i < currentItemCount)
                    {
                        itemsGrid.Add(CreateItemSlot(items[i].Item.Icon));
                    }
                    else
                    {
                        itemsGrid.Add(CreateItemSlot());
                    }
                }
            }
            storageContainer.Add(itemsGrid);

            invScrollView.Add(storageContainer);
        }
    }

    private VisualElement CreateStorageItemContainer()
    {
        var container = new VisualElement();
        container.name = "StorageContainer";
        container.AddToClassList("StorageContainer");

        if (styleSheet != null)
        {
            container.styleSheets.Add(styleSheet);
        }
        else
        {
            Debug.LogError("StyleSheet is still null.");
        }

        return container;
    }

    private VisualElement CreateTinyEquipmentView(Sprite icon)
    {
        var view = new VisualElement();
        view.AddToClassList("TinyEquipmentView");

        if (icon != null)
        {
            var image = new Image { sprite = icon };
            view.Add(image);
        }
        return view;
    }

    private VisualElement CreateItemsGridContainer()
    {
        var grid = new VisualElement();
        grid.AddToClassList("ItemsGridContainer");
        return grid;
    }

    private VisualElement CreateItemSlot(Sprite icon = null)
    {
        var slot = new VisualElement();

        if (icon != null)
        {
            var image = new Image { sprite = icon };
            slot.Add(image);
            slot.AddToClassList("ItemSlotWithItem");
        }
        else
        {
            slot.AddToClassList("ItemSlot");
        }

        return slot;
    }
}
