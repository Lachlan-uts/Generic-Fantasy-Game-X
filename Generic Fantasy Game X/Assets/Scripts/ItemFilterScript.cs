using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using PowerGridInventory;

public class ItemFilterScript : MonoBehaviour {

	public List<EntityStatisticsScript.entitySlots> AllowedTypes;

	public void IsTypeValid(PGISlotItem item, PGIModel model, PGISlot slot) {
		if (AllowedTypes != null && AllowedTypes.Count > 0) {
			var itemType = item.GetComponent<ItemTypeScript>().itemType;
			if (itemType != null && AllowedTypes.Contains (itemType)) {
				return;
			}

			// If the item type is invalid for the slot, inform the model thing are not well
			model.CanPerformAction = false;
		}
	}

	public void IsTypeValid(UnityAction action, PGISlotItem item, PGISlot slot) {
		if (AllowedTypes != null && AllowedTypes.Count > 0) {
			var itemType = item.GetComponent<ItemTypeScript>().itemType;
			if (itemType != null && AllowedTypes.Contains (itemType)) {
				return;
			}

			// If the item type is invalid for the slot, inform the model thing are not well
			slot.Model.CanPerformAction = false;
		}
	}

//	// Use this for initialization
//	void Start () {
//		
//	}
//	
//	// Update is called once per frame
//	void Update () {
//		
//	}


}
