/**********************************************
* Power Grid Inventory
* Copyright 2015-2016 James Clark
**********************************************/
using UnityEngine;
using System.Collections;

namespace PowerGridInventory.Demo
{
    /// <summary>
    /// This is a basic helper component for testing the inventory system.
    /// Simply attach it to a item that you wish to test with, provide
    /// a reference to the inventory to pickup into by default and now when
    /// you can click on this item it will be inserted into that inventory.
    /// </summary>
    [RequireComponent(typeof(PGISlotItem))]
    //[RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Renderer))]
    [AddComponentMenu("Power Grid Inventory/Examples/Simple Pickup", 42)]
    public class SimplePickup : MonoBehaviour
    {
        [Tooltip("The inventory that this item will enter when clicked.")]
        public PGIModel PickupToInventory;
        public bool ShowDebugMessages = false;
        public bool ShowDebugCanMessages = false;
        public bool ResizeBoundsOnStart = true;
        private PGISlotItem Item;

        void Start()
        {
            if (PickupToInventory != null) PickupToInventory.OnModelChanged.AddListener(OnModelChanged);
            Item = GetComponent<PGISlotItem>();

            //make sure the collider is as big as the sprite so we can click on it
            if (ResizeBoundsOnStart)
            {
                BoxCollider box3d = GetComponent<BoxCollider>();
                BoxCollider2D box2d = GetComponent<BoxCollider2D>();
                if (box3d != null)
                {
                    var bounds = GetComponent<Renderer>().bounds.size;
                    bounds.Scale(new Vector3(1.0f / transform.localScale.x, 1.0f / transform.localScale.y, 1.0f / transform.localScale.z));
                    box3d.size = bounds;
                }
                else if(box2d != null)
                {
                    var bounds = GetComponent<Renderer>().bounds.size;
                    bounds.Scale(new Vector3(1.0f / transform.localScale.x, 1.0f / transform.localScale.y, 1.0f / transform.localScale.z));
                    box2d.size = bounds;
                }
            }
            
           
            if (ShowDebugMessages)
            {
                Debug.Log("<b>Heads up!</b> The game object <i>'" + gameObject.name +
                           "'</i> has <i>ShowDebugMessages</i> set to <i>true</i> in the <i>SimplePickup</i> component. " +
                           "This may clutter your console a bit when using the Power Grid Inventory system.");
            }

            PickupToInventory.OnModelChanged.AddListener(OnChangeModel);
        }

        /// <summary>
        /// This is triggered when the model changes its reference, usually due to deserializing
        /// a new instance when loading from file.
        /// </summary>
        /// <param name="newModel"></param>
        void OnChangeModel(PGIModel newModel)
        {
            PickupToInventory = newModel;
        }

        /// <summary>
        /// Helper method that is used to disable/enable certain components
        /// when this object is being moved to and from inventory.
        /// </summary>
        /// <param name="flag">If set to <c>true</c> flag.</param>
        private void SetPhysicalManifestation(bool flag)
        {
            gameObject.SetActive(flag);
            //GetComponent<SpriteRenderer>().enabled = flag;
            //GetComponent<BoxCollider2D>().enabled = flag;
        }

        /// <summary>
        /// Usually triggered when the model has loaded a new copy of itself and
        /// needs all related objects to update their references to the new instance.
        /// </summary>
        /// <param name="newModel"></param>
        void OnModelChanged(PGIModel newModel)
        {
            PickupToInventory = newModel;
        }

        /// <summary>
        /// This is the event that allows us to store the item
        /// in the inventory when we click on it in the game world.
        /// </summary>
        void OnMouseDown()
        {
            PickupToInventory.Pickup(Item);
        }

        /// <summary>
        /// Triggered just before this item is enters the inventory.
        /// If you wish to stop the action set the passed PGIModel's
        /// 'CanPerformAction' flag to false.
        /// </summary>
        /// <param name="item">Item.</param>
        /// <param name="inv">Inv.</param>
        public void CanStore(PGISlotItem item, PGIModel inv)
        {
            inv.CanPerformAction = true;
            //we must set this flag or the item will not be able to be moved.
            if (ShowDebugCanMessages) Debug.Log("<color=yellow>Can store</color> " + this.gameObject.name + " in " + inv.gameObject.name + "?");
        }

