/**********************************************
* Power Grid Inventory
* Copyright 2015-2017 James Clark
**********************************************/
#define PGI_LITE
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using PowerGridInventory.Utility;
using UnityEngine.Events;
using System;
using Toolbox;
using Toolbox.Common;
using Toolbox.Graphics;

namespace PowerGridInventory
{
    /// <summary>
    /// Provies the corresponding UI view for a PGIModel.
    /// This particulatr view allows pointer manipulation with 
    /// click-and-hold Drag n' Drop funcitonality.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Power Grid Inventory/View", 12)]
    [RequireComponent(typeof(RectTransform))]
    [ExecuteInEditMode]
    public class PGIView : MonoBehaviour
    {
        #region Fields and Properties
        [SerializeField]
        private PGIModel _Model;
        /// <summary>
        /// The <see cref="PGIModel"/> whose data is displayed and manipulated by this view.
        /// </summary>
        public PGIModel Model
        {
            get { return _Model; }
            set
            {
                ClearList.Remove(this);
                var newModel = value;
                if (_Model != newModel)
                {
                    //out with the old
                    if (_Model != null)
                    {
                        _Model.OnEndGridResize.RemoveListener(UpdateView);
                        _Model.OnModelChanged.RemoveListener(OnChangeModel);
                        _Model.OnUpdateDirty -= this.UpdateView;
                    }

                    //in with the new
                    _Model = newModel;
                    if (_Model != null)
                    {
                        _Model.OnEndGridResize.AddListener(UpdateView);
                        _Model.OnModelChanged.AddListener(OnChangeModel);
                        _Model.OnUpdateDirty += this.UpdateView;
                        SetupEquipment();
                    }

                    UpdateView();
                }
                
            }
        }

        [Tooltip("The PGISlot prefab GameObject that is cloned for use in the view's grid.")]
        public PGISlot SlotPrefab;

        [SerializeField]
        private bool _DisableRendering;
        /// <summary>
        /// Used to disable all UI elements within the view's grid.
        /// </summary>
        public bool DisableRendering
        {
            get { return _DisableRendering; }
            set
            {
                if (_DisableRendering != value)
                {
                    if (Model != null)
                    {
                        UIUtility.SetUIElementsState(gameObject, true, !value);

                        //handle all equipment slots too
                        PGISlot[] slots = Model.Equipment;
                        PGISlot slot;
                        if (slots != null)
                        {
                            for (int i = 0; i < slots.Length; i++)
                            {
                                slot = slots[i];
                                UIUtility.SetUIElementsState(slot.gameObject, true, !value);
                                slot.Dirty = true;
                            }
                        }
                        UpdateView();//we do this so that icons get restored to proper state by the item re-assignment
                    }
                    _DisableRendering = value;
                }
            }
        }


        /// <summary>
        /// Disables the ability to begin dragging items from this PGIView. Hover and click functionalities are not affected by this.
        /// </summary>
        [Header("Drag & Drop Toggles")]
        [Tooltip("Disables the ability to begin dragging items from this PGIView. Hover and click functionalities are not affected by this.")]
        public bool DisableDragging;

        /// <summary>
        /// Disables the ability to drop items from this inventory or others into this PGIView's associated model.
        /// </summary>
        [Tooltip("Disables the ability to drop items from this inventory or others into this PGIView's associated model.")]
        public bool DisableDropping;

        /// <summary>
        /// Disables the ability to remove items from the model by dragging them outside of the view's confines.
        /// </summary>
        [Tooltip("Disables the ability to remove items from the model by dragging them outside of the view's confines.")]
        public bool DisableWorldDropping;

#if PGI_SLOT_BATCHING_AID
        /// <summary>
        /// This allows the view to re-arrange the child objects for all of this view's grid Slots in order to aid uGUI's batching process.
        /// Keeping this on will usually decrease draw calls and increase performance but may introduce rendering artifacts depending
        /// on the setup of your slot prefab.
        /// </summary>
        [Header("Grid Behaviour")]
        [Tooltip("This allows the view to re-arrange the child objects for all of this view's grid Slots in order to aid uGUI's batching process. Keeping this on will usually decrease draw calls and increase performance but may introduce rendering artifacts depending on the setup of your slot prefab.")]
        public bool BatchSlots = false;
#endif

        /// <summary>
        /// The column order items will be inserted into the grid view.
        /// </summary>
        [Tooltip("The column order the grid will be created.")]
        public HorizontalOrdering HorizontalOrder = HorizontalOrdering.LeftToRight;

        /// <summary>
        /// The row order items will be inserted into the grid view.
        /// </summary>
        [Tooltip("The row order the grid will be created.")]
        public VerticalOrdering VerticalOrder = VerticalOrdering.TopToBottom;

        /// <summary>
        /// The color used in the 'highlight' section of grid and equipment slots when no action is being taken.
        /// </summary>
        [Header("Slot Colors")]
        [Tooltip("The color used in the 'highlight' section of grid and equipment slots when no action is being taken.")]
        public Color NormalColor = Color.clear;

        /// <summary>
        /// The color used in the 'highlight' section of grid and equipment slots when a valid action is about to occur.
        /// </summary>
        [Tooltip("The color used in the 'highlight' section of grid and equipment slots when a valid action is about to occur.")]
        public Color HighlightColor = Color.green;

        /// <summary>
        /// The color used in the 'hilight' section of a grid and equipment slots when a valid socket action is about to occur.
        /// </summary>
        [Tooltip("The color used in the 'hilight' section of a grid and equipment slots when a valid socket action is about to occur.")]
        public Color SocketValidColor = Color.green;

        /// <summary>
        /// The color used in the 'highlight' section of grid and equipment slots when an invalid action is being taken.
        /// </summary>
        [Tooltip("The color used in the 'highlight' section of grid and equipment slots when an invalid action is being taken.")]
        public Color InvalidColor = Color.red;

        /// <summary>
        /// The color used in the 'highlight' section of grid and equipment slots when it has been flagged as bocked with the <see cref="PGISlot.Blocked"/> value.
        /// </summary>
        [Tooltip("The color used in the 'highlight' section of grid and equipment slots when it has been flagged as bocked with the PGISlot.Blocked value.")]
        public Color BlockedColor = Color.grey;

        /// <summary>
        /// Returns true if this view is currently in a drag operational state.
        /// </summary>
        public bool IsDragging { get { return (DraggedItem == null); } }

        private List<PGISlot> Slots = new List<PGISlot>(20);

        //cached fields
        private Canvas _UICanvas;
        protected Canvas UICanvas
        {
            get
            {
                if (_UICanvas == null) _UICanvas = GetComponentInParent<Canvas>();
                if (_UICanvas == null) Debug.LogError("PGIView must be a child of a UI canvas.");
                return _UICanvas;
            }

        }
        private RectTransform ParentRect;
        private float CellScaleX = -1;
        private float CellScaleY = -1;
        public static CachedItem DraggedItem;
        private static List<PGIView> ClearList = new List<PGIView>(5);

        //We need this stored in case we change the model size, so we know if we need
        //to rebuild the whole grid or simply resize it.
        int CachedSlotsX;
        int CachedSlotsY;

        public static DragIcon DragIcon;
        #endregion


        #region Inner Definitions
        public enum HorizontalOrdering
        {
            LeftToRight,
            RightToLeft,
        }
        public enum VerticalOrdering
        {
            TopToBottom,
            BottomToTop,
        }

