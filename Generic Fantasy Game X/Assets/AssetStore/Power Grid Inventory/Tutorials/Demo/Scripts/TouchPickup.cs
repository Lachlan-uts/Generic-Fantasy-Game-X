
using UnityEngine;

namespace PowerGridInventory.Demo
{
    public abstract class TouchPickup : MonoBehaviour
    {
        protected Grabber Owner;
        protected float DropTime;

        protected virtual void OnPickup(Grabber who, GameObject root)
        {
            
        }

        public virtual void OnDrop()
        {
            Owner = null;
            DropTime = Time.unscaledTime;
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            if (Owner == null && Time.unscaledTime - DropTime > 0.5f)
            {
                var root = collision.collider.attachedRigidbody.gameObject;
                Owner = root.GetComponentInChildren<Grabber>();
                if (Owner != null) OnPickup(Owner, root);
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (Owner == null && Time.unscaledTime - DropTime > 0.5f)
            {
                var body = other.attachedRigidbody;
                if (body != null)
                {
                    var root = body.gameObject;
                    Owner = root.GetComponentInChildren<Grabber>();
                    if (Owner != null) OnPickup(Owner, root);
                }
            }
        }

    }
}
