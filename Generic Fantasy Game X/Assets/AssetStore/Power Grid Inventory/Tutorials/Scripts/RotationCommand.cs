/**********************************************
* Power Grid Inventory
* Copyright 2015-2016 James Clark
**********************************************/
using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

namespace PowerGridInventory
{
    /// <summary>
    /// Provides a OnHover event hook that allows the user to rotate
    /// an item by right-clicking it.
    /// This is meant to be used as an example and will only work for mouse operated systems.
    /// </summary>
    public class RotationCommand : MonoBehaviour
    {
        /// <summary>
        /// Right-click event hook that rotates an item ina grid.
        /// </summary>
        /// <param name="eventData"></param>
        /// <param name="slot"></param>
        public void RotateItem(PointerEventData eventData, PGISlot slot)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                if (slot != null && slot.Item != null)
                {
                    if (slot.Item.RotatedDir == PGISlotItem.RotateDirection.None) slot.Item.Rotate(PGISlotItem.RotateDirection.CW);
                    else slot.Item.Rotate(PGISlotItem.RotateDirection.None);
                }
            }//end  if
        }

    }
}
