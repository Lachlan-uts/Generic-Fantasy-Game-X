/**********************************************
* Power Grid Inventory
* Copyright 2015-2017 James Clark
**********************************************/
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using PowerGridInventory.Utility;
using System.Xml.Serialization;
using Toolbox.Graphics;
using Toolbox.Common;

namespace PowerGridInventory
{
    /// <summary>
    /// Represents a single slot within the inventory grid's UI.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("Power Grid Inventory/Slot", 13)]
    [Serializable]
    public class PGISlot : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IDropHandler,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
    {
        #region Internal Classes
        [Serializable]
        public class DragTrigger : UnityEvent<PointerEventData> { }
        [Serializable]
        public class SlotTrigger : UnityEvent<PointerEventData, PGISlot> { }
        #endregion


        #region Members and Properties
        //visible to the inspector

        /// <summary>
        /// Excludes this slot from being considered when a <see cref="PGIModel"/>
        /// is searching for empty equipment slots to auto-equip an item to.
        /// </summary>
        [Header("Inventory Behaviour")]
        [Tooltip("Excludes this slot from being considered when a PGIModel is searching for empty equipment slots to auto-equip an item to.")]
        public bool SkipAutoEquip = false;

        /// <summary>
        /// The default icon to use when no item is displayed in this slot.
        /// </summary>
        [XmlIgnore]
        public Sprite DefaultIcon
        {
            get { return _DefaultIcon; }
            set
            {
                _DefaultIcon = value;
                if (Item == null) SetDefaultIcon();
            }
        }
        [SerializeField]
        [Tooltip("The default icon to use when no item is displayed in this slot.")]
        private Sprite _DefaultIcon;

        /// <summary>
        /// The color of the icon when no item is equipped in it's slot.
        /// </summary>
        [Tooltip("The color of the icon when no item is equipped in it's slot.")]
        public Color DefaultIconColor = Color.white;

        /// <summary>
        /// A reference to the 'Highlighted' portion of a SlotPrefab GameObject. See the Manual about 'Slot Prefab Spec' for more details.
        /// </summary>
        [Header("Sub-GameObject References")]
        [Tooltip("A reference to the 'Highlighted' portion of a SlotPrefab GameObject. See the Manual about 'Slot Prefab Spec' for more details.")]
        public Image Highlight;

        /// <summary>
        /// A reference to the 'Icon' portion of a SlotPrefab GameObject. See the Manual about 'Slot Prefab Spec' for more details.
        /// </summary>
        [Tooltip("A reference to the 'Icon' portion of a SlotPrefab GameObject. See the Manual about 'Slot Prefab Spec' for more details.")]
        public Image IconImage;

        /// <summary>
        /// A reference to the '3D Icon' portion of a SlotPrefab GameObject. See the Manual about 'Slot Prefab Spec' for more details.
        /// </summary>
        [Tooltip("A reference to the '3D Icon' portion of a SlotPrefab GameObject. See the Manual about 'Slot Prefab Spec' for more details.")]
        public Image3D IconMesh;

        /// <summary>
        /// A reference to the 'StackSize' portion of a SlotPrefab GameOBject. See the Manual about 'Slot Prefab Spec' for more details.
        /// </summary>
        [Tooltip("A reference to the 'StackSize' portion of a SlotPrefab GameOBject. See the Manual about 'Slot Prefab Spec' for more details.")]
        public Text StackSize;

        //invisible
        /// <summary>
        /// Internal flag used to ensure that this slot does not overwrite data that was set
        /// in its associated <see cref="PGModel"/> or <see cref="PGIView"/>.
        /// Not for external use.
        /// </summary>
        [HideInInspector]
        [NonSerialized]
        public bool ModelInitialized = false;

        /// <summary>
        /// The equipment index this slot is assigned to if it is being used as an
        /// equipment slot by a <see cref="PGIModel"/> or -1 if it is not.
        /// <seealso cref="PGIModel.Equipment"/>
        /// </summary>
        [HideInInspector]
        [NonSerialized]
        public int EquipmentIndex = -1;

        /// <summary>
        /// The x-axis grid location this slot has been assigned to by
        /// a <see cref="PGIView"/> or -1 if it is not a grid slot.
        /// <seealso cref="PGIView.GetSlotCell"/>
        /// </summary>
        [HideInInspector]
        [NonSerialized]
        public int xPos = -1;
        
