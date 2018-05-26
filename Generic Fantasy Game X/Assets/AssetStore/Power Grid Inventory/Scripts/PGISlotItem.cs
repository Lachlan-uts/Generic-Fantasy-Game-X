/**********************************************
* Power Grid Inventory
* Copyright 2015-2017 James Clark
**********************************************/
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using PowerGridInventory.Utility;
using System.Xml.Serialization;
using Toolbox.Common;

namespace PowerGridInventory
{
    /// <summary>
    /// Provides functionality for allowing a GameObject
    /// to interact with the Power Grid Inventory system
    /// as an item that can be stored in a <see cref="PGIModel"/>.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Power Grid Inventory/Item", 14)]
    [Serializable]
    public class PGISlotItem : MonoBehaviour, IEventSystemHandler, IComparable
    {
        [Serializable]
        public class InventoryEvent : UnityEvent<PGISlotItem, PGIModel> { };
        [Serializable]
        public class InventorySlotEvent : UnityEvent<PGISlotItem, PGIModel, PGISlot> { };

        /// <summary>
        /// Defines the types of icons that can be displayed in a view to represent an item.
        /// </summary>
        public enum IconAssetType
        {
            Sprite,
            Mesh,
        }

        /// <summary>
        /// Defines an abosulute rotation orientation for the slot item.
        /// </summary>
        public enum RotateDirection
        {
            None,
            CW,
            CCW,
        }


        #region Members
        /// <summary>
        /// A toggle that determines what kind of asset to display in the view as this item's icon.
        /// </summary>
        [Header("Icon")]
        [HideInInspector]
        public IconAssetType IconType = IconAssetType.Sprite;

        /// <summary>
        /// The sprite that represents this item in an inventory view.
        /// </summary>
        [Tooltip("The sprite that represents this item in an inventory view.")]
        [HideInInspector]
        public Sprite Icon;

        /// <summary>
        /// 3D mesh that represents this item in an inventory view.
        /// </summary>
        [Tooltip("The 3D mesh that represents ths item in an inventory view.")]
        [HideInInspector]
        public Mesh Icon3D;

        /// <summary>
        /// The material used by the 3D mesh icon.
        /// </summary>
        [Tooltip("The material used by the 3D mesh icon.")]
        [HideInInspector]
        public Material IconMaterial;

        /// <summary>
        /// The angle that this 3D icon will have within an inventory slot.
        /// </summary>
        [XmlIgnore]
        public Vector3 IconOrientation
        {
            get { return _IconOrientation; }
            set
            {
                _IconOrientation = value;
                //forces attached views to update all icons
                if (Model != null) Model.MarkDirty(this);
            }
        }
        [SerializeField]
        [HideInInspector]
        public Vector3 _IconOrientation = Vector3.zero;

        //public fields and properties
        /// <summary>
        /// Returns the index of the equipment slot this item is equipped to or -1 if it is not equipped.
        /// </summary>
        public int Equipped { get { return _Equipped; } private set { _Equipped = value; } }

        /// <summary>
        /// Read only. The x position of the inventory grid this item is stored at or -1 if it is not in an inventory or equipped to an equipment slot.
        /// </summary>
        public int xInvPos { get { return _xInvPos; } private set { _xInvPos = value; } }

        /// <summary>
        /// Read only. The y position of the inventory grid this item is stored at or -1 if it is not in an inventory or equipped to an equipment slot.
        /// </summary>
        public int yInvPos { get { return _yInvPos; } private set { _yInvPos = value; } }

        /// <summary>
        /// The number of grid cells this item takes up on the horizontal axis. It must be at least 1.
        /// </summary>
        [Header("Inventory Stats")]
        [Tooltip("The number of grid cells this item takes up on the horizontal axis. It must be at least 1.")]
        [FormerlySerializedAs("InvWidth")]
        public int CellWidth = 1;

