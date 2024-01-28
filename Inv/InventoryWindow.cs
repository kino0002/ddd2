using UnityEditor;
using UnityEngine.UIElements;
using UnityEngine;

public class InventoryWindow : EditorWindow
{
    [MenuItem("Window/InventoryWindow")]
    public static void ShowWindow()
    {
        GetWindow<InventoryWindow>();
    }
    private ItemContainer itemContainer;
    private const int ItemsPerRow = 5;

    public void OnEnable()
    {
        // Load your main UXML file here
        var mainVisualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/inve.uxml");
        mainVisualTree.CloneTree(rootVisualElement);

        // Load the InveSlot UXML file
        var inveSlotVisualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/InveSlot.uxml");

        // Create a new instance of the StorageFrame element
        var storageFrame = inveSlotVisualTree.CloneTree().Q<VisualElement>("StorageFrame");

        // Find the ScrollView element where you want to add the StorageFrame
        var scrollView = rootVisualElement.Q<ScrollView>("Inv");

        // Add the StorageFrame to the ScrollView
        scrollView.Add(storageFrame);

        // Find the SlotHolder element
        var slotHolder = storageFrame.Q<VisualElement>("SlotHolder");

        // Create a new SlotRow element
        var slotRow = new VisualElement();
        slotRow.name = "SlotRow";
        slotRow.style.flexDirection = FlexDirection.Row;
        slotRow.style.width = new Length(318, LengthUnit.Pixel);
        slotRow.style.height = new Length(62, LengthUnit.Pixel);

        // Replace the code inside the for loop in OnEnable with this
        for (int i = 0; i < itemContainer.Items.Count; i++)
        {
            var itemStack = itemContainer.Items[i];
            var itemSlot = new VisualElement();
            itemSlot.name = "ItemSlot";
            itemSlot.style.width = new Length(62, LengthUnit.Pixel);
            itemSlot.style.height = new Length(62, LengthUnit.Pixel);
            itemSlot.style.backgroundColor = Color.black;
            itemSlot.style.marginBottom = 2;

            // Add item-specific visuals (e.g., icon) to the itemSlot here
            // You can use itemStack.Item and itemStack.Quantity to customize the visuals

            if (i % ItemsPerRow == 0 && i != 0)
            {
                // Add the current SlotRow to the SlotHolder and create a new one
                slotHolder.Add(slotRow);
                slotRow = new VisualElement();
                slotRow.name = "SlotRow";
                slotRow.style.flexDirection = FlexDirection.Row;
                slotRow.style.width = new Length(318, LengthUnit.Pixel);
                slotRow.style.height = new Length(62, LengthUnit.Pixel);
            }

            slotRow.Add(itemSlot);
        }
        // Add the last SlotRow to the SlotHolder if it has any items

        if (slotRow.childCount > 0)
        {
            slotHolder.Add(slotRow);
        }
    }

}