        /// <summary>
        /// The y-axis location this slot has been assigned to by
        /// a <see cref="PGIView"/> or -1 if it is not a grid slot.
        /// <seealso cref="PGIView.GetSlotCell"/>
        /// </summary>
        [HideInInspector]
        [NonSerialized]
        public int yPos = -1;

        /// <summary>
        /// The number of cells wide this slot is in a grid.
        /// This value will often be changed automatically to match
        /// the item contents of this slot. It should never be set manually
        /// unless you know what you are doing.
        /// </summary>
        [HideInInspector]
        [NonSerialized]
        public int GridWidth = 1;

        /// <summary>
        /// The number of cells high this slot is in a grid.
        /// This value will often be changed automatically to match
        /// the item contents of this slot. It should never be set manually
        /// unless you know what you are doing.
        /// </summary>
        [HideInInspector]
        [NonSerialized]
        public int GridHeight = 1;

        /// <summary>
        /// The y-axis grid location this slot has been assigned to by
        /// a <see cref="PGIView"/> or -1 if it is not a grid slot.
        /// <seealso cref="PGIView.GetSlotCell"/>
        /// </summary>
        [HideInInspector]
        public PGISlot OverridingSlot; //slot that is covering this one
        
        /// <summary>
        /// The <see cref="PGIModel"/> this slot is being used by.
        /// </summary>
        [HideInInspector]
        public PGIModel Model
        {
            get { return View.Model; }
        }

        /// <summary>
        /// The <see cref="PGIView"/> this slot is being used by
        /// if it is part of a grid.
        /// </summary>
        [HideInInspector]
        public PGIView View;
        [HideInInspector]
        [SerializeField]
        bool _Blocked = false;

        /// <summary>
        /// Flags this slot as being blocked and disallows any item from being equipped to it.
        /// </summary>
        /// <remarks>
        /// Setting this value will also set this slot's <see cref="PGISlot.Dirty"/> flag to <c>true</c>.
        /// </remarks>
        [HideInInspector]
        public bool Blocked
        {
            get { return _Blocked; }
            set { _Blocked = value; Dirty = true; }
        }

        /// <summary>
        /// Signifies that a change has occured in this slot and that any associated <see cref="PGIView"/>
        /// should re-render this slot with the updated state information.
        /// </summary>
        [HideInInspector]
        public bool Dirty = false;

        //These are mostly cached references to
        //neccessary components, child objects,
        //and frequently accessed data.
        [HideInInspector]
        [SerializeField]
        private PGISlotItem _Item;

        /// <summary>
        /// A reference to the <see cref="PGISlotItem"/> component of the item GameObject
        /// that is being stored in this slot. Or null if there is no item.
        /// </summary>
        [HideInInspector]
        public PGISlotItem Item
        {
            get { return _Item; }
            protected set
            {
                if (_Item != value)
                {
                    _Item = value;
                    Dirty = true;
                }
            }
        }

        //None of this stuff needs to be serialized. It's either temporary storage
        //or it's set up during the awake sequence.
        [HideInInspector]
        public RectTransform LocalRect { get; protected set; }
        private Canvas _Canvas;
        protected Canvas Canvas
        {
            get
            {
                if (_Canvas == null)
                {
                    _Canvas = gameObject.GetComponentInParent<Canvas>();
                    if (_Canvas == null)
                        Debug.LogError("This GridSlot must have a parent somewhere in its heirarchy that has a Canvas attached to it.");
                }
                return _Canvas;
            }
        }
        protected Image BackgroundImage;
        protected Vector2 CachePosition;
        protected SlotBatch[] BatchedElements;
        bool Ready = false;

        //Data shared by all slots.
        protected static bool InvalidateClick = false;
        protected static bool TouchHover = false;
        protected static float HoverStartTime = 0.0f;
        protected static readonly float MinHoverTime = 0.75f;
        

        /// <summary>
        /// Returns true if this slot is considered a <see cref="PGIModel"/>'s equipment slot.
        /// </summary>
        [HideInInspector]
        [XmlIgnore]
        public bool IsEquipmentSlot
        {
            get { return (EquipmentIndex >= 0); }
        }

