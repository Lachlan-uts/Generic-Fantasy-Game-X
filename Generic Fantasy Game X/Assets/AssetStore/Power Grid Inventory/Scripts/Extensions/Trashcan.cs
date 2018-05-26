/**********************************************
* Power Grid Inventory
* Copyright 2015-2016 James Clark
**********************************************/
using UnityEngine;


namespace PowerGridInventory.Extensions
{
    /// <summary>
    /// Trashcan event handler for a Grid Inventory slot.
    /// When an item is equipped to this slot it will be removed
    /// from its inventory as though it were dropped.
    /// </summary>
    [AddComponentMenu("Power Grid Inventory/Extensions/Trashcan", 12)]
    [RequireComponent(typeof(PGISlot))]
    public class Trashcan : MonoBehaviour
    {
        void Start()
        {
            //Turn off auto-equip for this slot,
            //otherwise we'll be sending items straight
            //to the trash when they enter the inventory!
            PGISlot slot = GetComponent<PGISlot>();
            if (slot != null)
            {
                //make sure we don't try to trash items
                //as soon as they are picked up.
                slot.SkipAutoEquip = true;
                slot.OnEquipItem.AddListener(TrashItem);
            }
        }

        public void TrashItem(PGISlotItem item, PGIModel inv, PGISlot slot)
        {
            //This helper method will handle all of the magic for us.
            //It makes sure the item is unequipped and removed from the inventory
            //and triggers all of the necessary events.
            inv.Drop(item);
        }

    }
}