        /// <summary>
        /// The number of grid cells this item takes up on the vertical axis. It must be at least 1.
        /// </summary>
        [Tooltip("The number of grid cells this item takes up on the vertical axis. It must be at least 1.")]
        [FormerlySerializedAs("InvHeight")]
        public int CellHeight = 1;

        /// <summary>
        /// The color to display in a slot's highlight when this item is stored.
        /// </summary>
        [Tooltip("The color to display in a slot's highlight when this item is stored.")]
        public Color Highlight = Color.clear;

        /// <summary>
        /// A special identifier used to determine if two items are stackable.
        /// </summary>
        [Tooltip("A special identifier used to determine if two items are stackable.")]
        [FormerlySerializedAs("StackID")]
        public int ItemTypeId = 0;

        /// <summary>
        /// The number of items sharing the same <see cref="PGISlotItem.ItemTypeId"/> that can be stacked on top of each other in a single location.
        /// </summary>
        [Header("Stacking")]
        [Tooltip("The maximum number of items sharing the same StackID that can be stacked on top of each other in a single location.")]
        public int MaxStack = 1;

        /// <summary>
        /// The current stack size for this item. This number is used by the model's stacking system
        /// to treat this item as a collection even though, physically, there is only one instance of the item.
        /// Changing this value will set the 'dirty' flag for any slots used by it in an inventory.
        [Tooltip("The current stack size for this item. There is only one real instance of the item and all stacked instances are destroyed and cause this number to increment.")]
        public int StackCount = 1;

        /// <summary>
        /// Storage for commonly accessed MonoBehaviours attached to this item.
        /// </summary>
        [Space(10)]
        [Tooltip("Storage for commonly accessed MonoBehaviours attached to this item.")]
        public MonoBehaviour[] References;

        /// <summary>
        /// The <see cref="PGIModel"/> that this item is currently stored in, or null if it is not in an inventory.
        /// </summary>
        public PGIModel Model
        {
            get { return _Container; }
            protected set { _Container = value; }
        }

        /// <summary>
        /// Returns true if this item is currently equipped to an equipment slot.
        /// </summary>
        public bool IsEquipped
        {
            get { return (Model != null && Equipped >= 0); }
        }

        /// <summary>
        /// Returns true if this item is currently in a grid location.
        /// </summary>
        public bool IsStored
        {
            get { return (Model != null && xInvPos >= 0 && yInvPos >= 0); }
        }

        /// <summary>
        /// Returns true if this item is oriented in a rotated fashion.
        /// </summary>
        public bool Rotated
        {
            get { return (_RotatedDir != RotateDirection.None); }
        }

        public RotateDirection RotatedDir
        {
            get { return _RotatedDir; }
        }

        //backing data
        [SerializeField]
        [HideInInspector]
        private int _Equipped = -1;
        [SerializeField]
        [HideInInspector]
        private int _xInvPos = -1;
        [SerializeField]
        [HideInInspector]
        private int _yInvPos = -1;
        [SerializeField]
        [HideInInspector]
        private PGIModel _Container;
        //[SerializeField][ShowInPGIInspector]private int _StackCount = 1;
        //private data
        [SerializeField]
        [HideInInspector]
        private Transform PreviousParent;
        [SerializeField]
        [HideInInspector]
        private RotateDirection _RotatedDir = RotateDirection.None;
        
        #endregion


        #region PGI Event Fields
        /// <summary>
        /// Invoked when the pointer is clicked on an equipment slot or grid location with an item in it.
        /// <seealso cref="PGISlot.OnClick"/> 
        /// <seealso cref="PGIView.OnClickSlot"/> 
        /// </summary>
        [SerializeField]
        [PGIFoldedEvent]
        public PGISlot.SlotTrigger OnClick = new PGISlot.SlotTrigger();