        /// <summary>
        /// Child UI elements of slots are moved to this array of  GameObjects
        /// to allow UGui to batch them and reduce draw calls.
        /// </summary>
        protected GameObject[] SlotBatches;

        /// <summary>
        /// This is used primarily in the editor to determine if playmode is being exited
        /// so that we can skip potential updates that should happen. They seem to have a
        /// tendency to cause problems if they occurr between the time we try to exit and
        /// the time it actually occurs.
        /// </summary>
        protected static bool ApplicationQuitting = false;


        /// <summary>
        /// Helper class for managing items being moved internally due to drag n' drop actions.
        /// </summary>
        public class CachedItem
        {
            public PGISlotItem Item;
            public int xPos, yPos, Width, Height, EquipIndex;
            public PGISlot Slot;
            public PGIModel Model;
            public PGIView View;
            public bool WasEquipped { get { return (EquipIndex >= 0); } }
            public bool WasStored { get { return (xPos >= 0 && yPos >= 0); } }

            public CachedItem(PGISlotItem item, PGISlot slot, PGIModel model, PGIView view)
            {
                Item = item;
                xPos = item.xInvPos;
                yPos = item.yInvPos;
                Width = item.CellWidth;
                Height = item.CellHeight;
                EquipIndex = item.Equipped;
                Slot = slot;
                Model = model;
                View = view;
            }
        }
        #endregion


        #region PGI Events

        [Serializable]
        public class InvalidDropDestTrigger : UnityEvent<PointerEventData, PGISlotItem, PGISlot, GameObject> { }

        /// <summary>
        /// Invoked when a previous started drag operation ends on an invalid destination.
        /// Invalid destinations are anything that is not a <see cref="PGISlot">PGISlot</see>.
        /// The PGISlot supplied will be the slot that the item was dragged from and is returning to.
        /// The GameObject is whatever object the drag ended on that was an invalid target.
        /// <seealso cref="PGISlot.OnDragBegin"/> 
        /// <seealso cref="PGISlotItem.OnClick"/> 
        /// </summary>
        [SerializeField]
        [PGIFoldedEvent]
        public InvalidDropDestTrigger OnDragEndInvalid = new InvalidDropDestTrigger();

        /// <summary>
        /// Invoked when the pointer first enters equipment slot or grid location with an item in it.
        /// <seealso cref="PGISlot.OnHover"/> 
        /// </summary>
        [SerializeField]
        [PGIFoldedEvent]
        public PGISlot.SlotTrigger OnHoverSlot = new PGISlot.SlotTrigger();

        /// <summary>
        /// Invoked when the pointer leaves an equipment slot or grid location with an item in it.
        /// <seealso cref="PGISlot.OnEndHover"/> 
        /// </summary>
        [SerializeField]
        [PGIFoldedEvent]
        public PGISlot.SlotTrigger OnEndHoverSlot = new PGISlot.SlotTrigger();

        /// <summary>
        /// Invoked when the pointer is clicked on an equipment slot or grid location with an item in it.
        /// <seealso cref="PGISlot.OnClick"/> 
        /// <seealso cref="PGISlotItem.OnClick"/> 
        /// </summary>
        [SerializeField]
        [PGIFoldedEvent]
        public PGISlot.SlotTrigger OnClickSlot = new PGISlot.SlotTrigger();

        /// <summary>
        /// Invoked when a drag operation begins on an equipment slot or grid location with an item in it.
        /// <seealso cref="PGISlot.OnDragEnd"/> 
        /// <seealso cref="PGISlotItem.OnClick"/> 
        /// </summary>
        [SerializeField]
        [PGIFoldedEvent]
        public PGISlot.SlotTrigger OnSlotDragBegin = new PGISlot.SlotTrigger();

        /// <summary>
        /// Invoked when a previous started drag operation ends.
        /// <seealso cref="PGISlot.OnDragBegin"/> 
        /// <seealso cref="PGISlotItem.OnClick"/> 
        /// </summary>
        [SerializeField]
        [PGIFoldedEvent]
        public PGISlot.SlotTrigger OnSlotDragEnd = new PGISlot.SlotTrigger();


        [SerializeField]
        [FoldFlag("Events")]
        public bool FoldedEvents = false; //used by the inspector
        #endregion


        #region Unity Events
        static DragIcon CreateDragIcon(Transform dragParent)
        {
            GameObject di = new GameObject("Drag Icon");
            di.AddComponent<RectTransform>();
            DragIcon dragIcon = di.AddComponent<DragIcon>();
            di.transform.SetParent(dragParent);
            di.transform.SetSiblingIndex(dragParent.childCount - 1);
            di.transform.localScale = Vector3.one;

            GameObject icon = new GameObject("Icon");
            icon.transform.SetParent(di.transform);
            var rect = icon.AddComponent<RectTransform>();
            icon.AddComponent<CanvasRenderer>();
            icon.AddComponent<Image>().preserveAspect = true;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;

            GameObject icon3d = new GameObject("Icon3D");
            icon3d.transform.SetParent(di.transform);
            rect = icon3d.AddComponent<RectTransform>();
            icon3d.AddComponent<CanvasRenderer>();
            icon3d.AddComponent<Image3D>().PreserveAspect = true;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;

            dragIcon.Icon = icon.GetComponent<Image>();
            dragIcon.Icon3D = icon3d.GetComponent<Image3D>();
            dragIcon.gameObject.SetActive(false);
            return dragIcon;

        }

        void Awake()
        {
            //dragicon will be set to HideAndDontSave
            if (DragIcon == null && Application.isPlaying) DragIcon = CreateDragIcon(UICanvas.transform);

            //this will cause the view to completely refresh
            var old = _Model;
            Model = null;
            Model = old;
            if (Model != null) Model.MarkAllDirty();
        }

        void OnDestroy()
        {
            if(!ApplicationQuitting) RelenquishAllSlots();
        }

        void OnEnable()
        {
            //doing this ensures our view is completely refreshed
            if (Model != null) Model.MarkAllDirty();
            UpdateView();
#if UNITY_EDITOR
            if (!Application.isPlaying) ApplicationQuitting = false;
#endif
        }

        void OnDisable()
        {
            //We add a small delay so that if a triggered event attached to the view was the one that disabled it, they have time to
            //handle everything before we actually disable the view. This is particularly important to the
            //InventoryItem class. When the item is dropped, the nested inventory view should close at that point,
            //but doing so causes the item to be returned to its own inventory due to the shared nature of the DraggedItem.
            //This little delay helps avoid that scenario.
            if (Application.isPlaying) Invoke("DisableView", 0.1f);
            else DisableView();
        }

        private void OnApplicationQuit()
        {
            ApplicationQuitting = true;
        }
        #endregion


        #region Private Methods
        void DisableView()
        {
#if UNITY_EDITOR
            if (ApplicationQuitting) return;
#endif

            //Usually happens when we drag an empty slot
            if (DraggedItem == null)
            {
                OnSlotDragEnd.Invoke(null, null);
            }
            else
            {
                OnSlotDragEnd.Invoke(null, DraggedItem.Slot);
                DraggedItem.Model.ResetSwapCache();
            }

            ReturnDraggedItemToSlot();
            if (Model != null) Model.ResetSwapCache();
            DeselectAllViews();
            DraggedItem = null;
            if (DragIcon != null)
            {
                DragIcon.gameObject.SetActive(false);
                ResetDragIcon(DragIcon);
            }

        }


