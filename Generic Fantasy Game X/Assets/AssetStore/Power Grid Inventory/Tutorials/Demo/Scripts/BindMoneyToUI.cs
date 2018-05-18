using UnityEngine;
using UnityEngine.UI;

namespace PowerGridInventory.Demo
{
    /// <summary>
    /// Simple binding tool for making the UI match a money source.
    /// </summary>
    [ExecuteInEditMode]
    public class BindMoneyToUI : MonoBehaviour
    {
        public Money Source;
        public Text Output;

        int Cache = int.MaxValue;
        
        void Update()
        {
            if (Cache != Source.Total)
            {
                Cache = Source.Total;
                Output.text = Cache.ToString();
            }
        }
    }
}