        /// <summary>
        /// Convenience accessor that references this slot's IconImage sprite.
        /// See the Manual under 'Slot Prefab Spec' for more details.
        /// </summary>
        [HideInInspector]
        [XmlIgnore]
        public Sprite Icon
        {
            protected set
            {
                IconImage.sprite = value;
                if (value == null) IconImage.enabled = false;
                else IconImage.enabled = true;
            }
            get 
            { 
                return IconImage.sprite;  
            }
        }

        /// <summary>
        /// Convenience accessor that references this slot's CanvasMesh mesh.
        /// See the Manual under 'Slot Prefab Spec' for more details.
        /// </summary>
        [HideInInspector]
        [XmlIgnore]
        public Mesh Icon3D 
        {
            protected set
            {
                IconMesh.Mesh = value;
                if (value == null) IconMesh.enabled = false;
                else IconMesh.enabled = true;
                //IconMesh.UpdateView();
            }
            get { return IconMesh.Mesh; }
        }

        /// <summary>
        /// Convenience accessor that references this slot's CanvasMesh material.
        /// See the Manual under 'Slot Prefab Spec' for more details.
        /// </summary>
        [HideInInspector]
        public Material Icon3DMat
        {
            protected set { IconMesh.material = value; }
            get { return IconMesh.material; } 
        }

        protected int LastStackCount = 1;

        /// <summary>
        /// Convenience accessor that references the current stack count displayed in
        /// the Text UI element of this slot.
        /// </summary>
        [XmlIgnore]
        public int StackCount
        {
            protected set
            {
                if (LastStackCount != value)
                {
                    //the stack count has changed for our item, update the text
                    LastStackCount = value;
                    if (value < 2) StackSize.enabled = false;
                    else
                    {
                        StackSize.text = value.ToString();
                        StackSize.enabled = true;
                    }
                }
                else if (value < 2) StackSize.enabled = false;
            }

            get 
            {
                if (string.IsNullOrEmpty(this.StackSize.text)) return 0;
                else return int.Parse(StackSize.text);
            }
        }

        /// <summary>
        /// Convenience accessor that reference tis slot's 'Highlight' UI element's color.
        /// See the Manual under 'Slot Prefab Spec' for more details.
        /// </summary>
        [HideInInspector]
        [XmlIgnore]
        public Color HighlightColor
        {
            set
            {
                //NOTE: We are checking for null because sometimes when switching a view's model
                //around, we have leftover views to deselect and this will get called even though
                //there are no slots active for that view.
                if (Highlight != null)
                {
                    Highlight.color = value;
                    //disable the highlight if the color is fully transparent
                    if (value.a <= 0.001f) Highlight.enabled = false;
                    else Highlight.enabled = true;
                }
            }
            get { return Highlight.color; }
        }

        #endregion


        #region Triggered Events
        /// <summary>
        /// Mostly for internal use by a <see cref="PGIView"/>. This event allows
        /// the view to hook into the drag 'n drop actions of this object.
        /// </summary>
        [HideInInspector]
        public DragTrigger OnBeginDragEvent = new DragTrigger();

        /// <summary>
        /// Mostly for internal use by a <see cref="PGIView"/>. This event allows
        /// the view to hook into the drag 'n drop actions of this object.
        /// </summary>
        [HideInInspector]
        public DragTrigger OnEndDragEvent = new DragTrigger();

        /// <summary>
        /// Mostly for internal use by a <see cref="PGIView"/>. This event allows
        /// the view to hook into the drag 'n drop actions of this object.
        /// </summary>
        [HideInInspector]
        public DragTrigger OnDragEvent = new DragTrigger();

        /// <summary>
        /// Mostly for internal use by a <see cref="PGIView"/>. This event allows
        /// the view to hook into the drag 'n drop actions of this object.
        /// </summary>
        [HideInInspector]
        public DragTrigger OnDragDropEvent = new DragTrigger();



        /// <summary>
        /// Invoked when a <see cref="PGISlotItem"/> is about to be equipped to this slot.
        /// You can disallow this action by setting the the provided model's
        /// <see cref="PGIModel.CanPerformAction"/> to <c>false</c>.
        /// <seealso cref="PGIModel.OnCanEquipItem"/>
        /// <seealso cref="PGISlotItem.OnCanEquip"/>
        /// </summary>
        [SerializeField]
        [PGIFoldedEvent]
        public PGISlotItem.InventorySlotEvent OnCanEquipItem = new PGISlotItem.InventorySlotEvent();