        #if UNITY_EDITOR
        int FrameSkip = 0;
        /// <summary>
        /// Used to render frequent changes in-editor.
        /// </summary>
        protected void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                if (FrameSkip > 24 || (ParentRect != null && ParentRect.hasChanged))
                {
                    UpdateView();
                    FrameSkip = 0;
                }
                else FrameSkip++;
            }
        }
        #endif

        float lastUpdateTime;
        static float UpdateFreq = 2;
        /// <summary>
        /// Used to render frequent changes in playmode.
        /// </summary>
        void Update()
        {
            #if UNITY_EDITOR
            if (!Application.isPlaying) return;
            #endif

            var t = Time.realtimeSinceStartup;
            if(t - lastUpdateTime > PGIView.UpdateFreq || (ParentRect != null && ParentRect.hasChanged))
            {
                lastUpdateTime = t;
                UpdateView();
            }
        }

        /// <summary>
        /// Completely refreshes the view's grid and all equipment slots of the view's model.
        /// Internally, the grid and all slots may be re-sized and have their highlighting
        /// and icons set apporpriately according to slot contents.
        /// </summary>
        public void UpdateView()
        {
#if UNITY_EDITOR
            if (ApplicationQuitting) return;
#endif

            //check for early-outs
            //NOTE: If we try to update view while dragging an item, bad things will happen to the render state
            if (_DisableRendering) return;
            if (SlotPrefab == null)
            {
                //TODO: disable rendering here if prefab becomes null
                RelenquishAllSlots();
                //Debug.LogWarning("Can't update a view that doesn't have a slot prefab assigned.");
                return;
            }
            if (Model == null || CachedSlotsX != Model.GridCellsX || CachedSlotsY != Model.GridCellsY || ParentRect == null)
                CreateGrid();
            else ResizeGrid();
            SyncViewToModel();
        }

        /// <summary>
        /// Helper method to resize all slots to match the new size of the RectTransform.
        /// </summary>
        void ResizeGrid()
        {
            if (Model == null || Slots == null || Slots.Count < 0 || ParentRect == null || !ParentRect.hasChanged) return;
            //resize slots
            CellScaleX = ParentRect.rect.width / Model.GridCellsX;
            CellScaleY = ParentRect.rect.height / Model.GridCellsY;

            for (int y = 0; y < Model.GridCellsY; y++)
            {
                for (int x = 0; x < Model.GridCellsX; x++)
                {
                    //initialize slot
                    PGISlot slot = GetSlotCell(x, y);
                    if (slot == null)
                    {
                        //The slot was deleted but we are still referencing it. Uh-oh!
                        break;
                    }
                    //PGISlot slot = Slots[(y * Model.GridCellsX) + x];
                    GameObject slotGO = slot.gameObject;
                    slotGO.transform.position = Vector3.zero;
                    slotGO.transform.SetParent(this.transform, false);
                    slotGO.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

                    //position and size
                    RectTransform childRect = slotGO.transform as RectTransform;
                    var pos = CalculateCellPos(x, y);
                    //var rectTrans = slot.transform as RectTransform;
                    //rectTrans.anchoredPosition3D = new Vector3(rectTrans.anchoredPosition3D.x, rectTrans.anchoredPosition3D.y, 0.0f);
                    childRect.anchoredPosition = pos;
                    childRect.anchoredPosition3D = new Vector3(pos.x, pos.y, 0.0f); //CalculateCellPos(x, y);
                    childRect.sizeDelta = new Vector2(CellScaleX, CellScaleY);
                    slot.Dirty = true;
                }
            }
        }

        PGISlot MakeSlot(int x, int y)
        {
            //initialize slot
            PGISlot slot = Lazarus.Summon(SlotPrefab.gameObject).GetComponent<PGISlot>();
            #if UNITY_EDITOR
            if (Application.isPlaying) slot.gameObject.name = "Slot " + x + "," + y;
            else slot.gameObject.name = "Edit-time Slot" + x + "," + y;
#else
            slot.gameObject.name = "Slot " + x + "," + y;
#endif

            slot.View = this;
            slot.xPos = x;
            slot.yPos = y;
            slot.HighlightColor = NormalColor;

            //setup events
            slot.OnBeginDragEvent.RemoveAllListeners();
            slot.OnEndDragEvent.RemoveAllListeners();
            slot.OnDragEvent.RemoveAllListeners();
            slot.OnHover.RemoveAllListeners();
            slot.OnEndHover.RemoveAllListeners();
            slot.OnClick.RemoveAllListeners();

            slot.OnBeginDragEvent.AddListener(OnDragBegin);
            slot.OnEndDragEvent.AddListener(OnDragEnd);
            slot.OnDragEvent.AddListener(OnDrag);
            slot.OnHover.AddListener(OnHoverEvent);
            slot.OnEndHover.AddListener(OnEndHoverEvent);
            slot.OnClick.AddListener(OnClickSlotHandler);

            slot.Dirty = true;

            //TODO: do we need to reset slot Item here too?

            Slots.Add(slot);
            return slot;
        }

        void RelenquishAllSlots()
        {
            //get a list of all child objects that are slots
            var list = new List<PGISlot>(transform.childCount);
            for (int i = 0; i < transform.childCount; i++)
            {
                var slot = transform.GetChild(i).GetComponent<PGISlot>();
                if (slot != null) list.Add(slot);
            }

            //relenquish/destroy all found slots
            for (int i = 0; i < list.Count; i++)
                Lazarus.RelenquishToPool(list[i].gameObject);


            Slots.Clear();
            CachedSlotsX = -1;
            CachedSlotsY = -1;
        }

        /// <summary>
        /// Creates a grid of <see cref="PGISlot"/>s and sets up
        /// all events and references used by the view and model.
        /// </summary>
        void CreateGrid()
        {
            RelenquishAllSlots();

            //if no model, return
            if (Model == null || SlotPrefab == null) return;

            //resize to adjust for new model
            ParentRect = this.GetComponent<RectTransform>();
            CellScaleX = ParentRect.rect.width / Model.GridCellsX;
            CellScaleY = ParentRect.rect.height / Model.GridCellsY;

            //re-activate all old slots (repurposing them in the process)
            //and then create any additional ones we may need
            for (int y = 0; y < Model.GridCellsY; y++)
            {
                for (int x = 0; x < Model.GridCellsX; x++)
                    MakeSlot(x, y);
            }
            CachedSlotsX = Model.GridCellsX;
            CachedSlotsY = Model.GridCellsY;

            ResizeGrid();

#if PGI_SLOT_BATCHING_AID
            //This is a performance trick. When running in-game, we will
            //take all of the child elements of our slots and parent them
            //under a single gameobject held by this view. In this way we
            //can allow UGui batch more UI elements and reduce draw calls.
            if (Application.isPlaying && BatchSlots)
                PerformSlotBatching(ref SlotBatches, Slots.ToArray(), this.transform, UICanvas, true);
#endif

        }

        /// <summary>
        /// Sets up all references and triggered events for this view's model's Equipment slots.
        /// </summary>
        void SetupEquipment()
        {
            if (Model == null) return;
            if (Model.Equipment != null)
            {
                for (int i = 0; i < Model.Equipment.Length; i++)
                {
                    var slot = Model.Equipment[i];
                    if (slot != null)
                    {
                        if (slot.View != null)
                        {
                            //remove old listeners if they existed
                            slot.OnBeginDragEvent.RemoveAllListeners();
                            slot.OnEndDragEvent.RemoveAllListeners();
                            slot.OnDragEvent.RemoveAllListeners();
                            slot.OnHover.RemoveAllListeners();
                            slot.OnEndHover.RemoveAllListeners();
                        }

                        slot.OnBeginDragEvent.AddListener(OnDragBegin);
                        slot.OnEndDragEvent.AddListener(OnDragEnd);
                        slot.OnDragEvent.AddListener(OnDrag);
                        slot.OnHover.AddListener(OnHoverEvent);
                        slot.OnEndHover.AddListener(OnEndHoverEvent);
                        slot.OnClick.AddListener(OnClickSlotHandler);

                        slot.HighlightColor = NormalColor;
                        slot.View = this;
                    }
                }
            }

#if PGI_SLOT_BATCHING_AID
            if (Application.isPlaying && BatchSlots)
            {
                PerformSlotBatching(ref SlotBatches,
                    Model.Equipment,
                    this.transform,
                    UICanvas,
                    (Model.GridCellsX > 0 && Model.GridCellsY > 0) ? false : true); //pass false if we have a grid since already re-created them then
            }
#endif
        }

        /// <summary>
        /// Used when creating a grid to help arrange child object of slots
        /// to be more easily batched by UGui's renderer.
        /// </summary>
        /// <returns></returns>
        /// <param name="batches">A reference to an array of GameObject that represent each batch.</param>
        /// <param name="slots">An array of PGISlots that will be batched.</param>
        static void PerformSlotBatching(ref GameObject[] batches, PGISlot[] slots, Transform batchParent, Canvas canvas, bool reCreateBatches)
        {
            if (slots == null || batchParent == null) return;

            //setup the gameobjects that will hold each sub-GameObject of our slots.
            //In order for batching to work well we should have one GameObject batch
            //for each child of the slots. We'll sample the first slot of the grid (if any)
            //to see how many batches we'll need.
            if (reCreateBatches)
            {
                if (batches != null && batches.Length > 0)
                {
                    foreach (var go in batches) GameObject.Destroy(go);
                }
                if (slots != null && slots.Length > 0 && slots[0].transform.childCount > 0)
                {
                    batches = new GameObject[slots[0].transform.childCount];
                    for (int i = 0; i < batches.Length; i++)
                    {
                        //we can help UGui's batching by putting non-overlapping
                        //elements together in a single GameObject. Each GO will
                        //represent a batch.
                        batches[i] = new GameObject(slots[0].transform.GetChild(i).name + " Batch");
                        batches[i].AddComponent<RectTransform>();
                        batches[i].transform.SetParent(batchParent, false);

                    }
                }
            }

            //move all of the grid slots' children to the batching objects.
            SlotBatch child = null;
            List<SlotBatch> temp = new List<SlotBatch>(5);
            foreach (PGISlot slot in slots)
            {
                //collect a list of all batchable GameObjects under this slot.
                temp.Clear();
                for (int i = 0; i < slot.transform.childCount; i++)
                {
                    child = slot.transform.GetChild(i).GetComponent<SlotBatch>();
                    if (child != null) temp.Add(child);
                }

                //move the batchable objects
                for (int i = 0; i < temp.Count; i++)
                {
                    temp[i].transform.SetParent(batches[i].transform, true);
                }

            }
        }

        /// <summary>
        /// Handles the BeginDrag event trigger from a slot. This method
        /// will cache the item being manipulated before removing it from
        /// its storage location.
        /// </summary>
        /// <param name="eventData">Event data.</param>
        void OnDragBegin(PointerEventData eventData)
        {
            if (DisableDragging) return;
            //Usually happens when we drag an empty slot
            if (DraggedItem != null) return;

            //get the contents of the slot we started dragging,
            //cache it, and then remove it from the grid slot.
            PGISlot slot = eventData.pointerDrag.GetComponent<PGISlot>();
            DraggedItem = new CachedItem(slot.Item, slot, slot.Model, slot.View);
            if (slot.IsEquipmentSlot)
            {
                if (slot.Model.Unequip(DraggedItem.Item) != null)
                {
                    //send unequip events to the item and slot when we begin dragging
                    DraggedItem.Item.TriggerUnequipEvents(slot.Model, slot);
                }
                else
                {
                    throw new UnityException("There was an error while attempting to unequip item.");
                }
            }
            else
            {
                if (slot.Model.Remove(DraggedItem.Item) == null) throw new UnityException("There was an error while attempting to remove the item from the inventory grid.");
            }

            DraggedItem.Item.OnDragBegin.Invoke(DraggedItem.Item, slot.Model, slot);

            //display the icon that follows the mouse cursor
            SetDragIcon(DragIcon, DraggedItem, slot);
            //OnSlotDragBegin.Invoke(eventData, slot);
            OnSlotDragBegin.Invoke(eventData, DraggedItem.Slot);
        }

        /// <summary>
        /// Handles the EndDrag event trigger from the slot or location that began the drag.
        /// This method resets the dragging state and removes the previously
        /// cached results. It also returns the item to its original location
        /// or removes the item from the inventory depending on any previously
        /// fired Drop triggers.
        /// </summary>
        /// <param name="eventData">Event data.</param>
        void OnDragEnd(PointerEventData eventData)
        {
            //make sure this view's model's cache is reset
            Model.ResetSwapCache();

            //Usually happens when we drag an empty slot
            if (DraggedItem == null)
            {
                OnSlotDragEnd.Invoke(eventData, null);
                return;
            }
            var sourceSlot = DraggedItem.Slot;
            var item = DraggedItem.Item;

            //OnSlotDragEnd.Invoke(eventData, DraggedItem.Slot);

            //make sure the item's model's cache is reset
            DraggedItem.Model.ResetSwapCache();


            GameObject enteredGO = eventData.pointerEnter;
            PGISlot dropSlot = null;
            if (enteredGO == null)
            {
                //Was dropped in empty space (no UI elements at all)

                if (DraggedItem.View.DisableWorldDropping)
                {
                    //not allowed by view, return item to source slot
                    ReturnDraggedItemToSlot();
                    OnDragEndInvalid.Invoke(eventData, item, sourceSlot, enteredGO);
                    DeselectAllViews();
                    DraggedItem = null;
                    DragIcon.SetIconActive(DragIcon.ActiveIcon.None);
                    ResetDragIcon(DragIcon);
                    DraggedItem.Item.OnDragEnd.Invoke(item, sourceSlot.Model, sourceSlot);

                    return;
                }
                else
                {
                    //This is where we drop items from inventories entirely.
                    //The location that was chosen to end the drag was
                    //completely empty (including UI elements). So we
                    //will remove item from the inventory completely.

                    //make sure we return this item to normal orientation when removing it
                    DraggedItem.Item.Rotate(PGISlotItem.RotateDirection.None);

                    //trigger removal and unequip events
                    //NOTE: Unequip happens when drag starts now, so we don't need to trigger it here.
                    //if(DraggedItem.WasEquipped) DraggedItem.Item.OnUnequipped(DraggedItem.Model, DraggedItem.Slot);
                    DraggedItem.Item.TriggerRemoveEvents(DraggedItem.Model);

                    DraggedItem = null;
                    DragIcon.gameObject.SetActive(false);

                    DeselectAllViews();
                    ResetDragIcon(DragIcon);
                    return;
                }
            }
            else dropSlot = enteredGO.GetComponent<PGISlot>();


            if (dropSlot == null || dropSlot.View == null || dropSlot.View.DisableDropping)
            {
                //Dropped on UI element
                ReturnDraggedItemToSlot();
                OnDragEndInvalid.Invoke(eventData, item, sourceSlot, enteredGO);
                DraggedItem.Item.OnDragEnd.Invoke(item, sourceSlot.Model, sourceSlot);
            }
            else
            {
                //dropped onto a valid slot

                //make sure we have a valid grid size for our item
                if (!dropSlot.IsEquipmentSlot &&
                    (DraggedItem.Item.CellHeight > dropSlot.Model.GridCellsY || DraggedItem.Item.CellWidth > dropSlot.Model.GridCellsX))
                {
                    ReturnDraggedItemToSlot();
                    OnDragEndInvalid.Invoke(eventData, item, sourceSlot, enteredGO);
                    DraggedItem.Item.OnDragEnd.Invoke(item, sourceSlot.Model, sourceSlot);
                }
                else
                {
                    //Here is where we perform our drop action
                    dropSlot = dropSlot.View.GetOffsetSlot(DraggedItem.Item, dropSlot);
                    //If the offset is the same AND the view is the same,
                    //then we simply return the item from whence it came.
                    if (dropSlot != DraggedItem.Slot)
                    {
                        //we need to manually trigger a drop and relocate the item.
                        //The question is: do we store it, or do we equip it?
                        if (!dropSlot.View.AssignItemToSlot(DraggedItem.Item,
                                                          dropSlot,
                                                          DraggedItem.WasEquipped,
                                                          DraggedItem.WasStored,
                                                          DraggedItem.Slot))
                        {
                            //failed to assign item, revert
                            ReturnDraggedItemToSlot();
                            OnDragEndInvalid.Invoke(eventData, item, sourceSlot, enteredGO);
                            DraggedItem.Item.OnDragEnd.Invoke(item, sourceSlot.Model, sourceSlot);
                        }
                        else
                        {
                            //success!
                            OnSlotDragEnd.Invoke(eventData, DraggedItem.Slot);
                            DraggedItem.Item.OnDragEnd.Invoke(item, dropSlot.Model, dropSlot);
                        }
                    }
                    else
                    {
                        //source and dest slots are same, revert
                        ReturnDraggedItemToSlot();
                        OnDragEndInvalid.Invoke(eventData, item, sourceSlot, enteredGO);
                        DraggedItem.Item.OnDragEnd.Invoke(item, sourceSlot.Model, sourceSlot);
                    }
                }

            }

            DeselectAllViews();
            DraggedItem = null;
            ResetDragIcon(DragIcon);
        }

        /// <summary>
        /// Handles the updating drag event. Provides highlighting and cell
        /// offsetting (to ensure the item is placed in a reasonable way on the grid).
        /// </summary>
        /// <param name="eventData">Event data.</param>
        void OnDrag(PointerEventData eventData)
        {
            //this can happen if we attempt to drag and empty slot
            if (DraggedItem == null) return;
            AppendClearList(this);


            //figure out highlighting and cell offsets.
            PGISlot dropSlot = null;
            if (eventData.pointerEnter != null)
            {
                dropSlot = eventData.pointerEnter.GetComponent<PGISlot>();

                //clear all grids involved
                //kinda slow but I'm lazy right now
                DeselectAllViews();

                if (dropSlot != null)
                {
                    //Make sure the view is added to the dirty highlighting list, then highlight the dragged item
                    AppendClearList(dropSlot.View);
                    if (dropSlot.View != null) dropSlot.View.SelectSlot(dropSlot, DraggedItem.Item);
                }
            }

            DragIcon.transform.position = PGICanvasMouseFollower.GetPointerPosOnCanvas(UICanvas, eventData.position);
        }

        /// <summary>
        /// Helper method used to initialize the DragIcon when it becomes visible.
        /// </summary>
        void SetDragIcon(DragIcon icon, CachedItem draggedItem, PGISlot slot)
        {
            switch (draggedItem.Item.RotatedDir)
            {
                case PGISlotItem.RotateDirection.None:
                    {
                        icon.Icon.transform.eulerAngles = Vector3.zero;
                        icon.Icon3D.transform.eulerAngles = Vector3.zero;
                        break;
                    }
                case PGISlotItem.RotateDirection.CW:
                    {
                        icon.Icon.transform.eulerAngles = new Vector3(0.0f, 0.0f, 270.0f);
                        icon.Icon3D.transform.eulerAngles = new Vector3(0.0f, 0.0f, 270.0f);
                        break;
                    }
                case PGISlotItem.RotateDirection.CCW:
                    {
                        icon.Icon.transform.eulerAngles = new Vector3(0.0f, 0.0f, 90.0f);
                        icon.Icon3D.transform.eulerAngles = new Vector3(0.0f, 0.0f, 90.0f);
                        break;
                    }
            }

            if (float.IsInfinity(CellScaleX) || float.IsInfinity(CellScaleY) ||
               CellScaleX <= 0.01f || CellScaleY <= 0.01f)
            {
                //the slot sizes are probably too small to see.
                //This is mostly likely due to using an
                //inventory with a grid size of 0,0.
                //Use the slot size in this case.
                icon.GetComponent<RectTransform>().sizeDelta = slot.GetComponent<RectTransform>().sizeDelta * 0.9f;
            }
            else
            {
                //use a size roughly corresponding to the grid display's size
                //NOTE: we need to reverse them if the item has been rotated
                //Apparently the sizeDelta is absolute and does not take rotation into account.
                if(draggedItem.Item.Rotated)
                    icon.GetComponent<RectTransform>().sizeDelta = new Vector2((CellScaleX * 0.9f) * draggedItem.Height, (CellScaleX * 0.9f) * draggedItem.Width);
                else icon.GetComponent<RectTransform>().sizeDelta = new Vector2((CellScaleX * 0.9f) * draggedItem.Width, (CellScaleX * 0.9f) * draggedItem.Height);
            }

            //Display our icon image, either a sprite or a mesh.
            //icon.gameObject.SetActive(true); //activate first or GetComponent will fail
            if (draggedItem.Item.IconType == PGISlotItem.IconAssetType.Sprite)
            {
                icon.SetIconActive(DragIcon.ActiveIcon.Icon2D);
                icon.Icon.sprite = draggedItem.Item.Icon;
                icon.Icon3D.material = null;
                icon.Icon3D.Mesh = null;
                
            }
            else
            {
                icon.SetIconActive(DragIcon.ActiveIcon.Icon3D);
                icon.Icon.sprite = null;
                icon.Icon3D.material = draggedItem.Item.IconMaterial;
                icon.Icon3D.Rotation = draggedItem.Item.IconOrientation;
                icon.Icon3D.Mesh = draggedItem.Item.Icon3D;
                
            }

           
        }

        /// <summary>
        /// Helper method used to reset the drag icon. This should be
        /// called anytime a drag ends for any reason otherwise 3D mesh
        /// icons might not update correctly next time due to cached values
        /// within the CanvasMesh.
        /// </summary>
        void ResetDragIcon(DragIcon icon)
        {
            //We have to do this otherwise it won't update properly next time
            /*
            var mesh = icon.GetComponent<Image3D>();
            mesh.Mesh = null;
            mesh.material = null;
            mesh.enabled = false;
            */
            icon.Icon.transform.eulerAngles = Vector3.zero;
            icon.Icon3D.transform.eulerAngles = Vector3.zero;
            icon.SetIconActive(DragIcon.ActiveIcon.None);
            icon.gameObject.SetActive(false);
        }

        /// <summary>
        /// Helper method for assigning an item an equipment slot.
        /// </summary>
        /// <returns><c>true</c>, if dragged item to slot was assigned, <c>false</c> otherwise.</returns>
        /// <param name="item">The item being assigned.</param>
        /// <param name="dest">The destination slot to assign the item to.</param>
        /// <param name="wasEquipped">Set to <c>true</c> if the item being assigned was previously in an equipment slot.</param>
        /// <param name="wasStored">Set to <c>true</c> if the item being assigned was previously stored in an inventory. Equipped, or in a grid.</param>
        /// <param name="previousSlot">The equipment slot if any that the item was previously equipped to. This can be null unless <c>wasEquipped</c> is <c>true</c></param>
        /// <param name="equipIndex">Equip index.</param>
        bool AssignItemToSlot(PGISlotItem item, PGISlot dest, bool wasEquipped, bool wasStored, PGISlot previousSlot)
        {
            if (dest == null) return false;

            if (dest.IsEquipmentSlot)
            {

                PGISlotItem swappedItem;
                swappedItem = dest.Model.SwapEquip(item, previousSlot, dest);
                if (swappedItem != null)
                {
                    if (swappedItem == item)
                    {
                        //we didn't swap
                        if (previousSlot.Model != this.Model)
                        {
                            item.TriggerRemoveEvents(previousSlot.Model);
                            item.TriggerStoreEvents(dest.Model);
                        }
                        //if(wasEquipped) item.TriggerUnequipEvents(previousSlot.Model, previousSlot);
                        item.TriggerEquipEvents(Model, dest);//TODO: Confirm we don't need to test for equipment here
                        return true;

                    }
                    else
                    {
                        //since we didn't drag the swapped item, we'll need to trigger
                        //its unequip events now. We'll do this before we trigger the
                        //other stuff just so the order of events stays somewhat normal
                        //(as if we dragged the item normally)
                        //As for the other item... it was being dragged so it should
                        //already have had its unequip events triggerd
                        if (wasEquipped) swappedItem.TriggerUnequipEvents(dest.Model, dest);


                        //It's possible we moved this item to another container.
                        //If we did, we'll want to trigger some store/remove triggers.
                        if (item.Model != previousSlot.Model)
                        {
                            item.TriggerRemoveEvents(previousSlot.Model);
                            if (item.Model != null) item.TriggerStoreEvents(dest.Model);
                        }
                        if (swappedItem.Model != dest.Model)
                        {
                            swappedItem.TriggerRemoveEvents(dest.Model);
                            if (swappedItem.Model != null) swappedItem.TriggerStoreEvents(previousSlot.Model);
                            return true;
                        }

                        //this stuff happens when swapping between grid and equipment slot
                        if (swappedItem.IsEquipped)
                            swappedItem.TriggerEquipEvents(swappedItem.Model, previousSlot);
                        else if (!wasEquipped) swappedItem.TriggerUnequipEvents(swappedItem.Model, dest);
                        item.TriggerEquipEvents(item.Model, dest);

                        return true;
                    }

                }
            }
            else
            {
                //we are moving this to a normal grid slot
                if (dest.Model.Store(item, dest.xPos, dest.yPos))
                {
                    //if it wasn't stored before (which by all means it should have
                    //been if we are dragging it) then it will be now.
                    if (previousSlot.Model != this.Model)
                    {
                        item.TriggerRemoveEvents(previousSlot.Model);
                        item.TriggerStoreEvents(dest.Model);
                    }

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Helper method used to return a previously cached and dragged item to the
        /// location it came form when the drag started.
        /// </summary>
        /// <returns><c>true</c>, if dragged item was returned, <c>false</c> otherwise.</returns>
        bool ReturnDraggedItemToSlot()
        {
            if (DraggedItem == null) return false;

            //End the drag like normal. Return it to it's origial position
            //either in the grid on in an equipment slot.

            //NOTE: I am passing false to the 'checkCanMethods' flags here to avoid triggering them
            //on any attached scripts. This can be a little dangerous seeing as it is possible
            //something has changed in the model during the drag - Or users might find
            //this behaviour somewhat confusing. However, this fixes more problems than it solves right now.
            //Namely, the 'LinkedEquipSlots' example script breaks when trying to return an item
            //because its 'CanEquip' method will fail when dragging and dropping into the same slot.
            if (DraggedItem.EquipIndex >= 0)
            {
                DraggedItem.Model.Equip(DraggedItem.Item, DraggedItem.EquipIndex, false);
                DraggedItem.Item.TriggerEquipEvents(DraggedItem.Model, DraggedItem.Slot);
            }
            else
            {
                DraggedItem.Model.Store(DraggedItem.Item, DraggedItem.xPos, DraggedItem.yPos, false);
            }

            return true;
        }

        /// <summary>
        /// Updates the entire grid UI to match the state of the model.
        /// </summary>
        void SyncViewToModel()
        {
            if (Model == null) return;

            //Reset all slots to default size.
            ResetSlotRange(0, 0, Model.GridCellsX, Model.GridCellsY);
            if (Model.IsInitialized)
            {
                PGISlot slot;
                for (int i = 0; i < Slots.Count; i++)
                {
                    slot = Slots[i];
                    if (slot != null)
                    {
                        PGISlotItem item = Model.GetSlotContents(slot.xPos, slot.yPos);
                        if (item != null)
                        {
                            //There is an item in this slot. Make sure the slot is the right size.
                            //Order is important here. It ensures that 3D icon meshes will scale correctly
                            //in the event that they don't constantly check (mesh update interval is negative)
                            ResizeSlot(item.xInvPos, item.yInvPos, item.CellWidth, item.CellHeight);
                            slot.AssignItem(item);
                        }
                        else
                        {
                            //This will ensure that 3D icon meshes will scale correctly
                            //in the event that they don't constantly check (mesh update interval is negative)
                            slot.AssignItem(null);
                        }
                    }
                }

                //TODO: We need to make equipment slots part of the overall dirty-flag of the model
                //if any equip slots are dirty, simply update the highlighting
                //HACK ALERT: Forcing equipment slots to update now
                if (Model.Equipment != null)
                {
                    for (int i = 0; i < Model.Equipment.Length; i++)
                        if (Model.Equipment[i] != null) Model.Equipment[i].Dirty = true;
                }

                var dirtySlots = Model.GetAllDirtySlots();

                //update highlighting of all slots
                for (int i = 0; i < dirtySlots.Count; i++)
                {
                    slot = dirtySlots[i];
                    if (slot.Blocked) slot.HighlightColor = BlockedColor;
                    slot.AssignItem(slot.Item);
                    slot.Dirty = false;
                }
            }
        }

        /// <summary>
        /// Calculates the grid cell position given the
        /// number of cells and the size of the parenting object.
        /// </summary>
        /// <returns>The cell position.</returns>
        /// <param name="cellX">X position on grid.</param>
        /// <param name="cellY">Y Position on the grid.</param>
        /// <param name="slotWidth">Slot width.</param>
        /// <param name="slotHeight">Slot height.</param>
        public Vector2 CalculateCellPos(int cellX, int cellY, int slotWidth = 1, int slotHeight = 1)
        {
            float yDir = (VerticalOrder == VerticalOrdering.TopToBottom) ? -1.0f : 1.0f;
            float xDir = (HorizontalOrder == HorizontalOrdering.LeftToRight) ? 1.0f : -1.0f;
            float cellPosX = (float)(cellX * CellScaleX) * xDir;
            float cellPosY = (float)(cellY * CellScaleY) * yDir;
            float cellHalfWidth = ((CellScaleX * slotWidth) * 0.5f) * xDir;
            float cellHalfHeight = ((CellScaleY * slotHeight) * 0.5f) * yDir;

            float parentOffsetX = (ParentRect.rect.width * 0.5f) * xDir;
            float parentOffsetY = (ParentRect.rect.height * 0.5f) * yDir;

            return new Vector2(cellPosX + cellHalfWidth - parentOffsetX,
                                   cellPosY + cellHalfHeight - parentOffsetY);

        }

        /// <summary>
        /// Calculates the size of the cell given the slot's cell width and height and the
        /// size of the grid itself. This method takes rotation into account when calculating size.
        /// </summary>
        /// <param name="slot"></param>
        /// <returns></returns>
        public Vector2 CalculateSize(PGISlot slot)
        {
            float i = ParentRect.rect.width / Model.GridCellsX;
            float j = ParentRect.rect.height / Model.GridCellsY;

            float w = i * slot.GridWidth;
            float h = j * slot.GridHeight;

            return new Vector2(w, h);
        }

        /// <summary>
        /// Calculates the size of the cell given the slot's cell width and height and the
        /// size of the grid itself. This method does not consider if the item is rotated but simply
        /// bases it off the raw item's original width and height.
        /// </summary>
        /// <param name="slot"></param>
        /// <returns></returns>
        /// </summary>
        /// <param name="slot"></param>
        /// <returns></returns>
        public Vector2 CalculateNonRotatedSize(PGISlot slot)
        {
            float i = ParentRect.rect.width / Model.GridCellsX;
            float j = ParentRect.rect.height / Model.GridCellsY;

            float w, h;
            if (slot.Item != null && slot.Item.Rotated)
            {
                w = i * slot.GridHeight;
                h = j * slot.GridWidth;
            }
            else
            {
                w = i * slot.GridWidth;
                h = j * slot.GridHeight;
            }

            return new Vector2(w, h);
        }

        /// <summary>
        /// Returns the <see cref="PGISlot"/> found in the given grid coordinates. This represents
        /// a <see cref="PGIView"/> grid, not the internal grid of the model.
        /// </summary>
        /// <returns>The slot cell of this view.</returns>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        PGISlot GetSlotCell(int x, int y)
        {
            if (x < Model.GridCellsX && y < Model.GridCellsY)
                return Slots[(y * Model.GridCellsX) + x];

            return null;
        }

        /// <summary>
        /// Resizes the slot at the given location to the given grid-cell size.
        /// </summary>
        /// <returns><c>true</c>, if slot was resized, <c>false</c> otherwise.</returns>
        /// <param name="slot">Slot.</param>
        /// <param name="slotWidth">Slot width.</param>
        /// <param name="slotHeight">Slot height.</param>
        bool ResizeSlot(int x, int y, int width, int height)
        {
            //check for items that aren't actually in a slot (this is a defense against resizing a slot for an item that
            //was recently removed from a grid slot but the model isn't in sync just yet).
            if (x < 0 || y < 0) return false;

            PGISlot initial = this.GetSlotCell(x, y);

            //now, disable any slots that we will be stretching this slot overtop of
            for (int t = y; t < y + height; t++)
            {
                for (int s = x; s < x + width; s++)
                {
                    PGISlot slot = this.GetSlotCell(s, t);
                    slot.GridWidth = width;
                    slot.GridHeight = height;
                    if (s == x && t == y)
                    {
                        //this is the cell we are resizing. Set the new size
                        RectTransform rect = slot.GetComponent<RectTransform>();
                        rect.sizeDelta = CalculateSize(slot);
                        rect.anchoredPosition = CalculateCellPos(s, t, width, height);
                        #if PGI_SLOT_BATCHING_AID
                        if (BatchSlots) slot.UpdateSlotSize();
                        #endif
                    }
                    else
                    {
                        //this is a cell that we are disabling because
                        //the resized cell will be covering it
                        slot.gameObject.SetActive(false);
                        slot.OverridingSlot = initial;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Helper method used to reset a range of cell-slots to 
        /// a normal condition. Used mostly to restore slots that
        /// were previously disabled and covered up when another
        /// slot had to grow in size.
        /// </summary>
        /// <param name="xPos">X position.</param>
        /// <param name="yPos">Y position.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        /// <param name="active">Optional active flag to pass to the <see cref="PGISlot.SetActive"/> method.</param>
        void ResetSlotRange(int xPos, int yPos, int width, int height, bool active = true)
        {
            if (ParentRect == null || Model == null) return;

            float i = ParentRect.rect.width / Model.GridCellsX;
            float j = ParentRect.rect.height / Model.GridCellsY;

            for (int y = yPos; y < yPos + height; y++)
            {
                for (int x = xPos; x < xPos + width; x++)
                {
                    PGISlot slot = this.GetSlotCell(x, y);
                    if (slot != null)
                    {
                        slot.GridWidth = 1;
                        slot.GridHeight = 1;
                        RectTransform rect = slot.GetComponent<RectTransform>();
                        rect.sizeDelta = new Vector2(i, j);
                        rect.anchoredPosition = CalculateCellPos(x, y, 1, 1);
                        slot.gameObject.SetActive(active);
                        slot.OverridingSlot = null;
                        #if PGI_SLOT_BATCHING_AID
                        if (BatchSlots) slot.UpdateSlotSize();
                        #endif
                    }
                }
            }
        }

        /// <summary>
        /// Using the currently hovered slot and inventory size of the selected item,
        /// this method determines the final offset location to place the item so
        /// that it will fit the grid in the nearest selected set of grid slots
        /// and fits within the grid itself.
        /// <remarks>
        /// This aids in a refinement nicety for the user as the placement of items
        /// will seem more natural when the item snaps to the closest set of grid
        /// cells when they are placing larger inventory items.
        /// </remarks>
        /// </summary>
        /// <returns>The slot to actually highlight or store an item in based on item-size offsets.</returns>
        /// <param name="item">The item whose size will be used for offset calculations.</param>
        /// <param name="slot">The original slot being targedt for a drop or hilight.</param>
        PGISlot GetOffsetSlot(PGISlotItem item, PGISlot slot)
        {
            if (slot.IsEquipmentSlot) return slot;
            int offsetX = slot.xPos;
            int offsetY = slot.yPos;
            Vector2 quad = slot.GetLocalMouseCoords();
            if (item == null) return slot;
            if (item.CellWidth > 1)
            {
                if (HorizontalOrder == HorizontalOrdering.LeftToRight)
                {
                    //offset based on the quadrant of the selected cell
                    if ((item.CellWidth & 0x1) == 1) offsetX -= ((int)(item.CellWidth / 2)); //odd width
                    else if (quad.x < 0.0f) offsetX -= ((int)(item.CellWidth / 2)); //even width
                    else offsetX -= ((int)(item.CellWidth / 2) - 1);//even width
                }
                else
                {
                    //offset based on the quadrant of the selected cell
                    if ((item.CellWidth & 0x1) == 1) offsetX -= ((int)(item.CellWidth / 2)); //odd width
                    else if (quad.x > 0.0f) offsetX -= ((int)(item.CellWidth / 2)); //even width
                    else offsetX -= ((int)(item.CellWidth / 2) - 1);//even width
                }
            }

            if (item.CellHeight > 1)
            {
                if (VerticalOrder == VerticalOrdering.TopToBottom)
                {
                    //offset based on the quadrant of the selected cell
                    if ((item.CellHeight & 0x1) == 1) offsetY -= ((int)(item.CellHeight / 2)); //odd height
                    else if (quad.y > 0.0f) offsetY -= ((int)(item.CellHeight / 2)); //even height
                    else offsetY -= ((int)(item.CellHeight / 2) - 1);//event height
                }
                else
                {
                    //offset based on the quadrant of the selected cell
                    if ((item.CellHeight & 0x1) == 1) offsetY -= ((int)(item.CellHeight / 2)); //odd height
                    else if (quad.y < 0.0f) offsetY -= ((int)(item.CellHeight / 2)); //even height
                    else offsetY -= ((int)(item.CellHeight / 2) - 1);//even height
                }
            }
            //keep the final location within the grid
            if (offsetX < 0) offsetX = 0;
            if (offsetX > slot.Model.GridCellsX || offsetX + item.CellWidth > slot.Model.GridCellsX) offsetX = slot.Model.GridCellsX - item.CellWidth;
            if (offsetY < 0) offsetY = 0;
            if (offsetY > slot.Model.GridCellsY || offsetY + item.CellHeight > slot.Model.GridCellsY) offsetY = slot.Model.GridCellsY - item.CellHeight;

            return slot.View.GetSlotCell(offsetX, offsetY);
        }

        /// <summary>
        /// Handles highlighting effects when hovering over a grid slot
        /// while performing a drag. This method calculates
        /// the nearest central location for placing an item within
        /// the grid and highlights all cells that will be
        /// used for storage.
        /// </summary>
        /// <param name="slot">The slot that the pointer is currently over.</param>
        /// <param name="item">The item being dragged or dropped.</param>
        void SelectSlot(PGISlot slot, PGISlotItem item)
        {
            //if the item is too big, just highlight everything in the grid as invalid and be done with it.
            if (!slot.IsEquipmentSlot && (item.CellHeight > this.Model.GridCellsY || item.CellWidth > this.Model.GridCellsX))
            {
                List<PGISlot> slots = slot.View.Slots;
                for(int i = 0; i < slots.Count; i++) slots[i].HighlightColor = InvalidColor;
                return;
            }

            if (slot.IsEquipmentSlot)
            {
                //check highlighting for equipment slots here
                if (slot.Blocked) slot.HighlightColor = BlockedColor;
                else if (slot.Model.CanSwap(item, slot))
                {
                    slot.HighlightColor = HighlightColor;
                }
#if !PGI_LITE
                else if (slot.Model.CanSocket(item, slot.Item))
                {
                    slot.HighlightColor = SocketValidColor;
                }
#endif
                else
                {
                    slot.HighlightColor = InvalidColor;
                }

                return;
            }

            //check grid slots for special-case highlighting like socketables and stackables
            Color color;
            if (item == null || slot == null) return;
            var offset = slot.View.GetOffsetSlot(item, slot);
            if (slot.Model.CanStack(item, offset.xPos, offset.yPos) || slot.Model.CanStore(item, offset.xPos, offset.yPos))
                color = HighlightColor;
#if !PGI_LITE
            else if (slot.Model.CanSocket(item, slot.Item))
                color = SocketValidColor;
#endif
            else color = InvalidColor;


            //find out which slots to highlight based on current hover location
            //and the neighboring slots
            for (int y = offset.yPos; y < offset.yPos + item.CellHeight; y++)
            {
                for (int x = offset.xPos; x < offset.xPos + item.CellWidth; x++)
                {
                    PGISlot start = slot.View.GetSlotCell(x, y);
                    start.HighlightColor = color;
                    if (start.OverridingSlot != null) start.OverridingSlot.HighlightColor = color;
                }
            }
        }

        /// <summary>
        /// Helper method used to append grid views to a list
        /// that can later be cleared. Used for processing what
        /// view's slots need to be de-highlighted.
        /// </summary>
        /// <param name="view">The view to add to the list.</param>
        static void AppendClearList(PGIView view)
        {
            if (!ClearList.Contains(view)) ClearList.Add(view);
        }

        /// <summary>
        /// Removes all drag-related highlighting from all grid cells
        /// and equipment slots in this <see cref="PGIView"/>.
        /// </summary>
        public void DeselectAllSlots()
        {
            for (int i = 0; i < Slots.Count; i++ )
            {
                Slots[i].AssignHighlighting(this);
            }
            if (Model.Equipment != null)
            {
                PGISlot slot;
                for (int i = 0; i < Model.Equipment.Length; i++ )
                {
                    slot = Model.Equipment[i];
                    if (slot.Blocked) slot.HighlightColor = BlockedColor;
                    else slot.AssignHighlighting(this);
                }
            }
        }

        /// <summary>
        /// Helper method used to remove selection from all previously stored grid views.
        /// This can be kinda slow since it will inevitably cycle through all slots
        /// in all inventories that were dragged over during a drag operation.
        /// <seealso cref="PGIView.AppendClearList"/>
        /// <seealso cref="PGIView.DeselectAllSlots"/>
        /// 
        /// <remarks>
        /// TODO: This could use a good amount of optimizing. Likely, the
        /// OnDeselectAll method could use a dirty list to only clear
        /// slots that have changed recently.
        /// </remarks>
        /// </summary>
        static void DeselectAllViews()
        {
            for (int i = 0; i < ClearList.Count; i++)
            {
                if (ClearList[i] != null) ClearList[i].DeselectAllSlots();
            }

        }

        /// <summary>
        /// Handles the previously registered <see cref="PGISlot.OnHover"/> event 
        /// when the pointer first enters a <see cref="PGISlot"/> and invokes
        /// this view's <see cref="PGIView.OnHoverSlot"/> event.
        /// </summary>
        /// <param name="eventData">The pointer event data that triggered the event.</param>
        /// <param name="slot">The slot that the pointer entered.</param>
        void OnHoverEvent(PointerEventData eventData, PGISlot slot)
        {
            if (DraggedItem == null && slot.Item != null)
            {
                OnHoverSlot.Invoke(eventData, slot);
            }
        }

        /// <summary>
        /// Handles the previously registered <see cref="PGISlot.OnEndHover"/> event 
        /// when the pointer leaves a <see cref="PGISlot"/> and invokes
        /// this view's <see cref="PGIView.OnEndHoverSlot"/> event.
        /// </summary>
        /// <param name="eventData">The pointer event data that triggered the event.</param>
        /// <param name="slot">The slot that the pointer exited.</param>
        void OnEndHoverEvent(PointerEventData eventData, PGISlot slot)
        {
            OnEndHoverSlot.Invoke(eventData, slot);
        }

        /// <summary>
        /// Handles the previously registered <see cref="PGISlot.OnClick"/> event 
        /// when the pointer clicks on a <see cref="PGISlot"/> and invokes
        /// this view's <see cref="PGIView.OnClickSlot"/> event.
        /// </summary>
        /// <param name="eventData">The pointer event data that triggered the event.</param>
        /// <param name="slot">The slot that was clicked.</param>
        void OnClickSlotHandler(PointerEventData eventData, PGISlot slot)
        {
            OnClickSlot.Invoke(eventData, slot);
        }

        /// <summary>
        /// Reacts to the event that the model reference has changed. This is usually triggered
        /// by the model loading saved data and replacing the original model object with an all new copy.
        /// </summary>
        /// <param name="model"></param>
        void OnChangeModel(PGIModel model)
        {
            Model = model;
        }
#endregion

    }
}