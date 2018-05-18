/**********************************************
* Power Grid Inventory
* Copyright 2015-2016 James Clark
**********************************************/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PowerGridInventory.Extensions.ItemFilter
{
    /// <summary>
    /// Component used for handling item type filtering in a
    /// Grid Inventory equipment slot.
    /// </summary>
    [AddComponentMenu("Power Grid Inventory/Extensions/Item Filter/Item Type Filter")]
    [RequireComponent(typeof(PGISlot))]
    public class ItemTypeFilter : MonoBehaviour
    {
        public List<string> AllowedTypes;
        [HideInInspector]
        public PGISlot Slot;

        void Awake()
        {
            Slot = GetComponent<PGISlot>();
            if (Slot != null)
            {
                Slot.OnCanEquipItem.AddListener(CanEquip);
            }

        }

        public void CanEquip(PGISlotItem item, PGIModel inv, PGISlot slot)
        {
             //filter out what can and can't be equipped
            if (AllowedTypes != null && AllowedTypes.Count > 0)
            {
                var type = item.GetComponent<ItemType>();
                if (type != null && AllowedTypes.Contains(type.TypeName))
                {
                    return;
                }

                //let the inventory know that things are not well
                inv.CanPerformAction = false;
            }
        }

    }
}
