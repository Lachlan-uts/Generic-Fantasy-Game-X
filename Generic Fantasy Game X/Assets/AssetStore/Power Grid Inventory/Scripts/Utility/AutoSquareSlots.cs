/**********************************************
* Power Grid Inventory
* Copyright 2015-2017 James Clark
**********************************************/
using UnityEngine;
using Toolbox.Common;
using UnityEngine.UI;
using System.Xml.Serialization;
using Toolbox;

namespace PowerGridInventory
{
    /// <summary>
    /// Attach this component to the parent of a <see cref="GridView"/>
    /// to provide automatic resize of the view in order to maintain square slots.
    /// </summary>
    [ExecuteInEditMode]
    [AddComponentMenu("PGI/Utility/Auto-Square Slots", 21)]
    [RequireComponent(typeof(RectTransform))]
    public class AutoSquareSlots : MonoBehaviourEx
    {
        [Inspectable]
        [XmlIgnore]
        public PGIView View
        {
            get { return _View; }
            set
            {
                //be sure to remove previous view's listener
                if (_View != value && _View != null)
                    _View.Model.OnEndGridResize.RemoveListener(HandleModelResize);

                _View = value;
                if (value != null)
                {
                    _View.Model.OnEndGridResize.AddListener(HandleModelResize);
                    Fitter = _View.GetComponent<AspectRatioFitter>();
                    if (Fitter == null) Fitter = _View.gameObject.AddComponent<AspectRatioFitter>();
                }
            }
        }
        [SerializeField]
        [HideInInspector]
        PGIView _View;
        

        RectTransform RectTrans;
        AspectRatioFitter Fitter;

        void OnEnable()
        {
            RectTrans = transform as RectTransform;
            UpdateView();
        }

        public void OnDestroy()
        {
            if(View != null)
                View.Model.OnEndGridResize.RemoveListener(HandleModelResize);
            
        }

        #if UNITY_EDITOR
        /// <summary>
        /// Used to render frequent changes in-editor.
        /// </summary>
        protected void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                UpdateView();
        }
        #endif
        
        /// <summary>
        /// Used to render frequent changes in playmode.
        /// </summary>
        void Update()
        {
            #if UNITY_EDITOR
            if (!Application.isPlaying) return;
            #endif

            UpdateView();
        }

        /// <summary>
        /// Recalculates the size of the PGIView's RectTransform so that it maitains square slots
        /// while fitting as much of the space of this object's RectTransform as possible.
        /// </summary>
        public void UpdateView()
        {
            if (RectTrans != null && RectTrans.hasChanged &&
                View != null && View.Model != null)
            {
                if (Fitter == null) Fitter = _View.gameObject.GetComponent<AspectRatioFitter>();
                if (Fitter == null) Fitter = _View.gameObject.AddComponent<AspectRatioFitter>();

                RectTransform viewTrans = View.transform as RectTransform;
                viewTrans.anchoredPosition = Vector2.zero;
                Fitter.aspectRatio = ((float)View.Model.GridCellsX / (float)View.Model.GridCellsY);
                Fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            }
            
        }

        void HandleModelResize()
        {
            UpdateView();
        }
    }
}
