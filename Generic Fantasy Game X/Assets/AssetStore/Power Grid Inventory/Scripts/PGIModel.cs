/**********************************************
* Power Grid Inventory
* Copyright 2015-2017 James Clark
**********************************************/
#define PGI_LITE
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.Serialization;
using PowerGridInventory.Utility;
using System.Xml;
using Toolbox.Common;
using System.Linq;
using Toolbox;
using UnityEngine.Assertions;
#if !PGI_LITE
using Pantagruel.Serializer;
#endif

namespace PowerGridInventory
{
    /// <summary>
    /// Performs all backend item-position management
    /// for a grid inventory. Meant to work in tandem with
    /// a <see cref="PGIView"/> to render the model's data
    /// and provide user interactions. Items are expected to
    /// be represented as GameObjects with the <see cref="PGISlotItem"/>
    /// component attached to their root.
    /// </summary>
    [DisallowMultipleComponent]
    //[ExecuteInEditMode]
    [AddComponentMenu("Power Grid Inventory/Model", 10)]
    [Serializable]
    public class PGIModel : MonoBehaviour
    {
#region Local Classes
        [Serializable]
        public class ModelEvent : UnityEvent<PGIModel> { };

        /// <summary>
        /// Helper class for storing grid regions.
        /// </summary>
        public struct Area
        {
            public int x, y, width, height;

            public Area(int x, int y, int width, int height)
            {
                this.x = x;
                this.y = y;
                this.width = width;
                this.height = height;
            }
        }

        /// <summary>
        /// Helper class for storing grid locations.
        /// </summary>
        public class Pos
        {
            public int X;
            public int Y;

            public Pos(int x, int y)
            {
                X = x;
                Y = y;
            }