        /// <summary>
        /// Invoked when this item is about to be stored in a grid location in a <see cref="PGIModel"/>.
        /// It is only called the first time the item attempts to enter a new inventory.
        /// You can disallow this action by setting the the provided model's
        /// <see cref="PGIModel.CanPerformAction"/> to <c>false</c>.
        /// <seealso cref="PGIModel.OnCanStoreItem"/> 
        /// </summary>
        [SerializeField]
        [PGIFoldedEvent]
        public InventoryEvent OnCanStore = new InventoryEvent();

        /// <summary>
        /// Invoked when this item is about to be stored in an equipment slot in a <see cref="PGIModel"/>.
        /// You can disallow this action by setting the the provided model's
        /// <see cref="PGIModel.CanPerformAction"/> to <c>false</c>.
        /// <seealso cref="PGIModel.OnCanEquipItem"/>
        /// <seealso cref="PGISlot.OnCanEquipItem"/>
        /// </summary>
        [SerializeField]
        [PGIFoldedEvent]
        public InventorySlotEvent OnCanEquip = new InventorySlotEvent();

        /// <summary>
        /// Invoked when this item is about to be removed from a <see cref="PGIModel"/>'s inventory.
        /// It is only called when the item will be completely and officially removed from the inventory.
        /// You can disallow this action by setting the the provided model's
        /// <see cref="PGIModel.CanPerformAction"/> to <c>false</c>.
        /// <seealso cref="PGIModel.OnCanRemoveItem"/> 
        /// </summary>
        [SerializeField]
        [PGIFoldedEvent]
        public InventoryEvent OnCanRemove = new InventoryEvent();

        /// <summary>
        /// Invoked when this item is about to be removed from an equipment slot in a <see cref="PGIModel"/>.
        /// You can disallow this action by setting the the provided model's
        /// <see cref="PGIModel.CanPerformAction"/> to <c>false</c>.
        /// <seealso cref="PGIModel.OnCanUnequipItem"/>
        /// <seealso cref="PGISlot.OnCanUnequipItem"/>
        /// </summary>
        [SerializeField]
        [PGIFoldedEvent]
        public InventorySlotEvent OnCanUnequip = new InventorySlotEvent();

        /// <summary>
        /// Invoked after this item has been stored in a new <see cref="PGIModel"/>'s inventory. It will not be called when
        /// the item is moved around within that inventory.
        /// <seealso cref="PGIModel.OnStoreItem"/>
        /// </summary>
        [SerializeField]
        [PGIFoldedEvent]
        public InventoryEvent OnStoreInInventory = new InventoryEvent();

        /// <summary>
        /// Invoked after this item has been removed from a <see cref="PGIModel"/>'s inventory.
        /// <seealso cref="PGIModel.OnRemoveItem"/>
        /// </summary>
        [SerializeField]
        [PGIFoldedEvent]
        public InventoryEvent OnRemoveFromInventory = new InventoryEvent();

        /// <summary>
        /// Invoked after this item has been equipped to an equipment slot.
        /// <seealso cref="PGIModel.OnEquipItem"/>
        /// <seealso cref="PGISlot.OnEquipItem"/>
        /// </summary>
        [SerializeField]
        [PGIFoldedEvent]
        public InventorySlotEvent OnEquip = new InventorySlotEvent();

        /// <summary>
        /// Invoked after this item has been removed from an equipment slot.
        /// This will occur even when dragging an item from the slot but before dropping it
        /// in a new location.
        /// <seealso cref="PGIModel.OnUnequipItem"/>
        /// <seealso cref="PGISlot.OnUnequipItem"/>
        /// </summary>
        [SerializeField]
        [PGIFoldedEvent]
        public InventorySlotEvent OnUnequip = new InventorySlotEvent();

        /// <summary>
        /// Invoked after an item has failed to be stored in an inventory. Usually this is
        /// the result of a 'Can...' method disallowing the item to be stored or simply
        /// the fact that there is not enough room for the item.
        /// <seealso cref="PGIModel.OnStoreItemFailed"/>
        /// </summary>
        [SerializeField]
        [PGIFoldedEvent]
        public InventoryEvent OnStoreInInventoryFailed = new InventoryEvent();