        /// <summary>
        /// Invoked when a <see cref="PGISlotItem"/> is about to be unequipped from this slot.
        /// You can disallow this action by setting the the provided model's
        /// <see cref="PGIModel.CanPerformAction"/> to <c>false</c>.
        /// <seealso cref="PGIModel.OnCanUnequipItem"/>
        /// <seealso cref="PGISlotItem.OnCanUnequip"/>
        /// </summary>
        [SerializeField]
        [PGIFoldedEvent]
        public PGISlotItem.InventorySlotEvent OnCanUnequipItem = new PGISlotItem.InventorySlotEvent();

        /// <summary>
        /// Invoked after a <see cref="PGISlotItem"/> has been equipped to this slot.
        /// <seealso cref="PGIModel.OnEquipItem"/>
        /// <seealso cref="PGISlotItem.OnEquip"/>
        /// </summary>
        [SerializeField]
        [PGIFoldedEvent]
        public PGISlotItem.InventorySlotEvent OnEquipItem = new PGISlotItem.InventorySlotEvent();

        /// <summary>
        /// Invoked after a <see cref="PGISlotItem"/> has been removed from this slot.
        /// This will occur even when dragging an item from the slot but before dropping it
        /// in a new location.
        /// <seealso cref="PGIModel.OnUnequipItem"/>
        /// <seealso cref="PGISlotItem.OnUnequip"/>
        /// </summary>
        [SerializeField]
        [PGIFoldedEvent]
        public PGISlotItem.InventorySlotEvent OnUnequipItem = new PGISlotItem.InventorySlotEvent();

        /// <summary>
        /// Invoked after a <see cref="PGISlotItem"/> has failed to be equipped to this slot. Usually this
        /// is the result of a 'Can...' method disallowing the item to be sotred or simply
        /// the fact that there was another item already located in the same slot. This
        /// method may be called frequiently when using <see cref="PGIModel.FindFreeSpace"/>
        /// or <see cref="PGIModel.Pickup"/>.
        /// <seealso cref="PGIModel.OnEquipItemFailed"/>
        /// <seealso cref="PGISlotItem.OnEquipFailed"/>
        /// </summary>
        [SerializeField]
        [PGIFoldedEvent]
        public PGISlotItem.InventorySlotEvent OnEquipItemFailed = new PGISlotItem.InventorySlotEvent();

        /// <summary>
        /// Invoked when the pointer is clicked on this slot when an item is in it.
        /// <seealso cref="PGISloItem.OnClick"/> 
        /// <seealso cref="PGIView.OnClickSlot"/> 
        /// </summary>
        [SerializeField]
        [PGIFoldedEvent]
        public SlotTrigger OnClick = new SlotTrigger();

        /// <summary>
        /// Invoked when the pointer first enters this slot and an item is in it.
        /// <seealso cref="PGIView.OnHover"/> 
        /// </summary>
        [SerializeField]
        [PGIFoldedEvent]
        public SlotTrigger OnHover = new SlotTrigger();

        /// <summary>
        /// Invoked when the pointer leaves this slot and an item is in it.
        /// <seealso cref="PGIView.OnEndHover"/> 
        /// </summary>
        [SerializeField]
        [PGIFoldedEvent]
        public SlotTrigger OnEndHover = new SlotTrigger();
        [SerializeField]
        [FoldFlag("Events")]
        public bool FoldedEvents = false; //used by the inspector
        #endregion

        
        #region Unity Events
        protected void Awake()
        {
            if (IconImage == null) throw new UnityException("You must supply a gameobject with a UI.Image component attached to the 'IconImage' child object of each Grid Slot");
            
            //this will be used later when we need to update each of this object's
            //child elements. The reason is because those child elements will
            //may no longer be children due to the batching system used by the PGIView.
            if (transform.childCount > 0)
            {
                BatchedElements = new SlotBatch[transform.childCount];
                for (int i = 0; i < transform.childCount; i++)
                {
                    BatchedElements[i] = transform.GetChild(i).GetComponent<SlotBatch>();
                }
            }

            if (!ModelInitialized)
            {
                EquipmentIndex = -1;
                xPos = -1;
                yPos = -1;
            }

            BackgroundImage = GetComponent<Image>();
            LocalRect = this.GetComponent<RectTransform>();
            

        }