            public void Change(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

        /// <summary>
        /// Used in model queries for paring item ids with the required counts.
        /// </summary>
        public struct Query
        {
            public int ItemTypeId;
            public int Count;
            public GatherMode Mode;
            public GatherSource Source;

            public Query(int itemTypeId, int count, GatherMode mode = GatherMode.IncompleteStacksFirst, GatherSource source = GatherSource.GridAndEquipment)
            {
                ItemTypeId = itemTypeId;
                Count = count;
                Mode = mode;
                Source = source;
            }

            public Query(int itemTypeId, GatherMode mode = GatherMode.IncompleteStacksFirst, GatherSource source = GatherSource.GridAndEquipment)
            {
                ItemTypeId = itemTypeId;
                Count = -1;
                Mode = mode;
                Source = source;
            }
        }

        public enum GatherMode
        {
            IncompleteStacksFirst,
            CompleteStacksFirst,
        }

        public enum GatherSource
        {
            GridAndEquipment,
            GridOnly,
            EquipmentOnly,
        }

        enum ExistsIn
        {
            NoModel,
            AnotherModel,
            ThisModelComplete,
            ThisModelIncomplete,
            ThisModelInvalid,
        }

        public enum ArrangeCorner
        {
            TopLeft,
            BottomLeft,
            TopRight,
            BottomRight,
        }

        public enum ArrangeDirection
        {
            HorizontalFirst,
            VerticalFirst,
        }

        public delegate void DirtyEvent();

        public event DirtyEvent OnUpdateDirty;


        /// <summary>
        /// Helper PoD for storing cached results of a CanSwap check.
        /// </summary>
        protected class SwapCache
        {
            public PGISlot SwapSlot;
            public PGISlotItem DraggedItem;
            public bool SwapResult;

            /// <summary>
            /// Tests to see if the CanSwap settings were already checked, If not, the new ones are stored.
            /// If <c>false</c> is returned then <see cref="SwapCache.SwapResult"/> will prvoide the result of the CanSwap attempt.
            /// </summary>
            /// <param name="slot"></param>
            /// <param name="item"></param>
            /// <param name="newResult"></param>
            /// <returns><c>true</c> if the new swap settings can be attempted. In such a case the slot and item will be stored The SwapResult member must be manually set after a result is determined. <c>false</c> if these settings were already tried and cached.</returns>
            public bool IsNewSwapAttempt(PGISlot slot, PGISlotItem item)
            {
                if (slot == SwapSlot && item == DraggedItem) return false;

                SwapSlot = slot;
                DraggedItem = item;
                return true;
            }
        }

#endregion


#region Members
        /// <summary>
        /// If set, this model will automatically scan for items that have entered or left its transform
        /// hierarchy and add or remove them from the model as needed.
        /// </summary>
        [HideInInspector]
        public bool AutoDetectItems
        {
            get { return _AutoDetectItems; }
            set
            {
                _AutoDetectItems = value;
                if (Application.isPlaying && value && AutoDetector == null)
                {
                    AutoDetector = StartCoroutine(AutoDetectFunc());
                }
                if(Application.isPlaying && !value && AutoDetector != null)
                {
                    StopCoroutine(AutoDetector);
                }
            }
        }
        [Tooltip("If set, this model will automatically scan for items that have entered or left its transform hierarchy and add or remove them from the model as needed.")]
        [HideInInspector]
        [SerializeField]
        private bool _AutoDetectItems = false;

        /// <summary>
        /// The number of seconds between each attempt at automatically
        /// detecting any new items found in this model's hierarchy. If set
        /// to a negative value, no detection is used.
        /// </summary>
        [Tooltip("The number of seconds between each attempt at automatcially detecting any new items found in, or lost from, this model's hierarchy.")]
        [HideInInspector]
        [SerializeField]
        public float AutoDetectRate = 1.0f;
        private Coroutine AutoDetector;

        /// <summary>
        /// Cached list used by the auto item detector.
        /// </summary>
        private List<Transform> CachedChildren = new List<Transform>(10);


        /// <summary>
        /// Determines if this PGI model can change an item's parent transform
        /// in the heirarchy when moving items to and from inventories.
        /// </summary>
        [Tooltip("Determines if this PGI model can change an item's parent transform in the hierarchy when moving items to and from inventories.\n\nIMPORTANT: This should be left on in most cases since the save/load functionality requires an inventory's items to be children of it in order for them to be saved.")]
        [FormerlySerializedAs("MessWithItemTransforms")]
        public bool ModifyTransforms = true;

        /// <summary>
        /// The location to dump items if they are automatically detected in
        /// this model but cannot be stored and must be dropped from the inventory.
        /// </summary>
        [Tooltip("The location to dump items if they are automatically detected in this model but cannot be stored and must be dropped from the inventory.")]
        [HideInInspector]
        public Transform DefaultDumpLocation = null;

        /// <summary>
        /// Determines if this model can place items in any valid equipment slots
        /// when inserting an item into an inventory using <see cref="PGIModel.Pickup"/>. 
        /// It is also used when calling <see cref="PGIModel.FindFirstFreeSpace"/>. 
        /// </summary>
        /// <seealso cref="PGIModel.Equipment"/>
        /// <seealso cref="PGISlot"/>
        [Tooltip("Determines if this model can place items in any valid equipment slots when inserting an item into an inventory.")]
        public bool AutoEquip = true;

        /// <summary>
        /// Determines if this model should attempt to store items in valid equipment
        /// slots before using the grid when inserting an item into the inventory using <see cref="PGIModel.Pickup"/>.
        /// </summary>
        /// <seealso cref="PGIModel.Equipment"/>
        /// <seealso cref="PGISlot"/>
        [Tooltip("Determines if this model should attempt to store items in valid equipment slots before using the grid when inserting an item into the inventory.")]
        public bool AutoEquipFirst = true;

        /// <summary>
        /// Determines if this model should attempt to stack like items as they enter the inventory using <see cref="PGIModel.Pickup"/>.
        /// </summary>
        [Tooltip("Determines if this model should attempt to stack like items as they enter the inventory.")]
        public bool AutoStack = true;

        /// <summary>
        /// If set, this model allows socketable items to be stored in grid locations containing socketed items and thus activate socketing functionality between the two.
        /// </summary>
        [Tooltip("If set, this model allows socketable items to be stored in grid locations containing socketed items and thus activate socketing functionality between the two.")]
        public bool AllowSocketing = true;

        /// <summary>
        /// The number of cell-columns this model will provide for the grid.
        /// It may be zero, in which case there will be no grid.
        /// </summary>
        public int GridCellsX
        {
            get { return _GridCellsX; }
            set
            {
                if (value != _GridCellsX) RefreshGridSize(value, _GridCellsY);
            }
        }
        [Tooltip("The number of cell-columns this model will provide for the grid. It may be zero, in which case there will be no grid.")]
        [SerializeField]
        [Range(0, 1000)]
        private int _GridCellsX = 10;

        /// <summary>
        /// The number of cell-rows this model will provide for the grid.
        /// It may be zero, in which case there will be no grid.
        /// </summary>
        public int GridCellsY
        {
            get { return _GridCellsY; }
            set
            {
                if (value != _GridCellsY) RefreshGridSize(_GridCellsX, value);

            }
        }
        [Tooltip("The number of cell-rows this model will provide for the grid. It may be zero, in which case there will be no grid.")]
        [SerializeField]
        [Range(0, 1000)]
        private int _GridCellsY = 4;

        /// <summary>
        /// Stores the list of non-grid PGISlots that will be used as equipment slots.
        /// <seealso cref="PGISlot"/>
        /// </summary>
        [Space(10)]
        [Tooltip("Stores the list of non-grid PGISlots that will be used as equipment slots.")]
        public PGISlot[] Equipment;

        /// <summary>
        /// Returns a list of all items equipped within this model or null if there are no items equipped.
        /// </summary>
        public List<PGISlotItem> EquipmentItems
        {
            get
            {
                if (Equipment == null || Equipment.Length <= 0) return null;
                List<PGISlotItem> items = new List<PGISlotItem>(20);
                foreach (var slot in Equipment)
                {
                    if (slot.Item != null) items.Add(slot.Item);
                }
                return items;
            }
        }

        /// <summary>
        /// Returns a list of all items within this model's grid.
        /// </summary>
        public List<PGISlotItem> GridItems
        {
            get
            {
                return GetRangeContents(0, 0, GridCellsX, GridCellsY);
            }
        }

        /// <summary>
        /// Returns a list of all <see cref="PGISlotItem"/>s in this model, including both the grid and equipment slots.
        /// </summary>
        public List<PGISlotItem> AllItems
        {
            get
            {
                List<PGISlotItem> items = new List<PGISlotItem>(GetRangeContents(0, 0, GridCellsX, GridCellsY));


                if (Equipment != null && Equipment.Length > 0)
                {
                    foreach (var slot in Equipment)
                    {
                        if (slot.Item != null) items.Add(slot.Item);
                    }
                }
                return items;
            }
        }

        /// <summary>
        /// Used to determine if this model's internal grid has been setup. This is
        /// mostly used internally to determine if we are running in edit mode where
        /// the grid hasn't been initialized or play-mode where it has.
        /// </summary>
        public bool IsInitialized
        {
            get { return Slots != null; }
        }


        //Private fields
        protected SwapCache PreviousSwapResult = new SwapCache();

        [SerializeField]
        private PGISlotItem[,] Slots;

        protected bool _CanPerformAction = true;
        public bool CanPerformAction
        {
            get { return _CanPerformAction; }
            set
            {
                if (value == false) _CanPerformAction = false;
            }
        }

        /// <summary>
        /// Every time the internal state of the model changes, this value is incremented.
        /// This allows multiple views to observe when their representation is out of date
        /// without having to reset and dirty flags on the model.
        /// </summary>
        public uint DirtyId { get { return _DirtyId; } }
        uint _DirtyId;
#endregion


#region PGI Event Fields
        /// <summary>
        /// Invoked when a <see cref="PGISlotItem"/> is about to be stored in a grid location in this model.
        /// It is only called the first time the item attempts to enter a new inventory.
        /// You can disallow this action by setting the the provided model's
        /// <see cref="PGIModel.CanPerformAction"/> to <c>false</c>.
        /// <seealso cref="PGISlotItem.OnCanStore"/> 
        /// </summary>
        [SerializeField]
        [PGIFoldedEvent]
        public PGISlotItem.InventoryEvent OnCanStoreItem = new PGISlotItem.InventoryEvent();

        /// <summary>
        /// Invoked when a <see cref="PGISlotItem"/> is about to be stored in an equipment slot in this model.
        /// You can disallow this action by setting the the provided model's
        /// <see cref="PGIModel.CanPerformAction"/> to <c>false</c>.
        /// <seealso cref="PGISlotItem.OnCanEquip"/>
        /// <seealso cref="PGISlot.OnCanEquipItem"/>
        /// </summary>
        [SerializeField]
        [PGIFoldedEvent]
        public PGISlotItem.InventorySlotEvent OnCanEquipItem = new PGISlotItem.InventorySlotEvent();

        /// <summary>
        /// Invoked when a <see cref="PGISlotItem"/> is about to be removed from an inventory model.
        /// It is only called when the item will be completely and officially removed from the inventory.
        /// You can disallow this action by setting the the provided model's
        /// <see cref="PGIModel.CanPerformAction"/> to <c>false</c>.
        /// <seealso cref="PGISlotItem.OnCanRemove"/> 
        /// </summary>
        [SerializeField]
        [PGIFoldedEvent]
        public PGISlotItem.InventoryEvent OnCanRemoveItem = new PGISlotItem.InventoryEvent();

        /// <summary>
        /// Invoked when a <see cref="PGISlotItem"/> is about to be removed from an equipment slot in this model.
        /// You can disallow this action by setting the the provided model's
        /// <see cref="PGIModel.CanPerformAction"/> to <c>false</c>.
        /// <seealso cref="PGISlotItem.OnCanUnequip"/>
        /// <seealso cref="PGISlot.OnCanUnequipItem"/>
        /// </summary>
        [SerializeField]
        [PGIFoldedEvent]
        public PGISlotItem.InventorySlotEvent OnCanUnequipItem = new PGISlotItem.InventorySlotEvent();

        /// <summary>
        /// Invoked after a <see cref="PGISlotItem"/> has been stored in a new inventory model. It will not be called when
        /// the item is moved around within that inventory.
        /// <seealso cref="PGISlotItem.OnStoreInInventory"/>
        /// </summary>
        [SerializeField]
        [PGIFoldedEvent]
        public PGISlotItem.InventoryEvent OnStoreItem = new PGISlotItem.InventoryEvent();

        /// <summary>
        /// Invoked after a <see cref="PGISlotItem"/> has been removed from an inventory model.
        /// <seealso cref="PGISlotItem.OnRemoveFromInventory"/>
        /// </summary>
        [SerializeField]
        [PGIFoldedEvent]
        public PGISlotItem.InventoryEvent OnRemoveItem = new PGISlotItem.InventoryEvent();

        /// <summary>
        /// Invoked after a <see cref="PGISlotItem"/> has been equipped to an equipment slot.
        /// <seealso cref="PGISlotItem.OnEquip"/>
        /// <seealso cref="PGISlot.OnEquipItem"/>
        /// </summary>
        [SerializeField]
        [PGIFoldedEvent]
        public PGISlotItem.InventorySlotEvent OnEquipItem = new PGISlotItem.InventorySlotEvent();

        /// <summary>
        /// Invoked after a <see cref="PGISlotItem"/> has been removed from an equipment slot.
        /// This will occur even when dragging an item from the slot but before dropping it
        /// in a new location.
        /// <seealso cref="PGISlotItem.OnUnequip"/>
        /// <seealso cref="PGISlot.OnUnequipItem"/>
        /// </summary>
        [SerializeField]
        [PGIFoldedEvent]
        public PGISlotItem.InventorySlotEvent OnUnequipItem = new PGISlotItem.InventorySlotEvent();

        /// <summary>
        /// Invoked after an item has failed to be stored in an inventory. Usually this is
        /// the result of a 'Can...' method disallowing the item to be stored or simply
        /// the fact that there is not enough room for the item.
        /// <seealso cref="PGISlotItem.OnStoreInInventoryFailed"/>
        /// </summary>
        [SerializeField]
        [PGIFoldedEvent]
        public PGISlotItem.InventoryEvent OnStoreItemFailed = new PGISlotItem.InventoryEvent();

        /// <summary>
        /// Invoked after an item has failed to be equipped to an equipment slot. Usually this
        /// is the result of a 'Can...' method disallowing the item to be sotred or simply
        /// the fact that there was another item already located in the same slot. This
        /// method may be called frequiently when using <see cref="PGIMode.FindFreeSpace"/>
        /// or <see cref="PGIModel.Pickup"/>.
        /// <seealso cref="PGISlotItem.OnEquipFailed"/>
        /// <seealso cref="PGISlot.OnEquipItemFailed"/>
        /// </summary>
        [SerializeField]
        [PGIFoldedEvent]
        public PGISlotItem.InventorySlotEvent OnEquipItemFailed = new PGISlotItem.InventorySlotEvent();

        /// <summary>
        /// Invoked after loading a model and replacing this instance of the model with a newly deserialized one.
        /// This event can be used to let all concerned reference holders of this model know they need to update their reference.
        /// </summary>
        //[SerializeField]
        //[PGIFoldedEvent]
        [HideInInspector]
        [NonSerialized]
        public PGIModel.ModelEvent OnModelChanged = new PGIModel.ModelEvent();

        [SerializeField]
        [FoldFlag("Events")]
        public bool FoldedEvents = false; //used by the inspector

        /// <summary>
        /// Invoked when the model's grid is about to change size.
        /// </summary>
        /// <seealso cref="PGIModel.RefreshGridSize"/>
        [SerializeField]
        [PGIFoldedEvent]
        public UnityEvent OnBeginGridResize = new UnityEvent();

        /// <summary>
        /// Invoked just after a model's grid has finished resizing.
        /// </summary>
        /// <seealso cref="PGIModel.RefreshGridSize"/>
        [SerializeField]
        [PGIFoldedEvent]
        public UnityEvent OnEndGridResize = new UnityEvent();
#endregion


#region Private Methods
        void Awake()
        {
            RefreshGridSize(_GridCellsX, _GridCellsY);
        }

        void Start()
        {
            LinkEquipmentSlots();
        }

        void OnEnable()
        {
            LinkEquipmentSlots();
            AutoDetectItems = _AutoDetectItems;//this is used to get the ball rolling with the coroutines
            RefreshGridSize(_GridCellsX, _GridCellsY);
        }


        /// <summary>
        /// Helper method to link equipment slots back to this model.
        /// This should be called when the model is created, enabled, or loaded.
        /// </summary>
        void LinkEquipmentSlots()
        {
            //link equippment slots back to this model
            if (Equipment != null)
            {
                for (int i = 0; i < Equipment.Length; i++)
                {
                    var slot = Equipment[i];
                    if (slot != null)
                    {
                        slot.EquipmentIndex = i;
                        slot.xPos = -1;
                        slot.yPos = -1;
                        slot.ModelInitialized = true;
                    }
                }
            }
        }

        /// <summary>
        /// Used to periodically discover new items entering the model in the hierarchy
        /// without having registed in the model itself.
        /// </summary>
        /// <returns></returns>
        IEnumerator AutoDetectFunc()
        {
            while (AutoDetectItems)
            {
                UpdateModelList(DefaultDumpLocation, Slots, _GridCellsX, _GridCellsY);
                yield return new WaitForSeconds(AutoDetectRate);
            }

            AutoDetector = null;
        }

        /// <summary>
        /// Helper method used to determine if and how an item may belong to this model.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private ExistsIn ItemExistsInGridArray(PGISlotItem item, PGISlotItem[,] grid, int gridWidth, int gridHeight)
        {
            if (item.Model == null) return ExistsIn.NoModel;
            if (item.Model != this) return ExistsIn.AnotherModel;
            if (item != null)
            {
                //Does this item exist in our equipment list?
                if (item.IsEquipped && item.Equipped < Equipment.Length)
                {
                    if (Equipment[item.Equipped].Item == item) return ExistsIn.ThisModelComplete;
                    else return ExistsIn.ThisModelIncomplete;
                }

                //check for invalid equipment slot
                if (item.IsEquipped && item.Equipped >= Equipment.Length)
                    return ExistsIn.ThisModelInvalid;

                //confirm our location is valid
                if (item.IsStored)
                {
                    if (item.xInvPos < 0 || item.xInvPos + item.CellWidth > gridWidth ||
                        item.yInvPos < 0 || item.yInvPos + item.CellHeight > gridHeight)
                    {
                        return ExistsIn.ThisModelInvalid;
                    }
                }

                //Ok then, does this item exist somewhere in our model already?
                for (int j = 0; j < gridHeight; j++)
                {
                    for (int i = 0; i < gridWidth; i++)
                    {
                        if (grid[i, j] == item) return ExistsIn.ThisModelComplete;
                    }
                }

            }

            return ExistsIn.ThisModelIncomplete;
        }

        /// <summary>
        /// Scans the GameObject for any <see cref="PGISlotItem"/>s and confirms that they
        /// already exists in this model. If they do not, it attempts to add them to this model.
        /// If there is no room left, the remaining items they are moved in the transform hierarchy
        /// so that their parent is set to the DumpRoot transform.
        /// </summary>
        /// <param name="DumpRoot">The root transform to child object to if they are found to not
        /// belong and have no information about where they came from.</param>
        /// <param name="gridList">An option array list that represents the grid's contents. This
        /// is intended for internal use when resizing the grid and should not be needed by most users.</param>
        private void UpdateModelList(Transform DumpRoot, PGISlotItem[,] gridList, int gridWidth, int gridHeight)
        {
            //Check for items registed with the model that are not part of the heirarchy
            List<PGISlotItem> items = GetGridContents(gridList, 0, 0, gridWidth, gridHeight);// GetRangeContents(0, 0, GridCellsX, GridCellsY);


            foreach (PGISlotItem item in items)
            {
                if (item.transform.parent != this.transform) Drop(item);
            }
            items = EquipmentItems;
            if (items != null)
            {
                foreach (PGISlotItem item in items)
                {
                    if (item.transform.parent != this.transform) Drop(item);
                }
            }

            //Now check for items that are in the model hierarchy but aren't registed with the model.
            CachedChildren.Clear();
            //we need to cache a list of children so that if we dump any
            //we still process everything correctly.
            for (int childIndex = 0; childIndex < transform.childCount; childIndex++)
            {
                CachedChildren.Add(transform.GetChild(childIndex));
            }

            //check for 
            foreach (Transform child in CachedChildren)
            {
                GameObject go = child.gameObject;
                PGISlotItem item = go.GetComponent<PGISlotItem>();
                if (item != null)
                {
                    bool autoEquip = AutoEquip;
                    ExistsIn state = ItemExistsInGridArray(item, gridList, gridWidth, gridHeight);
                    switch (state)
                    {
                        case ExistsIn.AnotherModel:
                            {
                                //This item was added to this model's hierarchy but
                                //is still registed with another model. Remove it
                                //from that model and then add it to this one now.
                                //If we succeed then the item will be a valid child
                                //of this model, otherwise it will be properly dumped.
                                AutoEquip = false;
                                item.Model.Drop(item);
                                Pickup(item);
                                AutoEquip = autoEquip;
                                break;
                            }
                        case ExistsIn.NoModel:
                            {
                                //This item was added to this model's hierarchy but
                                //does not current have internal data pointing to this model.
                                //Simply add it to this model now. If we succeed then the
                                //item will be a valid child of this model, otherwise it
                                //will be properly dumped.
                                AutoEquip = false;
                                item.transform.SetParent(DumpRoot, false);
                                Pickup(item);
                                AutoEquip = autoEquip;
                                break;
                            }
                        case ExistsIn.ThisModelComplete:
                            {
                                //Everything is as it should be. Do nothing.
                                return;
                            }

                        case ExistsIn.ThisModelIncomplete:
                            {
                                //This item is referencing this model but its location
                                //information is invalid. Remove it from this model and
                                //then attempt to pick it back up. If we succeed then the
                                //item will be a valid child of this model, otherwise it
                                //will be properly dumped.
                                AutoEquip = false;
                                Drop(item);
                                Pickup(item);
                                AutoEquip = autoEquip;
                                break;
                            }
                        case ExistsIn.ThisModelInvalid:
                            {
                                //Our item is registered with this model and it's located
                                //in the model's hierarchy. However, the location within the
                                //model is not valid anymore (most likely due to a grid resizing)
                                //so now we need to see if we can fit it back in, and drop it if we can't.
                                //We do this by actually dropping it first, then attempting to pick it up again.
                                this.Drop(item);
                                this.Pickup(item);
                                break;
                            }
                    }//end switch
                }//endif PGISlotItem
            }
        }

        /// <summary>
        /// Utility for gathering all items stored within a region of grid space.
        /// Intended for internal use when swapping items around during a grid-resize.
        /// </summary>
        /// <param name="gridList">A 2D array representing a grid of slot items.</param>
        /// <param name="x">The x corrdinate to start at.</param>
        /// <param name="y">The y coordinate to start at.</param>
        /// <param name="width">The number of horizontal cells.</param>
        /// <param name="height">The number of vertical cells.</param>
        /// <returns></returns>
       private List<PGISlotItem> GetGridContents(PGISlotItem[,] gridList, int x, int y, int width, int height)
        {
            List<PGISlotItem> list = new List<PGISlotItem>(10);
            for (int t = y; t < y + height; t++)
            {
                for (int s = x; s < x + width; s++)
                {
                    var i = gridList[s, t];
                    if (i != null && !list.Contains(i)) list.Add(i);
                }
            }

            return list;
        }

        /// <summary>
        /// Updates the grid to match any changes to the
        /// horizontal or vertical grid count.
        /// </summary>
        /// <remarks>
        /// If this method is used in real-time during gameplay some items
        /// may be automatically re-arranged or even dropped if there is not
        /// enough room for them in the new grid.
        /// </remarks>
        private void RefreshGridSize(int newWidth, int newHeight)
        {
            if (newWidth < 0) newWidth = 0;
            if (newHeight < 0) newHeight = 0;

            OnBeginGridResize.Invoke();

            if (Slots == null || !Application.isPlaying)
            {
                _GridCellsX = newWidth;
                _GridCellsY = newHeight;

                Slots = new PGISlotItem[GridCellsX, GridCellsY];

                //let everyone know we are done
                OnEndGridResize.Invoke();

                //this is likely the first time we've initialized the grid. Don't worry about the other stuff.
                //We can infer this because the 'Slots' field was null.
                return;
            }

            //copy old contents before we dump anything and resize the internal grid.

            //dump everything from the grid.
            //WARNING: This will trigger all events so we may get
            //weird things like sound effect playing, graphical glitches, etc.
            var droppedItems = GetGridContents(Slots, 0, 0, _GridCellsX, _GridCellsY);
            foreach (PGISlotItem item in droppedItems) Drop(item);

            //resize the grid
            _GridCellsX = newWidth;
            _GridCellsY = newHeight;
            Slots = new PGISlotItem[GridCellsX, GridCellsY];

            //try to pickup eveything we just dropped.
            //BUG: This currently causes some slots to not reset properly. Not sure what is happening here.
            bool autoEquip = AutoEquip;
            AutoEquip = false;
            foreach (PGISlotItem item in droppedItems) Pickup(item);
            AutoEquip = autoEquip;

            //let everyone know we are done
            OnEndGridResize.Invoke();
        }

        /// <summary>
        /// Confirms that the given position is within
        /// the confines of the grid.
        /// </summary>
        /// <returns><c>true</c>, if valid cell position was confirmed, <c>false</c> otherwise.</returns>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        private bool ConfirmValidCellPos(int x, int y)
        {
            if (x >= GridCellsX) return false;
            if (y >= GridCellsY) return false;
            if (x < 0) return false;
            if (y < 0) return false;
            return true;
        }

        /// <summary>
        /// Confirms that the given position and dimension are within
        /// the confines of the grid.
        /// </summary>
        /// <returns><c>true</c>, if valid cell range was confirmed, <c>false</c> otherwise.</returns>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        private bool ConfirmValidCellRange(int x, int y, int width, int height)
        {
            if (ConfirmValidCellPos(x, y))
            {
                if (x + width > GridCellsX) return false;
                if (y + height > GridCellsY) return false;
                return true;
            }
            return false;
        }

#endregion


#region Can... Methods
        /// <summary>
        /// Determines if an item can be stored at the location (x,y).
        /// The location and required surrounding cells must be empty.
        /// </summary>
        /// <returns><c>true</c> if this model's grid can store the specified item at x y; otherwise, <c>false</c>.</returns>
        /// <param name="item">The item to be stored.</param>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="filter">An optional list of items to filter out when checking for available space
        /// Useful when you want to swap specific items.</param>
        public bool CanStore(PGISlotItem item, int x, int y, List<PGISlotItem> filter = null)
        {
            /*
            if (!BeginCanChecks)
            {
                BeginCanChecks = true;
                CanPerformAction = true;
            }
            else if(!CanPerformAction) return false;
            */
            if (item == null)
            {
                CanPerformAction = false;
                return false;
            }
            var items = this.GetRangeContents(x, y, item.CellWidth, item.CellHeight, filter);
            if (items == null || items.Count > 0)
            {
                CanPerformAction = false;
                return false;
            }

            _CanPerformAction = true;
            //'CanPerformAction' may change within this call.
            item.TriggerCanStoreEvents(this);
            return CanPerformAction;
        }

        /// <summary>
        /// Determines if the item can be dropped from this inventory.
        /// </summary>
        /// <returns><c>true</c> if this model can drop the specified item; otherwise, <c>false</c>.</returns>
        /// <param name="item">The item in question.</param>
        public bool CanDrop(PGISlotItem item)
        {
            if (item == null) CanPerformAction = false;
            _CanPerformAction = true;
            //'CanPerformAction' may change within this call.
            item.TriggerCanRemoveEvents(this);
            return CanPerformAction;
        }

        /// <summary>
        /// Determines if this instance can equip the specified item.
        /// </summary>
        /// <returns><c>true</c> if this model can equip the specified item in the given slot; otherwise, <c>false</c>.</returns>
        /// <param name="item">The item to equip.</param>
        /// <param name="equipSlot">The slot to equip the item to.</param>
        public bool CanEquip(PGISlotItem item, PGISlot equipSlot)
        {
            if (equipSlot.Item != null || equipSlot.Blocked)
            {
                CanPerformAction = false;
                return false;
            }

            _CanPerformAction = true;
            //'CanPerformAction' may change within this call.
            item.TriggerCanEquipEvents(this, equipSlot);
            return CanPerformAction;
        }

        /// <summary>
        /// Determines whether this instance can unequip the specified item.
        /// </summary>
        /// <returns><c>true</c> if this instance can unequip the specified item; otherwise, <c>false</c>.</returns>
        /// <param name="item">The item that is to be unequipped.</param>
        /// <param name="equipSlot">The slot that the item is being unequipped from.</param>
        public bool CanUnequip(PGISlotItem item, PGISlot equipSlot)
        {
            if (item == null || equipSlot == null)
            {
                CanPerformAction = false;
                return false;
            }
            _CanPerformAction = true;
            //'CanPerformAction' may change within this call.
            item.TriggerCanUnequipEvents(this, equipSlot);
            return CanPerformAction;
        }

        /// <summary>
        /// Resets the swap cache of the previous CanSwap test.
        /// This should be called any time a drag ends to ensure the next
        /// drag event has a fresh cache.
        /// </summary>
        public void ResetSwapCache()
        {
            PreviousSwapResult.DraggedItem = null;
            PreviousSwapResult.SwapSlot = null;
        }

        /// <summary>
        /// Determines if an item can be swapped with the contents of another equipment slot.
        /// </summary>
        /// <returns><c>true</c> if this model can store the specified item with or without having to swap another item currently stored there; otherwise, <c>false</c>.</returns>
        /// <param name="item">The item to store</param>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        public bool CanSwap(PGISlotItem item, PGISlot equipSlot)
        {
            if (item == null || equipSlot == null)
            {
                CanPerformAction = false;
                return false;
            }

            //Test for this particula swap event. If it already happened we don't need to do it again.
            //NOTE: This cache is cleared by the view whenever a drag ends
            if (!PreviousSwapResult.IsNewSwapAttempt(equipSlot, item))
            {
                return PreviousSwapResult.SwapResult;
            }

            if (equipSlot.Item == null)
            {
                _CanPerformAction = true;
                item.TriggerCanEquipEvents(this, equipSlot);
                PreviousSwapResult.SwapResult = CanPerformAction;
                return CanPerformAction;
            }



            //The very first thing we need to do is
            //remove our items from their positions.
            CanPerformAction = false;
            PGISlotItem swap = equipSlot.Item;
            if (Unequip(swap) != null)
            {
                if (Equip(item, equipSlot) != null)
                {
                    Pos temp = FindFirstFreeSpace(swap);
                    if (temp != null && Store(swap, temp.X, temp.Y)) //this resets 'CanPerformAction' to true
                    {
                        _CanPerformAction = true;
                        Remove(swap, false);
                    }
                    else CanPerformAction = false; //need to do this because 'Store()' sets it to true again
                    Unequip(item, false);
                }
                Equip(swap, equipSlot, false);
            }

            PreviousSwapResult.SwapResult = CanPerformAction;
            return CanPerformAction;
        }

        /// <summary>
        /// Determines if the item can be stacked with the contents of the given slot.
        /// </summary>
        /// <param name="item">The item to stack.</param>
        /// <param name="slot">The slot that is being checked for stack compatibility.</param>
        /// <returns></returns>
        public bool CanStack(PGISlotItem item, PGISlot slot)
        {
            if (item == null || slot == null || slot.Item == null)
            {
                CanPerformAction = false;
                return false;
            }

            if(slot.Item == null)
            //there is exactly 1 item here, check to see if we can stack
            if (item.MaxStack <= 1 || slot.Item.MaxStack <= 1 ||
                slot.Item.ItemTypeId != item.ItemTypeId ||
                item.ItemTypeId < 1 ||
                slot.Item.StackCount >= slot.Item.MaxStack)
            {
                CanPerformAction = false;
                return false;
            }

            //looks like we can stack
            _CanPerformAction = true;
            //'CanPerformAction' may change within this call.
            item.TriggerCanStoreEvents(this);
            return CanPerformAction;
    }

        /// <summary>
        /// Determines if an item can be stacked with any item potentially located on the grid at the specified region.
        /// </summary>
        /// <param name="item">The item to be stacked.</param>
        /// <param name="x">The left corner of the region to check for stack-compatible items.</param>
        /// <param name="y">The top corner of the region to check for stack-compatible items.</param>
        /// <returns></returns>
        public bool CanStack(PGISlotItem item, int x, int y)
        {
            if (item == null)
            {
                CanPerformAction = false;
                return false;
            }
            var items = this.GetRangeContents(x, y, item.CellWidth, item.CellHeight);
            if (items == null)
            {
                CanPerformAction = false;
                return false;
            }
            else if (items.Count > 1)
            {
                CanPerformAction = false;
                return false;
            }
            //TODO: it might be better to check all items for a valid stack target
            else if (items.Count == 1)
            {
                //there is exactly 1 item here, check to see if we can stack
                if (item.MaxStack <= 1 || items[0].MaxStack <= 1 ||
                    items[0].ItemTypeId != item.ItemTypeId ||
                    item.ItemTypeId < 1 ||
                    items[0].StackCount >= items[0].MaxStack)
                {
                    CanPerformAction = false;
                    return false;
                }

                //looks like we can stack
                _CanPerformAction = true;
                //'CanPerformAction' may change within this call.
                item.TriggerCanStoreEvents(this);
                return CanPerformAction;
            }

            //there is nothing to stack
            CanPerformAction = false;
            return false;
        }

#if !PGI_LITE
        /// <summary>
        /// Checks to see if a socketable item can be placed into a socketed item while contained within this inventory.
        /// </summary>
        /// <param name="socketable">The item that is being placed into another socketed item.</param>
        /// <param name="socketed">The item receiving the socketable.</param>
        /// <returns><c>true</c> if the items are compatible, <c>false</c> otherwise.</returns>
        public bool CanSocket(PGISlotItem socketable, PGISlotItem socketed)
        {
            if (!AllowSocketing)
            {
                CanPerformAction = false;
                return false;
            }
            if (socketed == null || socketable == null)
            {
                CanPerformAction = false;
                return false;
            }
            var thing = socketable.GetComponent<Socketable>();
            if (thing != null)
            {
                var receiver = socketed.GetComponent<Socketed>();
                if (receiver != null && receiver.SocketId == thing.SocketId && receiver.GetFirstEmptySocket() != -1)
                {
                    _CanPerformAction = true;
                    receiver.OnCanSocket.Invoke(this, receiver, thing);
                    thing.OnCanSocket.Invoke(this, receiver, thing);
                    return CanPerformAction;
                }
            }
            return false;
        }
#endif
#endregion


#region Public Methods
        /// <summary>
        /// Arranges item in inventory with an attempt to minimize space used.
        /// </summary>
        /// <param name="allowRotation">If <c>true</c> items can be rotated to make them fit better.</param>
        public void ArrangeItems(bool allowRotation)
        {
            ArrangeItems(allowRotation, PGISlotItem.RotateDirection.CW);
        }

        /// <summary>
        /// Arranges item in inventory with an attempt to minimize space used.
        /// </summary>
        /// <param name="allowRotation">If <c>true</c> items can be rotated to make them fit better.</param>
        /// <returns></returns>
        public void ArrangeItems(bool allowRotation, PGISlotItem.RotateDirection rotateDir)
        {
            _DirtyId++;

            //We're going prioritize what we put back first and base it on the dimensions
            //of the item, dimensions of the grid, and wether or not rotation an be used.
            //There might also be some magic fairy dust in here too.
            PGISlotItem[] sortedItems = new PGISlotItem[GridItems.Count];
            GridItems.CopyTo(sortedItems, 0);
            PGISlotItem.SortByWidth = false; //this may change if we are rotating items below
            Dictionary<PGISlotItem, PGIModel.Pos> origLocations = new Dictionary<PGISlotItem, Pos>(_GridCellsX * _GridCellsY);
            bool smallerWidth = (this._GridCellsX < this._GridCellsY) ? true : false;
            

            //take em all out
            foreach (PGISlotItem item in GridItems)
            {
                origLocations.Add(item, new Pos(item.xInvPos, item.yInvPos));
                //store the location of each item before removing it.
                //That way if our new arrangement doesn't have enough space
                //we can put it all back
                Remove(item, false);
            }


           //first of all, can we rotate?
            if(allowRotation)
            {
                
                //We can rotate. Let's try to rotate all items
                //that are 'closer' to the smallest dimension
                //of the grid itself.
                foreach(var item in sortedItems)
                {
                    //if our inventory is taller than it is wide and the item will have a closer fit by rotating then we'll rotate
                    if(smallerWidth)
                    {
                        if (item.CellWidth < item.CellHeight && item.CellWidth <= _GridCellsX)
                        {
                            if (item.Rotated) item.Rotate(PGISlotItem.RotateDirection.None);
                            else item.Rotate(rotateDir);
                        }
                        //we want to store the widest items first
                        PGISlotItem.SortByWidth = true;
                    }
                    //otherwise, our inventory is wider than tall, and if the item fits better rotated we'll do it that way.
                    else
                    {
                        if (item.CellHeight < item.CellWidth && item.CellHeight <= _GridCellsY)
                        {
                            if (item.Rotated) item.Rotate(PGISlotItem.RotateDirection.None);
                            else item.Rotate(rotateDir);
                        }
                        //we want to store the tallest items first
                        PGISlotItem.SortByWidth = false;
                    }
                }
            }

            //sort items by size
            Array.Sort(sortedItems);
            
            //put em all back
            foreach(PGISlotItem item in sortedItems)
            {
                if(!this.StoreAtFirstFreeSpaceIfPossible(item))
                {
                    //If we can't fit something back in, let's just revert to the original positions and be done with it.
                    ReturnItemsToOriginalPosition(origLocations);
                    return;
                }
            }
        }

        /// <summary>
        /// Helper used to return items to their original locations if a re-arrangement can't fit them all in.
        /// </summary>
        /// <param name="items"></param>
        /// <param name="orig"></param>
        void ReturnItemsToOriginalPosition(Dictionary<PGISlotItem, PGIModel.Pos> originalItemLocations)
        {
            foreach (PGISlotItem item in originalItemLocations.Keys)
            {
                if(item.IsStored) Remove(item, false);
            }

            foreach (KeyValuePair<PGISlotItem, PGIModel.Pos> pair in originalItemLocations)
            {
                Store(pair.Key, pair.Value.X, pair.Value.Y, false);
            }
        }

        /// <summary>
        /// Given an item, this method will attempt to find the nearest position
        /// that it can fit into when rotated. This new position will have at least
        /// one overlapping cell with the original unrotated position of the item.
        /// </summary>
        /// <param name="preRotatedItem">The item for which to find a rotated vailence position. Take note that the dimension for this item should be the pre-rotated ones.</param>
        /// <returns>The new root cell position that shares rotated vailence or null if none could be found.</returns>
        public Pos FindVailencePosition(PGISlotItem preRotatedItem, PGISlotItem.RotateDirection dir)
        {
            _DirtyId++;
            int width = preRotatedItem.CellHeight;
            int height = preRotatedItem.CellWidth;

            //We will start by rotating the object around it's current cell location.
            //If that doesn't work we will shift the object so that each cell slot it takes
            //has an opportunity to be used as a rotation point until we find enough space to
            //perform a successfull rotation. If none are found they this item will not fit
            //within the space it was located when rotated and we must return failure.

            if(dir == PGISlotItem.RotateDirection.CW || (dir == PGISlotItem.RotateDirection.None && preRotatedItem.RotatedDir == PGISlotItem.RotateDirection.CCW))
            {
                //clockwise rotation
                for (int y = preRotatedItem.yInvPos; y < preRotatedItem.yInvPos + height; y++)
                {
                    for (int x = preRotatedItem.xInvPos; x < preRotatedItem.xInvPos + width; x++)
                    {
                        var list = GetRangeContents(x, y, width, height, new List<PGISlotItem>(new PGISlotItem[] { preRotatedItem }));
                        if (list == null) continue;
                        else if (list.Count != 0) continue;
                        return new Pos(x, y);
                    }
                }

                for (int y = preRotatedItem.yInvPos; y > preRotatedItem.yInvPos - height; y--)
                {
                    for (int x = preRotatedItem.xInvPos; x > preRotatedItem.xInvPos - width; x--)
                    {
                        var list = GetRangeContents(x, y, width, height, new List<PGISlotItem>(new PGISlotItem[] { preRotatedItem }));
                        if (list == null) continue;
                        else if (list.Count != 0) continue;
                        return new Pos(x, y);
                    }
                }
            }
            else
            {
                //counter-clockwise rotation
                for (int y = preRotatedItem.yInvPos; y > preRotatedItem.yInvPos - height; y--)
                {
                    for (int x = preRotatedItem.xInvPos; x > preRotatedItem.xInvPos - width; x--)
                    {
                        var list = GetRangeContents(x, y, width, height, new List<PGISlotItem>(new PGISlotItem[] { preRotatedItem }));
                        if (list == null) continue;
                        else if (list.Count != 0) continue;
                        return new Pos(x, y);
                    }
                }

                for (int y = preRotatedItem.yInvPos; y < preRotatedItem.yInvPos + height; y++)
                {
                    for (int x = preRotatedItem.xInvPos; x < preRotatedItem.xInvPos + width; x++)
                    {
                        var list = GetRangeContents(x, y, width, height, new List<PGISlotItem>(new PGISlotItem[] { preRotatedItem }));
                        if (list == null) continue;
                        else if (list.Count != 0) continue;
                        return new Pos(x, y);
                    }
                }
            }

            
           
            return null;
            
        }

        /// <summary>
        /// Helper method for flagging the indexed equipment slot
        /// as 'dirty' so that the UI knows when and where to update
        /// the visual components of the inventory.
        /// </summary>
        /// <param name="index"></param>
        public void MarkDirtyEquipmentSlot(int index)
        {
            if (index < 0 || index >= Equipment.Length) return;
            Equipment[index].Dirty = true;

            if (OnUpdateDirty != null) OnUpdateDirty();
        }

        /// <summary>
        /// Helper method than marks the location used by the <see cref="PGISlotItem"/> as 'dirty'
        /// so that the UI knows when and where to update the visual components
        /// of the inventory. The item can be either in the grid or an
        /// equipment slot.
        /// </summary>
        /// <param name="item"></param>
        public void MarkDirty(PGISlotItem item)
        {
            if (item.IsEquipped)
            {
                //this also invokes 'OnUpdateDirty()'
                MarkDirtyEquipmentSlot(item.Equipped);
                return;
            }
            if (OnUpdateDirty != null) OnUpdateDirty();
        }

        /// <summary>
        /// Helper used to mark all slots and items as dirty. Usually used when loading
        /// so that we can inform the view to update everything.
        /// </summary>
        public void MarkAllDirty()
        {
            if (Equipment != null)
            {
                for (int i = 0; i < Equipment.Length; i++)
                {
                    if(Equipment[i] != null) Equipment[i].Dirty = true;
                }
            }

            if (OnUpdateDirty != null) OnUpdateDirty();
        }

        /// <summary>
        /// Gets all slots that have had their 'Dirty' flags marked. 
        /// Currently the grid does not use slots to store
        /// items so this will only return equipment slots.
        /// </summary>
        /// <returns>The all dirty slots.</returns>
        public List<PGISlot> GetAllDirtySlots()
        {
            List<PGISlot> dirty = new List<PGISlot>(20);
            if (Equipment != null)
            {
                foreach (PGISlot slot in Equipment)
                {
                    if (slot != null && slot.Dirty) dirty.Add(slot);
                }
            }

            return dirty;
        }

        /// <summary>
        /// Gets the contents of the given (x,y) location.
        /// If the location is empty or invalid then
        /// 'null' is returned instead.
        /// </summary>
        /// <returns>The <see cref="PGISlotItem"/> stored at the location or null if there isn't one.</returns>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        public PGISlotItem GetSlotContents(int x, int y)
        {
            if (!ConfirmValidCellPos(x, y)) return null;
            return Slots[x, y];
        }

        /// <summary>
        /// Determines if the slot at the given location is empty.
        /// </summary>
        /// <returns><c>true</c>, if there is no item, <c>false</c> otherwise.</returns>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        public bool SlotIsEmpty(int x, int y)
        {
            return (GetSlotContents(x, y) == null);
        }

        /// <summary>
        /// Gets all <see cref="PGISlotItem"/>s  within the given area.
        /// If the area is empty then an empty list is returned.
        /// If the area is invalid then null is returned. 
        /// </summary>
        /// <returns>The range contents as ab array of <see cref="PGISlotItem"/>s.</returns>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        /// <param name="filter">An optional list of items to filter out of the returned array.</param> 
        public List<PGISlotItem> GetRangeContents(int x, int y, int width, int height, List<PGISlotItem> filter = null)
        {
            if (!ConfirmValidCellRange(x, y, width, height)) return null;
            List<PGISlotItem> list = new List<PGISlotItem>(20);
            for (int t = y; t < y + height; t++)
            {
                for (int s = x; s < x + width; s++)
                {
                    var i = GetSlotContents(s, t);
                    if (i != null && !list.Contains(i))
                    {
                        if (filter == null || !filter.Contains(i)) list.Add(i);
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Determines if the given grid-area is empty.
        /// </summary>
        /// <returns><c>true</c>, if is empty was ranged, <c>false</c> otherwise.</returns>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        /// <param name="filter">An optional list of items to filter out of searched area.</param>
        public bool RangeIsEmpty(int x, int y, int width, int height, List<PGISlotItem> filter = null)
        {
            var contents = GetRangeContents(x, y, width, height, filter);
            if (contents != null && contents.Count < 1) return true;
            return false;
        }
        
        /// <summary>
        /// Specialized method that is used internally by the model when storing like-items.
        /// It is called within the <see cref="PGIModel.Store"/> and <see cref="PGIModel.Equip"/> methods.
        /// </summary>
        /// <remarks>
        /// This method should be considered highly volitile as it will usually destroy the object
        /// being stacked. Stacking works by destroying the object to be stacked and incrementing
        /// the stack counter stored internally in the item that it was stacked with. Later. they can
        /// be unstacked by de-incrementing the counter and instantiating a copy of the item.
        /// <para>
        /// As a concequence of stacking you cannot expect unique items with different internal data
        /// to be properly stored and retreived and should only supply the same stack ID to items
        /// that are exactly alike.
        /// </para>
        /// </remarks>
        /// <returns><c>null</c> if the item was fully stacked and destroyed with no remainder.
        /// Otherwise a reference to the item with the remainder will be returned.</returns>
        /// <param name="item">The item to stack. This item may be destroyed by the end of this method.</param>
        /// <param name="stack">The item that the item will be stacked with.</param>
        /// <param name="stackOccurred">A flag that signifies if the item was stacked in any way.</param>
        PGISlotItem StackStore(PGISlotItem item, PGISlotItem stack, out bool stackOccurred)
        {
            if (item == null) throw new UnityException("Invalid item passed to PGIModel.StackStore()");
            if (stack == null) throw new UnityException("Invalid stack passed to PGIModel.StackStore()");

            stackOccurred = false;
            if (item.ItemTypeId == stack.ItemTypeId && stack.StackCount < stack.MaxStack)
            {
                stack.StackCount += item.StackCount;
                int remainder = stack.StackCount - stack.MaxStack;
                if (remainder <= 0)
                {
                    MarkDirty(item);
                    if(item.Model != stack.Model) MarkDirty(stack);

                    //nothing left
                    GameObject.Destroy(item.gameObject);
                    stackOccurred = true;
                    return null;
                }
                else
                {
                    //there was a remainder
                    stack.StackCount = stack.MaxStack;
                    item.StackCount = remainder;
                    stackOccurred = true;
                }
            }
            if (item.Model != stack.Model) MarkDirty(stack);
            MarkDirty(item);
            return item;
        }

#if !PGI_LITE
        /// <summary>
        /// Attempts to attach one socketable item to another socketed one.
        /// </summary>
        /// <param name="receiver">The socketed item that will receive the other.</param>
        /// <param name="thing">The socketable item that will be attached.</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        bool Socket(Socketed receiver, Socketable thing)
        {
            if (receiver.AttachSocketable(thing) != -1)
            {
                thing.transform.SetParent(receiver.transform, false);
                receiver.OnSocketed.Invoke(this, receiver, thing);
                thing.OnSocketed.Invoke(this, receiver, thing);
                return true;
            }

            return false;
        }
#endif

        /// <summary>
        /// Attempts to store the item at the given location in this model's grid.
        /// </summary>
        /// <returns><c>true</c>, if the item was stored, <c>false</c> otherwise.</returns>
        /// <param name="item">The item to store.</param>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="checkCanMethods">Optional parameter. If <c>true</c>, all attached 'Can...' methods
        /// associated with the item and this model will be called to verify that the item can be properly located
        /// at the given location. It is recommened that this be used to avoid state-corruption.</param>
        public bool Store(PGISlotItem item, int x, int y, bool checkCanMethods = true)
        {
            _DirtyId++;

#if !PGI_LITE
            //check socketing before stacking or storing
            var destSlot = GetSlotContents(x, y);
            if (checkCanMethods && CanSocket(item, destSlot))
            {
                //THINGS TO CONSIDER:
                // 1) We need to consider socketing stacked items. We should only socket one at a time
                //  so we'll need to split the stack.
                // 2) When we socket an item we need to remove it from the inventory and make it belong
                //  to the item it is being attached to.
                if (Socket(destSlot.GetComponent<Socketed>(), item.GetComponent<Socketable>()))
                {
                    MarkDirty(item);
                    return true;
                }
            }
#endif

            //check for stacking next
            if (checkCanMethods && CanStack(item, x, y))
            {
                try
                {
                    bool success;
                    //techincally, this might succeed but we still need to return remainders
                    //so we'll return false to let the handler do what it needs to with the original item/stack.
                    if (StackStore(item, GetSlotContents(x, y), out success) != null)
                        if(success) return false;
                }
                catch (UnityException e)
                {
                    Debug.LogError(e.Message);
                    return false;
                }
                MarkDirty(item);
                return true;
            }
            //if not stacking or socketing, check slot for availablility
            else if (checkCanMethods && !CanStore(item, x, y))
            //if (checkCanMethods && !CanStore(item, x, y))
            {
                item.TriggerStoreFailedEvents(this);
                return false;
            }

            //we need to assign the item to
            //the full range of slots being used by it
            for (int t = y; t < y + item.CellHeight; t++)
            {
                for (int s = x; s < x + item.CellWidth; s++)
                {
                    Slots[s, t] = item;
                }
            }

            
            item.ProcessStorage(this, x, y);
            MarkDirty(item);
            //NOTE: We aren't calling the events for the item
            //because this might be a drag n' drop operation. We'll
            //let the caller UI handle that part.
            
            return true;

        }

        static PGISlot TempSlot;
        static PGISlot TempItem;
        /// <summary>
        /// Stores the item in the inventory at the first available space if possible.
        /// <para>
        ///  If this model's <see cref="PGIModel.AutoEquip"/> flag is set then equipment
        ///  slots will be considered when searching for space. Equipment slots will be
        ///  considered only after the grid has been determined to have insufficent space
        ///  unless the <see cref=" PGIModel.AutoEquipFirst"/> flags is also set.
        /// </para>
        /// <para>
        /// If this model's <see cref="PGIModel.AutoStack"/> flag is set and the item is
        /// stackable, the inventory will be searched for like items to stack it with first.
        /// </para>
        /// </summary>
        /// <returns><c>true</c>, if the item was stored, <c>false</c> otherwise.</returns>
        /// <param name="item">The item to store.</param>
        public bool StoreAtFirstFreeSpaceIfPossible(PGISlotItem item)
        {
            _DirtyId++;
            if (item == null || item.Model == this) return false;

            //Check for stacking first, if anything is left over 
            //we'll drop down to the 'single-item' section.
            if (item.MaxStack > 1 && AutoStack)
            {
                foreach(PGISlotItem thing in Slots)
                {
                    if (thing != null && thing.MaxStack > 1 && thing.ItemTypeId == item.ItemTypeId && thing.StackCount < thing.MaxStack)
                    {
                        bool success;
                        if (StackStore(item, thing, out success) == null)
                        {
                            //we got nothing back, we can safely return success
                            return true;
                        }
                        //at this point we must have leftovers of some kind. We'll
                        //drop down to try storing the rest as normal.
                    }
                }
            }

            //Store as a single item. If anything was left over
            //from the stack-section above it will get handled here.
            //check equipment slots first
            if (AutoEquip && AutoEquipFirst && Equipment != null)
            {
                for (int i = 0; i < Equipment.Length; i++)
                {
                    TempSlot = Equipment[i];
                    if (TempSlot != null && !TempSlot.SkipAutoEquip && Equip(item, TempSlot)) return true;
                }
                
            }

            var pair = FindFirstFreeSpace(item);
            if (pair != null)
            {
                return Store(item, pair.X, pair.Y);
            }

            if (AutoEquip && !AutoEquipFirst && Equipment != null)
            {
                for (int i = 0; i < Equipment.Length; i++)
                {
                    TempSlot = Equipment[i];
                    if (TempSlot != null && !TempSlot.SkipAutoEquip && Equip(item, TempSlot)) return true;
                }
            }
            TempSlot = null; //always make sure this is null when done with it to avoid leaks
            return false;
        }

        /// <summary>
        /// Specialized storage method. This is often used to introduce an item to
        /// the inventory from the outside world. It provides a few nice features
        /// that other storage method don't, like trigger all associated events
        /// and automatcially find storage space for the item.
        /// <para>
        ///  If this model's <see cref="PGIModel.AutoEquip"/> flag is set then equipment
        ///  slots will be considered when searching for space. Equipment slots will be
        ///  considered only after the grid has been determined to have insufficent space
        ///  unless the <see cref=" PGIModel.AutoEquipFirst"/> flags is also set.
        /// </para>
        /// </summary>
        /// <remarks>This is the reccomended way of intially inserting an item into an inventory
        /// through code as it will properly trigger all attached events for the item, model, slot, and view.
        /// </remarks>
        /// <returns><c>true</c>, if the item was stored, <c>false</c> otherwise.</returns>
        /// <param name="item">The item to store.</param>
        public bool Pickup(PGISlotItem item)
        {
            if (StoreAtFirstFreeSpaceIfPossible(item))
            {
                item.TriggerStoreEvents(this);
                if (item.IsEquipped) item.TriggerEquipEvents(this, Equipment[item.Equipped]);
                return true;
            }
            item.TriggerStoreFailedEvents(this);
            return false;
        }

        /// <summary>
        /// Specialized storage method. This is often used to safely and completely remove
        /// an item from an inventory while ensuring that all of the correct events are triggered.
        /// The item will be automatically unequipped if it was equipped.
        /// </summary>
        /// <remarks>This is the recommended way of programmatically removing an item from an inventory
        /// as it will ensure the item is unequipped if previously equipped and will trigger all of the
        /// necessary events for the item, model, slot, and view for unequipping and removal.
        /// </remarks>
        /// <param name="item">The item to drop.</param>
        /// <returns><c>true</c>, if the item was dropped, <c>false</c> otherwise.</returns>
        public bool Drop(PGISlotItem item)
        {
            _DirtyId++;
            if (item.Model == null || item.Model != this) return false;
            if (!item.IsEquipped && !item.IsStored) return false;

            //unequipping and removal both handle resetting the item's internal references to
            //the model and slot index. So we don't need to handle 'Remove(item)' in both cases.
            if (item.IsEquipped)
            {
                int slotIndex = item.Equipped;
                if (Unequip(item) == null) return false;
                item.TriggerUnequipEvents(this, Equipment[slotIndex]);
            }
            else if (Remove(item) == null) return false;
            item.TriggerRemoveEvents(this);

            return true;
        }

        /// <summary>
        /// Checks to see if the model has room for the item.
        /// </summary>
        /// <param name="item">The item to check for room.</param>
        /// <returns><c>true</c> if this model has room for the item, <c>false</c> otherwise.</returns>
        public bool HasRoomForItem(PGISlotItem item, bool checkEquipSlot, bool checkStacks)
        {
            if (item == null) return false;

            for (int t = 0; t < GridCellsY; t++)
            {
                for (int s = 0; s < GridCellsX; s++)
                {
                    if (CanStore(item, s, t)) return true;
                    if (checkStacks && CanStack(item, s, t)) return true;
                }
            }

            if (checkEquipSlot)
            {
                foreach (var slot in Equipment)
                {
                    if (CanEquip(item, slot)) return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Drops the item from this inventory model that is located at the given grid position.
        /// </summary>
        /// <returns>The item that was dropped if successful, otherwise null.</returns>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="checkCanMethods">Optional parameter. If <c>true</c>, all attached 'Can...' methods
        /// associated with the item and this model will be called to verify that the item can be properly located
        /// at the given location. It is recommened that this be used to avoid state-corruption.</param>
        public PGISlotItem Remove(int x, int y, bool checkCanMethods = true)
        {
            return Remove(GetSlotContents(x, y), checkCanMethods);
        }

        /// <summary>
        /// Drops the specified item from this inventory model and returns it if successful.
        /// </summary>
        /// <returns>The item that was dropped if successful, otherwise null.</returns>
        /// <param name="item">The item to drop.</param>
        /// <param name="checkCanMethods">Optional parameter. If <c>true</c>, all attached 'Can...' methods
        /// associated with the item and this model will be called to verify that the item can be properly located
        /// at the given location. It is recommened that this be used to avoid state-corruption.</param>
        public PGISlotItem Remove(PGISlotItem item, bool checkCanMethods = true)
        {
            _DirtyId++;
            if (item == null) throw new UnityException("Missing item reference.");
            if (checkCanMethods && !CanDrop(item))
            {
                return null;
            }

            if (item.Equipped > -1)
            {
                return Unequip(item, checkCanMethods);
            }
            else
            {
                //Unassign all slots that were used by the item.
                for (int t = item.yInvPos; t < item.yInvPos + item.CellHeight; t++)
                {
                    for (int s = item.xInvPos; s < item.xInvPos + item.CellWidth; s++)
                    {
                        Slots[s, t] = null;
                    }
                }
                item.ProcessRemoval();
                MarkDirty(item);

                //NOTE: We aren't call the events for the item
                //because this might be a drag n' drop operation. We'll
                //let the caller UI handle that part.
                return item;
            }
        }

        /// <summary>
        /// Attempts to equip the item to the given equipment slot in this inventory's model.
        /// </summary>
        /// <remarks>
        /// This method is mostly intended for internal use only as it does not perform any
        /// swap behaviour if another item is already located in the destination slot.
        /// <seealso cref="PGIModel.SwapEquip"/>
        /// </remarks>
        /// <returns>The item if it was equipped. Null otherwise.</returns>
        /// <param name="item">The item to equip.</param>
        /// <param name="dest">The equipment slot that the item will be equipped to. <seealso cref="PGIModel.Equipment"/></param>
        /// <param name="checkCanMethods">Optional parameter. If <c>true</c>, all attached 'Can...' methods
        /// associated with the item and this model will be called to verify that the item can be properly located
        /// at the given location. It is recommened that this be used to avoid state-corruption.</param>
        public PGISlotItem Equip(PGISlotItem item, PGISlot dest, bool checkCanMethods = true)
        {
            _DirtyId++;
            if (Equipment == null) return null;

            bool found = false;
            int equipmentIndex = 0;
            if (checkCanMethods)
            {
                if (!CanEquip(item, dest))
                {
                    item.TriggerEquipFailedEvents(this, dest);
                    return null;
                }
            }
            foreach (PGISlot equip in Equipment)
            {
                if (equip == dest) { found = true; break; }
                equipmentIndex++;
            }
            if (found)
            {
                Equipment[equipmentIndex].AssignItem(item, false);
                item.ProcessEquip(this, equipmentIndex);
                MarkDirty(item);
                //NOTE: We aren't triggering any events for the item
                //because this might be a drag n' drop operation. We'll
                //let the caller UI handle that part.
                return item;
            }
            //We will, however, trigger any failure events
            item.TriggerEquipFailedEvents(this, dest);
            return null;
        }

        /// <summary>
        /// Attempts to equip the item to the given equipment slot in this inventory's model.
        /// </summary>
        /// <remarks>
        /// This method is mostly intended for internal use only as it does not perform any
        /// swap behaviour if another item is already located in the destination slot.
        /// <seealso cref="PGIModel.SwapEquip"/>
        /// </remarks>
        /// <returns>The item if it was equipped. Null otherwise.</returns>
        /// <param name="item">The item to equip.</param>
        /// <param name="equipmentIndex">The array index to the equipment slot that the item will be equipped to. <seealso cref="PGIModel.Equipment"/></param>
        /// <param name="checkCanMethods">Optional parameter. If <c>true</c>, all attached 'Can...' methods
        /// associated with the item and this model will be called to verify that the item can be properly located
        /// at the given location. It is recommened that this be used to avoid state-corruption.</param>
        public PGISlotItem Equip(PGISlotItem item, int equipmentIndex, bool checkCanMethods = true)
        {
            _DirtyId++;
            if (Equipment == null) return null;
            PGISlot dest = null;
            int index = 0;
            foreach (PGISlot equip in Equipment)
            {
                if (index == equipmentIndex) { dest = equip; break; }
                index++;
            }
            if (checkCanMethods)
            {
                if (!CanEquip(item, dest))
                {
                    item.TriggerEquipEvents(this, dest);
                    return null;
                }
            }

            if (dest != null)
            {
                Equipment[equipmentIndex].AssignItem(item);
                item.ProcessEquip(this, equipmentIndex);
                MarkDirty(item);
                //NOTE: We aren't triggering any equip events for the item
                //because this might be a drag n' drop operation. We'll
                //let the caller UI handle that part.
                return item;
            }
            //we will, however, trigger any failure events
            item.TriggerEquipFailedEvents(this, dest);
            return null;
        }

        /// <summary>
        /// Unequips the specified item from the equipment slot.
        /// </summary>
        /// <remarks>
        /// Internally the item will be considered removed from this inventory but no events associated
        /// with unequipping or removing will be triggered on the model, view, slot, or item. It is
        /// up to the caller of this method to determine what events will be triggered when and how.
        /// </remarks>
        /// <param name="item">The item to unequip.</param>
        /// <param name="checkCanMethods">Optional parameter. If <c>true</c>, all attached 'Can...' methods
        /// associated with the item and this model will be called to verify that the item can be properly located
        /// at the given location. It is recommened that this be used to avoid state-corruption.</param>
        public PGISlotItem Unequip(PGISlotItem item, bool checkCanMethods = true)
        {
            _DirtyId++;
            if (checkCanMethods && !CanUnequip(item, Equipment[item.Equipped])) return null;
            Equipment[item.Equipped].AssignItem(null);
            item.ProcessRemoval();
            MarkDirty(item);
            //NOTE: We aren't call the events for the item
            //because this might be a drag n' drop operation. We'll
            //let the caller UI handle that part.
            return item;
        }

        /// <summary>
        /// Equips the specified item to the equipment slot, and swaps out any item that may be
        /// stored in the destination slot.
        /// </summary>
        /// <remarks>
        /// If the equip action fails all items will be returned to their original location and null will be returned.
        /// If the destination slot is empty the item to be equipped will do so normally and be the return result.
        /// If the dest slot has an item and the source slot's item can stack with it, the result will be the dest item.
        /// If the dest slot has an item, then a swap will occur as follows: If the source item comes from an
        /// equipment slot that belongs to the same model as the destination slot, the two items will be moved to
        /// the other's slot. If the source item comes from a different model or the grid of the same model as
        /// the dest slot, the destination slot's item will be moved to its model's grid and the source item will
        /// take it's place in the destination slot. If there is insufficient room in the grid for such an action
        /// or one of the items cannot be equipped to the other slot, the swap action will fail.
        /// <para>
        /// It is up to the caller of this method to determine what equip-events will be triggered when and how.
        /// </para>
        /// </remarks>
        /// <returns>If a swap or socketing occured, then the swapped or socketed item is returned. If not, then the item stored. Null if the equip failed.</returns>
        /// <param name="sourceItem">The item to store</param>
        /// <param name="sourceSlot">The slot that the item-to-be-equipped is coming from. This must not be null or the method will fail.</param>
        /// <param name="destSlot">The desitnation slot to equip the item to.</param>
        public PGISlotItem SwapEquip(PGISlotItem sourceItem, PGISlot sourceSlot, PGISlot destSlot)
        {
            _DirtyId++;
            if (sourceSlot == null || destSlot == null || sourceItem == null) return null;
#if !PGI_LITE
            if (CanSocket(sourceItem, destSlot.Item))
            {
                //THINGS TO CONSIDER:
                // 1) We need to consider socketing stacked items. We should only socket one at a time
                //  so we'll need to split the stack.
                // 2) When we socket an item we need to remove it from the inventory and make it belong
                //  to the item it is being attached to.
                if (Socket(destSlot.Item.GetComponent<Socketed>(), sourceItem.GetComponent<Socketable>()))
                {
                    MarkDirty(destSlot.Item);
                    return sourceItem;
                }
            }
#endif

            //check for stacking next
            if (CanStack(sourceItem, destSlot))
            {
                try
                {
                    bool success;
                    //techincally, this might succeed but we still need to return remainders
                    //so we'll return false to let the handler do what it needs to with the original item/stack.
                    StackStore(sourceItem, destSlot.Item, out success);
                        if (success) return destSlot.Item;
                        
                }
                catch (UnityException e)
                {
                    Debug.LogError(e.Message);
                    return null;
                }
                //return null;
            }

            //check to see if we can do a simple equip without swapping
            if (destSlot.Item == null) return Equip(sourceItem, destSlot);



            //The very first thing we need to do is
            //remove our items from their positions.
            //We can assume safely that our destSlot is an equipment slot.
            //So we only need to figure out if the source slot is an equipment slot
            //or a grid slot.
            CanPerformAction = false;
            PGISlotItem destItem = destSlot.Item;
            if (sourceItem.IsEquipped)
            {
                if (sourceSlot.Model.Unequip(sourceItem) != null)
                {
                    sourceItem.TriggerUnequipEvents(sourceSlot.Model, sourceSlot);
                }
                else return null;
            }
            else if (sourceItem.IsStored)
            {
                if (sourceSlot.Model.Remove(sourceItem))
                {
                    sourceItem.TriggerRemoveEvents(sourceSlot.Model);
                }
                else return null;
            }
            //unequip dest item...
            if (destSlot.Model.Unequip(destItem) != null)
            {
                //equip moving item to dest location...
                if (destSlot.Model.Equip(sourceItem, destSlot))
                {
                    //is the dragged item coming from the grid or another equipment slot?...
                    if (sourceSlot.IsEquipmentSlot && sourceSlot.Model == destSlot.Model)
                    {
                        //From another equipment slot. 
                        //Equip the swapped item to the equipment slot the dragged item came from
                        if (sourceSlot.Model.Equip(destItem, sourceSlot) != null)
                        {
                            MarkDirty(destItem);
                            //MarkDirty(sourceItem);
                            return destItem;
                        }
                    }
                    else
                    {
                        //From the grid.
                        //Find a free space and send the swapped out item to the grid
                        //at that location. Notice! We are sending to the grid of the same
                        //model as the dest slot here.
                        Pos temp = FindFirstFreeSpace(destItem);
                        if (temp != null)
                        {
                            if (destSlot.Model.Store(destItem, temp.X, temp.Y))
                            {
                                MarkDirty(destItem);
                                //MarkDirty(sourceItem);
                                return destItem;
                            }

                        }

                    }
                    //Something could not be stored or equipped... back out.
                    destSlot.Model.Unequip(sourceItem, false);
                }
                //Rewind.
                if (destSlot.IsEquipmentSlot) destSlot.Model.Equip(destItem, destSlot, false);
                else destSlot.Model.Store(destItem, destSlot.xPos, destSlot.yPos, false);
            }

            //TODO: Test and see if these two lines are even needed here.
            //make sure we are sending the item being moved it back to the correct inventory too!
            if (sourceSlot.IsEquipmentSlot) sourceSlot.Model.Equip(sourceItem, sourceSlot, false);
            else sourceSlot.Model.Store(sourceItem, sourceSlot.xPos, sourceSlot.yPos, false);

            return null;
        }

        /// <summary>
        /// Locates the first free location in the grid that provides enough space to store the given item 
        /// s a <see cref="PGIModel.Pos"/>. If no space exists then null is returned.
        /// </summary>
        /// <remarks>
        /// This method also invokes the 'Can...' methods of the item and model when checking for availablility.
        /// </remarks>
        /// <returns>A <see cref="PGIModel.Pos"/> with the coordinates of first free grid space or null.</returns>
        /// <param name="item">The item whose</param>
        public Pos FindFirstFreeSpace(PGISlotItem item)
        {
            for (int y = 0; y < this.GridCellsY; y++)
            {
                for (int x = 0; x < this.GridCellsX; x++)
                {
                    if (CanStore(item, x, y)) return new Pos(x, y);
                }
            }
            return null;
        }

#if !PGI_LITE
        /// <summary>
        /// Saves this model by serializing its entire GameObject hierarchy and storing it in an XmlDocument.
        /// </summary>
        /// <remarks>
        /// In order for the model's inventory to be properly saved with it, all items must be children of this model at some level.
        /// </remarks>
        /// <param name="model">The model whose GameObject hierarchy will be serialized.</param>
        /// <param name="version">A version number that can be used to track updates to the serialization system or the model. When deserializing, the version number must be lower or equal to this value.</param>
        /// <returns>An XmlDocument containing a serialized format of the GameObject hierarchy that this model was attached to.</returns>
        public XmlDocument SaveModel(int version)
        {
            return XmlSerializer.Serialize(this.gameObject, version);
        }
#endif

        /// <summary>
        /// Performs multiple calls to <see cref="QueryAndRemove"/> and returns a list of all
        /// found items.
        /// <seealso cref="QueryList(Query)"/>
        /// </summary>
        /// <param name="usePooling">If true, items that are instantiated from stacks will use the built-in auto-pool system.</param>
        /// <param name="queries">A list of query structures that contains information on what type of item to look for and how it should be found.</param>
        public List<List<PGISlotItem>> QueryAndRemoveAll(bool usePooling, params Query[] queries)
        {
            var list = new List<List<PGISlotItem>>(5);

            for(int i = 0; i < queries.Length; i++)
            {
                var subList = QueryAndRemove(usePooling, queries[i]);
                if (subList != null && subList.Count > 0) list.Add(subList);
            }

            return list;
        }

        /// <summary>
        /// Queries the model for the specified item type and amount.
        /// The desired number of items will be gathered from the model
        /// using the model specified. If the model does not contain enough of the
        /// desired item the list may have few items than was speified by the query.
        /// 
        /// All items found will be removed from this model.
        /// 
        /// This method will never return a stack but in fact, will split all stacks into individual items.
        /// Setting <see cref="usePooling"/> to true can, in some cases, minimize allocations.
        /// </summary>
        /// <param name="usePooling">If true, items that are instantiated from stacks will use the built-in auto-pool system.</param>
        /// <param name="query">A query structure that contains information on what type of item to look for and how it should be found.</param>
        /// <returns></returns>
        public List<PGISlotItem> QueryAndRemove(bool usePooling, Query query)
        {
            //create a new list of all items in inventory and sort by stack size
            List<PGISlotItem> items = null;
            if (query.Source == GatherSource.GridAndEquipment) items = AllItems;
            else if (query.Source == GatherSource.GridOnly) items = GridItems;
            else items = EquipmentItems;
            if (query.Mode == GatherMode.CompleteStacksFirst) items.OrderByDescending(x => x.StackCount);
            else items.OrderBy(x => x.StackCount);
            var stacks = items.Where(x => x.ItemTypeId == query.ItemTypeId);

            //gather the required number of items or the max available, whichever is smaller
            int countLeft = query.Count;
            //var itemsToAdd = new List<PGISlotItem>(25);
            var finalList = new List<PGISlotItem>(25);
            foreach(var item in stacks)
            {
                //we reuse the 'items' list in a different context here. This is to reduce allocations a little
                items.Clear();
                if (item.StackCount <= 1) items.Add(item.Model.Remove(item, false));
                else items.AddRange(PGIModel.DecomposeStack(item, Mathf.Min(item.StackCount, countLeft), usePooling));

                if(items.Count > 0)
                {
                    finalList.AddRange(items);
                    countLeft -= items.Count;
                }

                if (countLeft == 0) break;
            }

            //remove all queried items from this model
            for (int i = 0; i < finalList.Count; i++)
            {
                if (finalList[i].Model != null) Remove(finalList[i], false);
            }

            //cleanup the remaining stacks in the model and return our resulting list
            //RestackItems();
            return finalList;
        }
        
        /// <summary>
        /// Internal helper that restacks items without re-allocating a new list.
        /// </summary>
        /// <param name="items"></param>
        protected void RestackItems(List<PGISlotItem> items)
        {
            //we're going to do this the inefficient and lazy way.
            //We'll switch on auto stacking and then remove/restore each stack
            //in the model to ensure we have full stacks and only one partial if
            //there are any remainders.

            bool auto = AutoStack;
            AutoStack = true;
            PGISlotItem stack;
            Pos p = new Pos(0, 0);

            for (int i = 0; i < items.Count; i++)
            {
                stack = items[i];
                if(stack.StackCount < stack.MaxStack)
                {
                    p.Change(stack.xInvPos, stack.yInvPos);
                    Remove(stack, false);
                    Store(stack, p.X, p.Y, false);
                }
            }

            AutoStack = auto;
        }

        /// <summary>
        /// Goes through all stacked items and makes sure that all stacks are completed before creating a partial.
        /// </summary>
        public void RestackItems()
        {
            RestackItems(AllItems);
        }
#endregion


#region Static Methods
#if !PGI_LITE
        /// <summary>
        /// Loads a previously saved PGIModel hierarchy from the supplied xml.
        /// </summary>
        /// <param name="xmlText">The xml text to deserialize and load the model from.</param>
        /// <param name="maxSupportedVersion">The version number the model was serialized with. If this number is lower than the one in the xml stream, it will fail.</param>
        /// <param name="modelToReplace">A reference to the model that should be replaced with the newly deserialized one. The GameObject this component is attached to will be destroyed and the reference will be redirected to the newly deserialized model. This reference must not be null.</param>
        public static void LoadModel(string xmlText, int maxSupportedVersion, ref PGIModel modelToReplace)
        {
            GameObject go = XmlDeserializer.Deserialize(xmlText, maxSupportedVersion) as GameObject;
            if (go != null)
            {
                var newModel = go.GetComponent<PGIModel>();
                if (newModel != null && modelToReplace != null)
                {
                    var parent = modelToReplace.gameObject.transform.parent;
                    GameObject.Destroy(modelToReplace.gameObject);
                    modelToReplace.OnModelChanged.Invoke(newModel);
                    modelToReplace = newModel;
                    newModel.transform.SetParent(parent, false);
                    newModel.MarkAllDirty();
                }
            }
        }
#endif

        /// <summary>
        /// Splits a stack into two stacks. The newly created stack will be of the desired size while the
        /// source stack will be decreased by the appropriate amount. The newly created stack will also be
        /// removed from any source model whereas the source will remain where it is.
        /// </summary>
        /// <param name="stack"></param>
        /// <param name="newStackCount"></param>
        /// <param name="usePooling"></param>
        /// <returns></returns>
        public static PGISlotItem SplitStack(PGISlotItem stack, int newStackCount, bool usePooling = false)
        {
            Assert.IsTrue(stack.MaxStack > 1, "Cannot split an item that can't be stacked.");
            Assert.IsTrue(newStackCount > 0, "New stack size is invalid. It must be greater than zero.");
            Assert.IsTrue(newStackCount < stack.StackCount, "Desired count is invalid. It must be less than current stack size.");
            Assert.IsNotNull(stack);

            GameObject obj = null;
            if (usePooling) obj = Lazarus.Summon(stack.gameObject);
            else obj = Instantiate(stack.gameObject);

            var newStack = obj.GetComponent<PGISlotItem>();
            newStack.StackCount = newStackCount;
            stack.StackCount -= newStackCount;

            if (newStack.Model != null) newStack.Model.Remove(newStack, false);
            return newStack;
        }

        /// <summary>
        /// Splits a stack by cloning a number of live items. Each item will be a single object with a stack size of one.
        /// The original stack, if one is left, will be reduced by the number of items cloned in this way. All new items
        /// will be removed from the model they originated from. If the stack become empty it will be destroyed.
        /// 
        /// Note: <see cref="usePooling"/> can be used to potentially mitigate the number
        /// of allocations this method must perform.
        /// 
        /// </summary>
        /// <param name="stack"></param>
        /// <param name="count"></param>
        /// <param name="usePooling"></param>
        /// <returns>A list of the items removed from the source stack.</returns>
        public static List<PGISlotItem> DecomposeStack(PGISlotItem stack, int count, bool usePooling = false)
        {
            Assert.IsTrue(stack.MaxStack > 1, "Cannot decompose an item that can't be stacked.");
            Assert.IsTrue(count > 0, "Item count is invalid. It must be greater than zero.");
            Assert.IsTrue(count <= stack.StackCount, "Desired count is invalid. It must be less-than or equal-to current stack size.");
            Assert.IsNotNull(stack);

            var list = new List<PGISlotItem>(25);

            PGISlotItem newItem = null;
            GameObject obj = null;
            for (int i = 0; i < count; i++)
            {
                if (usePooling) obj = Lazarus.Summon(stack.gameObject);
                else obj = Instantiate(stack.gameObject);
                newItem = obj.GetComponent<PGISlotItem>();
                newItem.StackCount = 1;
                stack.StackCount--;
                if (newItem.Model != null) newItem.Model.Remove(newItem, false);
            }

            if (stack.StackCount < 1)
            {
                if (usePooling) Lazarus.Summon(stack.gameObject);
#if UNITY_EDITOR
                else
                {
                    if (Application.isPlaying) Destroy(stack.gameObject);
                    else DestroyImmediate(stack.gameObject);
                }
#else
                else Destroy(stack.gameObject);
#endif
            }

            return list;
        }
#endregion
    }
}