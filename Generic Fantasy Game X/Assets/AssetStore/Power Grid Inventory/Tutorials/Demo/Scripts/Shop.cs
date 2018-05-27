using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace PowerGridInventory.Demo
{
    [RequireComponent(typeof(PGIModel))]
    public class Shop : MonoBehaviour
    {
        public enum ShopMode
        {
            Buy,
            Sell,
        }

        public string[] AllowedTags = new string[] { "Player" };
        public PGIView ShoppersView;
        public GameObject ShoppersViewRoot;

        public GameObject ShopViewRoot;
        public Button BuyButton;
        public Button SellButton;
        public Color SelectedOption;
        public Color DeselectedOption;
        public ShopMode Mode;
        public AudioClip BuySound;
        public AudioClip SellSound;
        public AudioClip FailedSound;

        Transform CurrentShopper;
        AudioSource Audio;
        PGIView ShopView;
        bool OldDragState;
        bool OldDropState;
        bool OldWorldDropState;

        void Awake()
        {
            BuyButton.onClick.AddListener(SetBuyMode);
            SellButton.onClick.AddListener(SetSellMode);
            Audio = gameObject.AddComponent<AudioSource>();
        }

        void OnEnable()
        {
            OldDragState = ShoppersView.DisableDragging;
            OldDropState = ShoppersView.DisableDropping;
            OldWorldDropState = ShoppersView.DisableWorldDropping;

            ShoppersView.DisableDragging = true;
            ShoppersView.DisableDropping = true;
            ShoppersView.DisableWorldDropping = true;
            var menu = ShoppersViewRoot.GetComponentInChildren<MenuEventHandlers>();
            menu.ChangeMenuState(true);

            ShopViewRoot.SetActive(true);
            ShopView = ShopViewRoot.GetComponentInChildren<PGIView>();
            

            ShoppersView.OnClickSlot.AddListener(ProcessSale);
            ShopView.OnClickSlot.AddListener(ProcessSale);
        }

        void OnDisable()
        {
            ShoppersView.DisableDragging = OldDragState;
            ShoppersView.DisableDropping = OldDropState;
            ShoppersView.DisableWorldDropping = OldWorldDropState;
            var menu = ShoppersViewRoot.GetComponentInChildren<MenuEventHandlers>();
            menu.ChangeMenuState(false);

            ShopViewRoot.SetActive(false);

            ShoppersView.OnClickSlot.RemoveListener(ProcessSale);
            ShopView.OnClickSlot.RemoveListener(ProcessSale);
        }

        void ProcessSale(PointerEventData data, PGISlot slot)
        {
            if(Mode == ShopMode.Buy && slot.Model == ShopView.Model && slot.Item != null)
            {
                var itemValue = slot.Item.GetComponent<ItemValue>();
                var money = ShoppersView.Model.GetComponent<Money>();
                if (money.Total > itemValue.Value)
                {

                    //buying shop item
                    var clone = Instantiate(slot.Item.gameObject);
                    var itemClone = clone.GetComponent<PGISlotItem>();
                    var clonePickup = clone.GetComponent<PGIPickup>();
                    clonePickup.OnRemove(itemClone, GetComponent<PGIModel>());
                    
                    if (!ShoppersView.Model.Pickup(itemClone))
                    {
                        //no luck with room - cancel purchase
                        Destroy(itemClone);
                        if (FailedSound != null) Audio.PlayOneShot(FailedSound);
                    }
                    else
                    {
                        money.Total -= itemValue.Value;
                        if (BuySound != null) Audio.PlayOneShot(BuySound);
                        itemClone.transform.localPosition = Vector3.zero;
                    }
                }
                else if (FailedSound != null) Audio.PlayOneShot(FailedSound);
            }
            else if(Mode == ShopMode.Sell && slot.Model == ShoppersView.Model && slot.Item != null)
            {
                //selling shopper's item

                //if we can fit it, we'll store it in the shop's inventory - this allows buyback
                var item = ShoppersView.Model.Remove(slot.Item, true);
                if (item != null)
                {
                    //if it doesn't fit, just destroy it
                    if (!ShopView.Model.Pickup(item)) Destroy(item.gameObject);
                    else item.gameObject.transform.localPosition = Vector3.zero;
                    var itemValue = item.GetComponent<ItemValue>();
                    var money = ShoppersView.Model.GetComponent<Money>();
                    money.Total += itemValue.Value;
                    if (SellSound != null) Audio.PlayOneShot(SellSound);
                }
                else if (FailedSound != null) Audio.PlayOneShot(FailedSound);
            }
        }

        /// <summary>
        /// Linked to UI Buy button.
        /// </summary>
        void SetBuyMode()
        {
            Mode = ShopMode.Buy;
        }

        /// <summary>
        /// Linked to UI Sell button.
        /// </summary>
        void SetSellMode()
        {
            Mode = ShopMode.Sell;
        }
        

        void LateUpdate()
        {
            //this is used to make sure the highlighting for the associated buttons
            //stays in sync with the shop's current mode.
            if(Mode == ShopMode.Buy)
            {
                ColorBlock cb = BuyButton.colors;
                cb.normalColor = SelectedOption;
                cb.highlightedColor = SelectedOption;
                BuyButton.colors = cb;

                cb = SellButton.colors;
                cb.normalColor = DeselectedOption;
                cb.highlightedColor = DeselectedOption;
                SellButton.colors = cb;
            }
            else
            {
                ColorBlock cb = BuyButton.colors;
                cb.normalColor = DeselectedOption;
                cb.highlightedColor = DeselectedOption;
                BuyButton.colors = cb;

                cb = SellButton.colors;
                cb.normalColor = SelectedOption;
                cb.highlightedColor = SelectedOption;
                SellButton.colors = cb;
            }
        }
        

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if(collision.gameObject.CompareTag("Player"))
            {
                string tag = collision.gameObject.tag;
                for(int i = 0; i < AllowedTags.Length; i++)
                {
                    if (AllowedTags[i] == tag)
                    {
                        enabled = true;
                    }
                }
            }
        }
    }
}
