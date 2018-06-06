using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PowerGridInventory;

public class UIEquipSlotScript : MonoBehaviour {

	public Text itemDescription;

//	// Use this for initialization
//	void Start () {
//		
//	}
//	
//	// Update is called once per frame
//	void Update () {
//		
//	}

	public void UpdateText() {
		itemDescription.text = "";
		itemDescription.color = Color.white;
	}

	public void UpdateText(string information) {
		itemDescription.text = information;
		itemDescription.color = Color.white;
	}

	public void UpdateText(string information, Color textColour) {
		itemDescription.text = information;
		itemDescription.color = textColour;
	}

	public void UpdateText(PGISlotItem item, PGIModel model, PGISlot slot) {
		switch (item.GetComponent<ItemTypeScript> ().itemType) {
		case EntityStatisticsScript.entitySlots.Potion:
			itemDescription.text = item.GetComponent<PotionUsageScript> ().fluidAmount + "/" + item.GetComponent<PotionUsageScript> ().fluidLimit;
			itemDescription.color = Color.green;
			break;
		case EntityStatisticsScript.entitySlots.RightHand:
			itemDescription.text = "DMG: " + item.GetComponent<WeaponScript> ().damage;
			itemDescription.color = Color.red;
			break;
		default:
			itemDescription.text = "";
			itemDescription.color = Color.white;
			break;

		}
	}

}