        /// <summary>
        /// Invoked after this item has failed to be equipped to an equipment slot. Usually this
        /// is the result of a 'Can...' method disallowing the item to be stored or simply
        /// the fact that there was another item already located in the same slot. This
        /// method may be called frequiently when using <see cref="PGIModel.FindFreeSpace"/>
        /// or <see cref="PGIModel.Pickup"/>.
        /// <seealso cref="PGIModel.OnEquipItemFailed"/>
        /// <seealso cref="PGISlot.OnEquipItemFailed"/>
        /// </summary>
        [SerializeField]
        [PGIFoldedEvent]
        public InventorySlotEvent OnEquipFailed = new InventorySlotEvent();

        /// <summary>
        /// Invoked by a view when this item has been grabbed and a drag operation has started.
        /// </summary>
        [SerializeField]
        [PGIFoldedEvent]
        public InventorySlotEvent OnDragBegin = new InventorySlotEvent();

        /// <summary>
        /// Invoked by a view when this item has been dropped after a drag operation ended.
        /// This will be invoked both when a valid slot is the drop target and when it is invalid
        /// and the item is returned to its original position.
        /// </summary>
        [SerializeField]
        [PGIFoldedEvent]
        public InventorySlotEvent OnDragEnd = new InventorySlotEvent();
        
        
        
        [SerializeField]
        [FoldFlag("Events")]
        public bool FoldedEvents = false; //used by the inspector
        #endregion


        #region Unity Events
        void OnDestroy()
        {
            //TODO: release all events here!
#if UNITY_EDITOR
            if(Application.isPlaying)
            {

            }
#else
            
#endif
        }
#endregion


#region Methods
        /// <summary>
        /// Marks this object as being in one of three states that represent rotation
        /// in grid-axis space. Effectively all it does is swap the item's
        /// height and width, store a 'rotated' state flag, and rotate the UI image.
        /// </summary>
        /// <remarks>
        /// If the item is currently stored within a model then it may not be possible to
        /// rotate the item and have it remain within that model. The object will attempt
        /// to automatical adjust its position so that it can fit while still maintaining
        /// some vailence with its original position, however if it is not possible the operation
        /// will fail and the item will remain unrotated. The item will always succeed at being
        /// rotated if it is not currently within a model.
        /// </remarks>
        /// <returns><c>true</c> if the item was rotated, <c>false</c> otherwise.</returns>
        /// <param name="dir">The absolute direction to have the item's top rotated towards. </param>
        public bool Rotate(RotateDirection dir)
        {
            if(_RotatedDir == dir) return false;
            if (CellWidth == CellHeight) return false;
            
            PGIModel.Pos newPos = null;
            PGIModel model = this.Model;
            if(model != null && this.IsStored)
            {
                //If in a grid we'll have to check for viable 'vailence' positions for the rotated object.
                //i.e. positions that are still overlapping where the item was before it was rotated.
                //If we can't make the object fit roughly in the same place as before when rotated
                //then we'll have to fail.
                newPos = Model.FindVailencePosition(this, dir);
                if (newPos == null) return false;
            }

            if (newPos != null && model != null) model.Remove(this, false);

            //regarldess of where it is stored, the item needs some basic
            //stats changes and its image rotated.
            int temp = CellHeight;
            CellHeight = CellWidth;
            CellWidth = temp;
            _RotatedDir = dir;
            
            //This should only be called when the 'this.Model' is non-null and
            //the item is stored in the grid but we can safely assume for now 
            //that if 'newPo's is non-null then this is the case.
            if(newPos != null && model != null) model.Store(this, newPos.X, newPos.Y, false);

            return true;
        }

