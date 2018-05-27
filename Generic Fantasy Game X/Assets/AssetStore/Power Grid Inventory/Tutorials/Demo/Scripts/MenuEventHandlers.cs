using UnityEngine;
using UnityEngine.SceneManagement;

namespace PowerGridInventory.Demo
{
    public class MenuEventHandlers : MonoBehaviour
    {
        public Animator Anim;
        public string AnimationStateName = "Open";
        public Shop LinkedShopMenu;

        public void ChangeMenuState(bool state)
        {
            var roots = SceneManager.GetActiveScene().GetRootGameObjects();
            for(int i = 0; i < roots.Length; i++)
                roots[i].BroadcastMessage("MenuStateChanged", state, SendMessageOptions.DontRequireReceiver);

            if (Anim != null) Anim.SetBool(AnimationStateName, state);
            if (LinkedShopMenu != null && !state) LinkedShopMenu.enabled = false;
        }
    }
}