        protected void Start()
        {
            if (IconMesh == null) throw new UnityException("You must supply a gameobject with a PowerGridInventory.Icon3D component attached to the 'IconMesh' child object of each Grid Slot");
            if (Highlight == null) throw new UnityException("You must supply a gameobject with a UI.Image component attached to the 'Highlight' child object of each Grid Slot");
            if (StackSize == null) throw new UnityException("You must supply a gameobject with a UI.Text component attached to the 'Stack' child object of each Grid Slot");
            Ready = true;
        }

        /**
         * We need to enable sub-objects of this slot (highlight, icon, stack size, etc) even
         * if they have become the child of another object due to view batching.
         **/
        void OnEnable()
        {
            if (BatchedElements != null)
            {
                foreach (var element in BatchedElements)
                {
                    if (element != null) element.gameObject.SetActive(true);
                }
            }

            if (_Blocked && this.View != null) this.HighlightColor = this.View.BlockedColor;
            else if (Item != null)
            {
                this.HighlightColor = Item.Highlight;
            }
            else if (this.View != null) this.HighlightColor = this.View.NormalColor;
            InitIcon();
        }

        /**
         * We need to disable sub-objects of this slot (highlight, icon, stack size, etc) even
         * if they have become the child of another object due to view batching.
         **/
        void OnDisable()
        {
            
            if (BatchedElements != null)
            {
                foreach (var element in BatchedElements)
                {
                    if (element != null)
                    {
                        element.gameObject.SetActive(false);
                    }
                }
            }

            
        }
        #endregion


        #region Methods
        /// <summary>
        /// Helper used to initialize the icon when creating the slot, either
        /// through instatiation or through deserialization.
        /// </summary>
        void InitIcon()
        {
            if (IconImage != null && IconMesh != null)
            {
                //make sure our icon state matched the default value upon startup
                if (DefaultIcon == null)
                {
                    Icon = null;
                    Icon3D = null;
                }
                else
                {
                    Icon = DefaultIcon;
                    Icon3D = null;
                }
            }
        }

        /// <summary>
        /// This method is used to return batched sub-elements as children of this GameObject.
        /// You should call this before destroying or changing this slot's transform parent.
        /// </summary>
        public void RestoreToNonbatchedState()
        {
            if (BatchedElements == null || BatchedElements.Length < 1) return;

            foreach (var element in BatchedElements)
            {
                //if element.Rect is null that means we are probably just starting
                //the game and not everything has run through the proper 'Awake' sequence yet.
                if (element != null && element.Rect != null)
                {
                    element.Rect.SetParent(this.transform, true);
                    element.Rect.anchorMin = element.AnchorMin;
                    element.Rect.anchorMax = element.AnchorMax;
                    element.Rect.offsetMin = element.OffsetMin;
                    element.Rect.offsetMax = element.OffsetMax;
                    element.Rect.localScale = element.OriginalScale;
                }
            }
        }