        /// <summary>
        /// Used to mark this item as being stored within a grid
        /// at the given location. This usually gets called internally
        /// by a <see cref="PGIModel"/> when storing an item.
        /// </summary>
        /// <param name="storage">The <see cref="PGIModel"/> that this item is being stored in.</param>
        /// <param name="x">The x coordinate in the gird or -1.</param>
        /// <param name="y">The y coordinate in the grid or -1.</param>
        public void ProcessStorage(PGIModel storage, int x, int y)
        {
            Equipped = -1;
            xInvPos = x;
            yInvPos = y;
            Model = storage;
            SetNewParentTransform(Model);
        }

        /// <summary>
        /// Marks this item as no longer
        /// being stored within a grid or equipment slot.
        /// It also returns this item to the ownership of
        /// the transform heirarchy it was before it was stored.
        /// This usually gets called internally
        /// by a <see cref="PGIModel"/> when dropping an item.
        /// </summary>
        public void ProcessRemoval()
        {
            Equipped = -1;
            xInvPos = -1;
            yInvPos = -1;
            RestoreOldTransform();
            Model = null;
        }

        /// <summary>
        /// Marks this item as being equipped to an equipment slot
        /// with the given index. This usually gets called internally
        /// by a <see cref="PGIModel"/> when equipping an item.
        /// <seealso cref="PGIModel.Equipment"/>
        /// </summary>
        /// <param name="storage">The <see cref="PGIModel"/> that this item is being stored in.</param>
        /// <param name="equipIndex">The index of the equipment slot in the <see cref="PGIModel"/>.</param>
        public void ProcessEquip(PGIModel storage, int equipIndex)
        {
            Equipped = equipIndex;
            xInvPos = -1;
            yInvPos = -1;
            Model = storage;
            SetNewParentTransform(Model);
        }

        /// <summary>
        /// Helper method for moving an item into a model's hierarchy and caching important data.
        /// </summary>
        /// <param name="newModel"></param>
        protected void SetNewParentTransform(PGIModel newModel)
        {
            if (newModel.ModifyTransforms)
            {
                if (transform.parent != newModel.transform)
                {
                    PreviousParent = transform.parent;
                }
                transform.SetParent(newModel.transform, false);
            }
        }

        /// <summary>
        /// Helper method for resroring previously cached parent info of this item.
        /// </summary>
        protected void RestoreOldTransform()
        {
            if (Model.ModifyTransforms)
            {
                transform.SetParent(PreviousParent, false);
            }
        }

        /// <summary>
        /// Used to trigger any 'CanEquip' events in the item, slot, or inventory just before equipping the item.
        /// If any attached handler wishes to invalidate this check they must
        /// set the <see cref="PGIModel.CanPerformAction"/> flag to <c>false</c>
        /// in the provided model parameter.
        /// <seealso cref="PGISlotItem.OnCanEquip"/>
        /// <seealso cref="PGIModel.OnCanEquipItem"/>
        /// <seealso cref="PGISlot.OnCanEquipItem"/>
        /// </summary>
        /// <returns><c>true</c> if this instance can equip the specified storage destSlot; otherwise, <c>false</c>.</returns>
        /// <param name="storage">The <see cref="PGIModel"/> whose equipment slot this item is being assigned to.</param>
        /// <param name="destSlot">The <see cref="PGISlot"/> that this item is being equipped to.</param>
        public void TriggerCanEquipEvents(PGIModel storage, PGISlot destSlot)
        {
            OnCanEquip.Invoke(this, storage, destSlot);
            destSlot.OnCanEquipItem.Invoke(this, storage, destSlot);
            if (storage != null) storage.OnCanEquipItem.Invoke(this, storage, destSlot);
        }

