using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Data/Item")]
public class Item : ScriptableObject
{
    public string itemId;
    public string itemName;
    public string Description;
    public int Price;
    public Sprite Icon;
    public Dimensions SlotDimension;
    public int stackSize;
    public int maxStack;
    public int Durability;
    public bool isHorizontal = true; 
}

[Serializable]
public struct Dimensions
{
    public int Height;
    public int Width;
}

public interface IEquippable
{
    void Equip(EquipmentManager equipmentManager);
}

public enum EquipmentSlot
{
    PrimarySlot,
    HeadSlot,
    ChestSlot,
    LegsSlot,
    PouchSlot,
    BagSlot,
    RingSlot
}

public enum WeaponType
{
    Sword,
    Bow,
    Axe,
    Wand
}

[CreateAssetMenu(fileName = "New Weapon", menuName = "Data/Weapon")]
public class WeaponDefinition : Item, IEquippable {
    public WeaponType Type;
    public int Damage;

    public void Equip(EquipmentManager equipmentManager) {
        // Equip logic specific to weapons
    }
}

[CreateAssetMenu(fileName = "New Food", menuName = "Data/Food")]
public class FoodDefinition : Item
{
    public int HealthRestore;
    public int EnergyRestore;
}

[CreateAssetMenu(fileName = "New Health", menuName = "Data/Health")]
public class HealthDefinition : Item
{
    public int HealthRestore;
}

[CreateAssetMenu(fileName = "New Equipment", menuName = "Data/Equipment")]
public class EquipmentDefinition : Item, IEquippable
{
    public EquipmentSlot Slot;
    public int Armor;
    public string slotType;

    public void Equip(EquipmentManager equipmentManager)
    {
        equipmentManager.EquipItem(this);
    }
}

[CreateAssetMenu(fileName = "New Armor", menuName = "Data/Armor")]
public class ArmorDefinition : EquipmentDefinition
{
    // Add any additional properties specific to armor here
}

[CreateAssetMenu(fileName = "New Storage Item", menuName = "Data/StorageItem")]
public class StorageItem : EquipmentDefinition
{
    [SerializeField] private int maxStorageSpace = 10;
    public int MaxStorageSpace => maxStorageSpace;
}


