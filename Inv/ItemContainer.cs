using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemContainer
{
    public int MaxSize { get; private set; }
    public List<ItemStack> Items { get; private set; }

    public ItemContainer(int maxSize)
    {
        MaxSize = maxSize;
        Items = new List<ItemStack>();
    }

    public List<ItemStack> GetItems()
    {
        return Items;
    }

    public bool AddItem(Item item, int quantity)
    {
        ItemStack existingStack = Items.Find(stack => stack.Item.itemId == item.itemId && stack.Quantity < item.maxStack);

        if (existingStack != null)
        {
            int spaceInExistingStack = item.maxStack - existingStack.Quantity;
            int addToExistingStack = Mathf.Min(spaceInExistingStack, quantity);
            existingStack.Quantity += addToExistingStack;
            quantity -= addToExistingStack;
        }

        while (quantity > 0 && Items.Count < MaxSize)
        {
            int newStackSize = Mathf.Min(quantity, item.maxStack);
            Items.Add(new ItemStack(item, newStackSize));
            quantity -= newStackSize;
        }

        return quantity == 0;
    }

    public bool RemoveItem(Item item, int quantity)
    {
        for (int i = Items.Count - 1; i >= 0 && quantity > 0; i--)
        {
            ItemStack stack = Items[i];
            if (stack.Item.itemId == item.itemId)
            {
                int removeQuantity = Mathf.Min(quantity, stack.Quantity);
                stack.Quantity -= removeQuantity;
                quantity -= removeQuantity;

                if (stack.Quantity == 0)
                {
                    Items.RemoveAt(i);
                }
            }
        }

        return quantity == 0;
    }

    public bool CanAddItem(Item item, int quantity)
    {
        int remainingSpace = MaxSize - Items.Count;
        int fullStacks = Mathf.FloorToInt(quantity / (float)item.maxStack);
        int remainder = quantity % item.maxStack;

        ItemStack existingStack = Items.Find(stack => stack.Item.itemId == item.itemId && stack.Quantity < item.maxStack);
        int spaceInExistingStack = existingStack != null ? item.maxStack - existingStack.Quantity : 0;

        bool canAdd = remainingSpace >= fullStacks && (spaceInExistingStack >= remainder || remainingSpace >= Mathf.CeilToInt(remainder / (float)item.maxStack));

        Debug.Log($"CanAddItem: remainingSpace={remainingSpace}, fullStacks={fullStacks}, remainder={remainder}, spaceInExistingStack={spaceInExistingStack}, canAdd={canAdd}");

        return canAdd;
    }

    public bool HasAvailableSpace()
    {
        return Items.Count < MaxSize;
    }

    public bool IsEmpty()
    {
        return Items.Count == 0;
    }
    public void AddItems(List<ItemStack> itemsToAdd)
    {
        foreach (var itemStack in itemsToAdd)
        {
            AddItem(itemStack.Item, itemStack.Quantity);
        }
    }


}

[System.Serializable]
public class ItemStack
{
    public Item Item { get; private set; }
    public int Quantity { get; set; }

    public ItemStack(Item item, int quantity)
    {
        Item = item;
        Quantity = quantity;
    }
}