        /// <summary>
        /// Used to trigger any 'CanUnequip' events in the item, slot, or inventory just before unequipping the item.
        /// If any attached handler wishes to invalidate this check they must
        /// set the <see cref="PGIModel.CanPerformAction"/> flag to <c>false</c>
        /// in the provided model parameter.
        /// <seealso cref="PGISlotItem.OnCanUnequip"/>
        /// <seealso cref="PGIModel.OnCanUnequipItem"/>
        /// <seealso cref="PGISlot.OnCanUnequipItem"/>
        /// </summary>
        /// <returns><c>true</c> if this instance can equip the specified storage destSlot; otherwise, <c>false</c>.</returns>
        /// <param name="storage">The <see cref="PGIModel"/> whose equipment slot this item is being removed from.</param>
        /// <param name="destSlot">The <see cref="PGISlot"/> that this item is being unequipped from.</param>
        public void TriggerCanUnequipEvents(PGIModel storage, PGISlot destSlot)
        {
            OnCanUnequip.Invoke(this, storage, destSlot);
            destSlot.OnCanUnequipItem.Invoke(this, storage, destSlot);
            if (storage != null) storage.OnCanUnequipItem.Invoke(this, storage, destSlot);
        }

        /// <summary>
        /// Used to trigger any 'CanStore' events in the item, slot, or inventory just before storing the item.
        /// If any attached handler wishes to invalidate this check they must
        /// set the <see cref="PGIModel.CanPerformAction"/> flag to <c>false</c>
        /// in the provided model parameter.
        /// <seealso cref="PGISlotItem.OnCanStore"/>
        /// <seealso cref="PGIModel.OnCanStoreItem"/>
        /// </summary>
        /// <param name="storage">The <see cref="PGIModel"/> whose inventory this item is being stored in.</param>
        public void TriggerCanStoreEvents(PGIModel storage)
        {
            OnCanStore.Invoke(this, storage);
            if (storage != null) storage.OnCanStoreItem.Invoke(this, storage);
        }

        /// <summary>
        /// Used to trigger any 'CanRemove' events in the item, or inventory just before dropping this item.
        /// If any attached handler wishes to invalidate this check they must
        /// set the <see cref="PGIModel.CanPerformAction"/> flag to <c>false</c>
        /// in the provided model parameter.
        /// <seealso cref="PGISlotItem.OnCanRemove"/>
        /// <seealso cref="PGIModel.OnCanRemoveItem"/>
        /// </summary>
        /// <param name="storage">The <see cref="PGIModel"/> whose inventory this item is being removed from.</param>
        public void TriggerCanRemoveEvents(PGIModel storage)
        {
            OnCanRemove.Invoke(this, storage);
            if (storage != null) storage.OnCanStoreItem.Invoke(this, storage);
        }

        /// <summary>
        /// Used to trigger any 'OnStoreInInventory' events for this item, or inventory.
        /// <seealso cref="PGISlotItem.OnStoreInInventory"/>
        /// <seealso cref="PGIModel.OnStoreItem"/>
        /// </summary>
        /// <param name="storage">The <see cref="PGIModel"/> whose inventory this item is being stored in.</param>
        public void TriggerStoreEvents(PGIModel storage)
        {
            OnStoreInInventory.Invoke(this, storage);
            if (storage != null) storage.OnStoreItem.Invoke(this, storage);
        }

        /// <summary>
        /// Used to trigger any OnRemoved events for this item,
        /// and the associated inventory.
        /// <seealso cref="PGISlotItem.OnRemoveFromInventory"/>
        /// <seealso cref="PGIMode.OnRemoveItem"/>
        /// </summary>
        /// <param name="storage">The <see cref="PGIModel"/> whose inventory this item is being removed from.</param>
        public void TriggerRemoveEvents(PGIModel storage)
        {
            OnRemoveFromInventory.Invoke(this, storage);
            if (storage != null) storage.OnRemoveItem.Invoke(this, storage);
        }

