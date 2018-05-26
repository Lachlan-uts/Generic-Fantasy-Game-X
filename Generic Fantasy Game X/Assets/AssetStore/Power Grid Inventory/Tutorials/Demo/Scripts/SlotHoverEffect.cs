using Toolbox.Graphics;
using UnityEngine;

namespace PowerGridInventory.Demo
{
       
    public class SlotHoverEffect : MonoBehaviour
    {
        public RectTransform Icon;
        public float Speed;
        public float MaxBounce = 1.2f;
        public float MinBounce = 0.9f;

        bool Upswing = true;
        float CurrBounce = 1.0f;

        private void OnMouseOver()
        {
            if (Icon == null) return;
            if (Upswing)
            {
                CurrBounce += Speed * Time.deltaTime;
                if (CurrBounce > MaxBounce)
                {
                    CurrBounce = MaxBounce;
                    Upswing = !Upswing;
                }
            }
            else
            {
                CurrBounce -= Speed * Time.deltaTime;
                if (CurrBounce < MinBounce)
                {
                    CurrBounce = MinBounce;
                    Upswing = !Upswing;
                }
            }
            Icon.localScale = Vector3.one * CurrBounce;
        }

        private void OnMouseExit()
        {
            if (Icon == null) return;
            Icon.localScale = Vector3.one;
        }
    }
}