        /// <summary>
        /// Triggered just before this item is leaves the inventory.
        /// If you wish to stop the action set the passed PGIModel's
        /// 'CanPerformAction' flag to false.
        /// </summary>
        /// <param name="item">Item.</param>
        /// <param name="inv">Inv.</param>
        public void CanRemove(PGISlotItem item, PGIModel inv)
        {
            //we must set this flag or the item will not be able to be moved.
            if (ShowDebugCanMessages) Debug.Log("<color=yellow>Can remove</color> " + this.gameObject.name + " from " + inv.gameObject.name + "?");
        }

        /// <summary>
        /// Triggered just before this item is unequipped.
        /// If you wish to stop the action set the passed PGIModel's
        /// 'CanPerformAction' flag to false.
        /// </summary>
        /// <param name="item">Item.</param>
        /// <param name="inv">Inv.</param>
        public void CanEquip(PGISlotItem item, PGIModel inv, PGISlot slot)
        {
            inv.CanPerformAction = true;
            if (ShowDebugCanMessages) Debug.Log("<color=yellow>Can equip</color> " + this.gameObject.name + " to " + slot.gameObject.name + " in " + inv.gameObject.name + "?");
        }

        /// <summary>
        /// Triggered just before this item is equipped.
        /// </summary>
        /// <param name="item">Item.</param>
        /// <param name="inv">Inv.</param>
        public void CanUnequip(PGISlotItem item, PGIModel inv, PGISlot slot)
        {
            if (ShowDebugCanMessages) Debug.Log("<color=yellow>Can unequip</color> " + this.gameObject.name + " from " + slot.gameObject.name + " in " + inv.gameObject.name + "?");
        }

        /// <summary>
        /// Triggered after this item is being stored in the given inventory.
        /// </summary>
        /// <param name="item">Item.</param>
        /// <param name="inv">Inv.</param>
        public void StoreInInventory(PGISlotItem item, PGIModel inv)
        {
            if (ShowDebugMessages) Debug.Log("<b>Storing</b> " + this.gameObject.name + " in " + inv.gameObject.name);
            SetPhysicalManifestation(false);
        }

        /// <summary>
        /// Triggered when this item fails to equip.
        /// </summary>
        /// <param name="item">Item.</param>
        /// <param name="inv">Inv.</param>
        public void StoreInInventoryFailed(PGISlotItem item, PGIModel inv)
        {
            if (ShowDebugMessages) Debug.Log("<color=red>Failed to store</color> " + this.gameObject.name + " in " + inv.gameObject.name);
        }

        /// <summary>
        /// Triggered after this item is removed from the given inventory.
        /// </summary>
        /// <param name="item">Item.</param>
        /// <param name="inv">Inv.</param>
        public void DropFromInventory(PGISlotItem item, PGIModel inv)
        {
            if (ShowDebugMessages) Debug.Log("<color=grey>Dropping</color> " + this.gameObject.name + " from " + inv.gameObject.name);
            SetPhysicalManifestation(true);
        }

        /// <summary>
        /// Triggered when this item is equipped.
        /// </summary>
        /// <param name="item">Item.</param>
        /// <param name="inv">Inv.</param>
        public void Equip(PGISlotItem item, PGIModel inv, PGISlot from)
        {
            if (ShowDebugMessages) Debug.Log("<color=blue>Equipped</color> " + this.gameObject.name + " to " + from.gameObject.name + " in " + inv.gameObject.name);
        }

        /// <summary>
        /// Triggered when this item fails to equip.
        /// </summary>
        /// <param name="item">Item.</param>
        /// <param name="inv">Inv.</param>
        public void EquipFailed(PGISlotItem item, PGIModel inv, PGISlot failedDest)
        {
            if (ShowDebugMessages) Debug.Log("<color=red>Failed to equip</color> " + this.gameObject.name + " to " + failedDest.gameObject.name + " in " + inv.gameObject.name);
        }

        /// <summary>
        /// Triggered when this item is unequipped.
        /// </summary>
        /// <param name="item">Item.</param>
        /// <param name="inv">Inv.</param>
        public void Unequip(PGISlotItem item, PGIModel inv, PGISlot dest)
        {
            if (ShowDebugMessages) Debug.Log("<color=cyan>Unequipped</color> " + this.gameObject.name + " from " + dest.gameObject.name + " in " + inv.gameObject.name);
        }

    }
}