        /// <summary>
        /// Used to trigger any OnEquip events for this item,
        /// the slot it is going in, and the associated inventory.
        /// <seealso cref="PGISlotItem.OnEquip"/>
        /// <seealso cref="PGIModel.OnEquipItem"/>
        /// <seealso cref="PGISlot.OnEquipItem"/>
        /// </summary>
        /// <param name="storage">The <see cref="PGIModel"/> to which the equipment slot belongs.</param>
        /// <param name="destSlot">The <see cref="PGISot"/> that this item is being equipped to.</param>
        public void TriggerEquipEvents(PGIModel storage, PGISlot destSlot)
        {
            OnEquip.Invoke(this, storage, destSlot);
            if (destSlot != null) destSlot.OnEquipItem.Invoke(this, storage, destSlot);
            if (storage != null) storage.OnEquipItem.Invoke(this, storage, destSlot);
        }

        /// <summary>
        /// Used to trigger any OnUnequip events for this item,
        /// the slot it is going in, and the associated inventory.
        /// <seealso cref="PGISlotItem.OnUnequip"/>
        /// <seealso cref="PGIModel.OnUnequipItem"/>
        /// <seealso cref="PGISlot.OnUnequipItem"/>
        /// </summary>
        /// <param name="storage">The <see cref="PGIModel"/> to which the equipment slot belongs.</param>
        /// <param name="destSlot">The <see cref="PGISot"/> that this item is being unequipped from.</param>
        public void TriggerUnequipEvents(PGIModel storage, PGISlot sourceSlot)
        {
            OnUnequip.Invoke(this, storage, sourceSlot);
            if (sourceSlot != null) sourceSlot.OnUnequipItem.Invoke(this, storage, sourceSlot);
            if (storage != null) storage.OnUnequipItem.Invoke(this, storage, sourceSlot);
        }

        /// <summary>
        /// Used to trigger any OnStoreInInventoryFailed events for this item,
        /// and the associated inventory.
        /// <seealso cref="PGISlotItem.OnStoreInInventoryFailed"/>
        /// <seealso cref="PGIModel.OnStoreItemFailed"/>
        /// </summary>
        /// <param name="storage">The <see cref="PGIModel"/> whose inventory this item had attempted to enter.</param>
        public void TriggerStoreFailedEvents(PGIModel storage)
        {
            OnStoreInInventoryFailed.Invoke(this, storage);
            if (storage != null) storage.OnStoreItemFailed.Invoke(this, storage);
        }

        /// <summary>
        /// Used to trigger any OnStoreInInventoryFailed events for this item,
        /// and the associated inventory.
        /// <seealso cref="PGISlotItem.OnEquipFailed"/>
        /// <seealso cref="PGIModel.OnEquipItemFailed"/>
        /// <seealso cref="PGISlot.OnEquipItemFailed"/>
        /// </summary>
        /// <remarks>
        /// This method can potentially be called many times during drag 'n drop operations
        /// or when using the <see cref="PGIModel.Pickup"/> method.
        /// </remarks>
        /// <param name="storage">The <see cref="PGIModel"/> to which the equipment slot belongs.</param>
        /// <param name="destSlot">The <see cref="PGISot"/> that this item failed to equip to.</param>
        public void TriggerEquipFailedEvents(PGIModel storage, PGISlot destSlot)
        {
            OnEquipFailed.Invoke(this, storage, destSlot);
            if (destSlot != null) destSlot.OnEquipItemFailed.Invoke(this, storage, destSlot);
            if (storage != null) storage.OnEquipItemFailed.Invoke(this, storage, destSlot);
        }

        /// <summary>
        /// Used to globally define what metrics to use for sorting (width or height).
        /// </summary>
        public static bool SortByWidth = false;

        /// <summary>
        /// Compares this slot item's size to another slot item's size.
        /// Items with bigger sizes come before items with smaller sizes.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            PGISlotItem other = obj as PGISlotItem;
            if (other == null) return 1;
            if (SortByWidth) return other.CellWidth - this.CellWidth;
            else return other.CellHeight - this.CellHeight;
        }
#endregion


    }

}