        /// <summary>
        /// Assigns the item to this slot. This will set both the <see cref="PGISlot.Item"/>
        /// property and <see cref="PGISlot.IconImage"/>'s sprite or <see cref="PGISlot.IconMesh"/>'s mesh and material property.
        /// </summary>
        /// <param name="item">The <see cref="PGISlotItem"/> to assign.</param>
        public virtual void AssignItem(PGISlotItem item, bool setIcon = true)
        {
            if (!Ready) return;
            //TODO: We probably can skip this entire process if the incoming item matches
            //the one already stored in this slot. This needs testing though!

            var oldItem = Item;
            Item = item;

            //set the icon
            if (Item == null)
            {
                //if (!Blocked && View != null) this.HighlightColor = this.View.NormalColor;
                SetDefaultIcon();
            }
            else
            {
                //make sure default color is removed - otherwise we might tint our icon incorrectly
                IconImage.color = Color.white;

                //we need to check for this so we don't override another color set by the view.
                //if (!Blocked && oldItem == null) this.HighlightColor = item.Highlight;

                //trigger stacksize UI element to update
                if (this._Item != null)
                {
                    StackCount = _Item.StackCount;
                }
                else if (LastStackCount != 0)
                {
                    //this will reset our stack count when the slot goes empty
                    StackCount = 0;
                }

                //set icon data
                if (item.IconType == PGISlotItem.IconAssetType.Sprite && IconImage != null)
                {
                    Icon = item.Icon;
                    IconMesh.material = null;
                    IconMesh.Rotation = item.IconOrientation;
                    Icon3D = null;

                    IconImage.enabled = true;
                    IconMesh.enabled = false;

                    //the second instance of this component is for the rotation of the slot item
                    //regardless of the orientation in the model (i.e. arbitrary visual rotation)
                    UIRotate[] rot = IconImage.GetComponents<UIRotate>();
                    if (rot != null && rot.Length > 1) rot[1].EulerAngles = item.IconOrientation;

                }
                else if (item.IconType == PGISlotItem.IconAssetType.Mesh && IconMesh != null)
                {
                    Icon = null;
                    IconMesh.material = item.IconMaterial;
                    IconMesh.Rotation = item.IconOrientation;
                    Icon3D = item.Icon3D;

                    IconImage.enabled = false;
                    IconMesh.enabled = true;
                }

                //fallback in case we are somehow missing a sprite *and* a mesh for this item's icon.
                else SetDefaultIcon();

                //rotate image to match orientation but only if it is for grid
                if (!this.IsEquipmentSlot)
                {
                    UIRotate rotateEffect = IconImage.GetComponent<UIRotate>();
                    if (rotateEffect != null)
                    {
                        switch (item.RotatedDir)
                        {
                            case PGISlotItem.RotateDirection.None:
                                {
                                    rotateEffect.EulerAngles = Vector3.zero;
                                    break;
                                }
                            case PGISlotItem.RotateDirection.CW:
                                {
                                    rotateEffect.EulerAngles = new Vector3(0.0f, 0.0f, 270.0f);
                                    break;
                                }
                            case PGISlotItem.RotateDirection.CCW:
                                {
                                    rotateEffect.EulerAngles = new Vector3(0.0f, 0.0f, 90.0f);
                                    break;
                                }
                            default:
                                {
                                    break;
                                }
                        }//end switch
                    }//end null check
                }//end if
            }

            
        }

        /// <summary>
        /// Helper for setting default icon of the slot.
        /// </summary>
        void SetDefaultIcon()
        {
            IconMesh.Rotation = Vector3.zero;
            IconMesh.material = null;
            Icon3D = null;
            StackCount = 0;
            Icon = DefaultIcon;
            
            IconImage.color = DefaultIconColor;
        }

        /// <summary>
        /// Helper used to restore this slot's color from a
        /// drag-related highlighted state to whatever it should
        /// be otherwise. The color restored to usually depends
        /// on if it has an item stored in it currently.
        /// </summary>
        /// <param name="defaultViewColor">The default color for slots when nothing is stored and the slot has no special state.</param>
        public void AssignHighlighting(PGIView view)
        {
            if (view == null) return;
            if (Blocked) HighlightColor = view.BlockedColor;
            else if (Item != null) HighlightColor = Item.Highlight;
            else HighlightColor = view.NormalColor;
        }

        /*
        public void RestoreHighlight(Color defaultViewColor)
        {
            if (Blocked) HighlightColor = this.View.BlockedColor;
            else if (Item != null) HighlightColor = Item.Highlight;
            else HighlightColor = defaultViewColor;
        }*/


        /// <summary>
        /// Gets the mouse location in local coordinates for this UI object.
        /// </summary>
        /// <returns>The local mouse coords.</returns>
        public Vector2 GetLocalMouseCoords()
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(LocalRect, Input.mousePosition, this.Canvas.worldCamera, out CachePosition))
            {
                return CachePosition;
            }

