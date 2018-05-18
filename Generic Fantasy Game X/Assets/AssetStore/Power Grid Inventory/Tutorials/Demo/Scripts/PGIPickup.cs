using UnityEngine;


namespace PowerGridInventory.Demo
{
    [RequireComponent(typeof(PGISlotItem))]
    public class PGIPickup : TouchPickup
    {
        public AudioClip PickupSound;
        public AudioClip DropSound;

        void Awake()
        {
            var item = GetComponent<PGISlotItem>();
            item.OnStoreInInventory.AddListener(OnStore);
            item.OnRemoveFromInventory.AddListener(OnRemove);
        }

        void OnDestroy()
        {
            var item = GetComponent<PGISlotItem>();
            item.OnStoreInInventory.RemoveListener(OnStore);
            item.OnRemoveFromInventory.RemoveListener(OnRemove);
        }

        protected override void OnPickup(Grabber who, GameObject root)
        {
            base.OnPickup(who, root);
            var model = root.GetComponentInChildren<PGIModel>();
            if (model != null)
                model.Pickup(GetComponent<PGISlotItem>());

        }

        public void OnStore(PGISlotItem item, PGIModel model)
        {
            gameObject.SetActive(false);
            gameObject.transform.position = model.transform.position;
            if (PickupSound != null) AudioSource.PlayClipAtPoint(PickupSound, transform.position);
        }

        public void OnRemove(PGISlotItem item, PGIModel model)
        {
            gameObject.SetActive(true);
            gameObject.transform.position = model.transform.position;
            if(DropSound != null) AudioSource.PlayClipAtPoint(DropSound, transform.position);
            OnDrop();
        }
    }
}