            return Vector2.zero;
        }

        /// <summary>
        /// Due to the fact that many child objects of this slot will be parented
        /// by another GameObject during startup for performance reasons, this
        /// method will often be called by a PGIView to ensure that these elements
        /// have the proper scale when different sized object enter a slot.
        /// 
        /// This is mostly a hack-ish trick to ensure that slots are resized based on the
        /// original parent's anchors when batching. By temporarily restoring them to their
        /// original slot parent, resizing, and then returning to the batch parent we can
        /// get the best of both worlds, usually.
        /// </summary>
        public void UpdateSlotSize()
        {
            if (BatchedElements == null || BatchedElements.Length < 1) return;

            foreach (var element in BatchedElements)
            {
                //if element.Rect is null that means we are probably just starting the game and not everything
                //has run through the proper 'Awake' sequence yet.
                if (element != null && element.Rect != null)// && element.ResizeElement)
                {
                    var batchParent = element.transform.parent;
                    element.Rect.SetParent(this.transform, true);

                    //BUG ALERT: Setting this value is fudging things up very badly.
                    //Don't ever set it when changing transforms like this.
                    //I'm keeping this here as a reminder.
                    //element.Rect.position = element.OriginalPos;

                    element.Rect.anchorMin = element.AnchorMin;
                    element.Rect.anchorMax = element.AnchorMax;
                    element.Rect.offsetMin = element.OffsetMin;
                    element.Rect.offsetMax = element.OffsetMax;
                    element.Rect.localScale = element.OriginalScale;

                    element.Rect.SetParent(batchParent, true);
                }
            }
        }
        #endregion


        #region Interfaces
        /// <summary>
        /// Handles the begin drag event.
        /// Usually, a <see cref="PGIView"/> will hook to this event
        /// so thatit can handle drag 'n drop events.
        /// </summary>
        /// <param name="eventData">Event data.</param>
        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            if (Item != null)
            {
                OnBeginDragEvent.Invoke(eventData);
                InvalidateClick = true;
            }
        }

        /// <summary>
        /// Handles the drop event.
        /// Usually, a <see cref="PGIView"/> will hook to this event
        /// so that it can handle drag 'n drop events.
        /// </summary>
        /// <param name="eventData">Event data.</param>
        public virtual void OnDrop(PointerEventData eventData)
        {
            OnDragDropEvent.Invoke(eventData);
            InvalidateClick = false;
        }

        /// <summary>
        /// Handles the end drag event.
        /// Usually, a <see cref="PGIView"/> will hook to this event
        /// so that it can handle drag 'n drop events.
        /// </summary>
        /// <param name="eventData">Event data.</param>
        public virtual void OnEndDrag(PointerEventData eventData)
        {
            OnEndDragEvent.Invoke(eventData);
            InvalidateClick = false;
        }

        /// <summary>
        /// Handles the drag event.
        /// Usually, a <see cref="PGIView"/> will hook to this event
        /// so that it can handle drag 'n drop events.
        /// </summary>
        /// <param name="eventData">Event data.</param>
        public virtual void OnDrag(PointerEventData eventData)
        {
            OnDragEvent.Invoke(eventData);
        }

        /// <summary>
        /// Handles the pointer enter event.
        /// Usually, a <see cref="PGIView"/> will hook to this event
        /// so thatit can handle drag 'n drop events.
        /// </summary>
        /// <param name="eventData">Event data.</param>
        public void OnPointerEnter(PointerEventData eventData)
        {
            OnHover.Invoke(eventData, this);

            //Because hovering on a touch-device necessitates touching the control we need a special case here.
            //If we are using a touch device and we were within the hovering phase before clicking, we can conclude
            //that this should not be considered a click.
            if (Input.touches.Length > 0)
            {
                HoverStartTime = Time.unscaledTime;
                TouchHover = true;
            }
        }

        /// <summary>
        /// Handles the pointer enter event.
        /// Usually, a <see cref="PGIView"/> will hook to this event
        /// so thatit can handle drag 'n drop events.
        /// </summary>
        /// <param name="eventData">Event data.</param>
        public void OnPointerExit(PointerEventData eventData)
        {
            OnEndHover.Invoke(eventData, this);
            InvalidateClick = false;
            TouchHover = false;
        }

        /// <summary>
        /// Handles the pointer click event.
        /// Usually, a <see cref="PGIView"/> will hook to this event
        /// so that it can handle drag 'n drop events.
        /// </summary>
        /// <param name="eventData">Event data.</param>
        public void OnPointerClick(PointerEventData eventData)
        {
            if(InvalidateClick) return;
            if(TouchHover && Time.unscaledTime - HoverStartTime > MinHoverTime) return;

            OnClick.Invoke(eventData, this);
            if (this.Item != null) Item.OnClick.Invoke(eventData, this);
        }
        #endregion
    }

}