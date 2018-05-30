using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropManagerScript : MonoBehaviour {

	// serialized private variables
	[SerializeField]
	private List<GameObject> itemTemplates;
	//[SerializeField]
	//private List<GameObject> rareItemTemplates;
	[SerializeField]
	private GameObject lootHolder;
	[SerializeField]
	private GameObject specificLootItem; // Do not update this through inspector

	// private variables
	private int floorNumber;

	// Use this for initialization
	void Start () {
		GameObject levelGenerator;
		if ((levelGenerator = GameObject.Find ("LevelGenerator")) != null) {
			floorNumber = levelGenerator.GetComponent<LevelGenerationScript> ().floorNumber;
		} else {
			floorNumber = 1;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.L)) {
			generateLoot (this.gameObject.transform);
		}
	}

//	public void generateLoot(Transform lootPosition) {
//		GameObject newItem = Instantiate (itemTemplates [Random.Range (0, itemTemplates.Count)]) as GameObject;
//		// create code to dynamically generate the item's properties
//
//
//
//		GameObject newLootHolder = Instantiate (lootHolder, lootPosition.position, Quaternion.identity) as GameObject;
//		newItem.transform.SetParent (newLootHolder.transform);
//		newLootHolder.GetComponent<DropScript> ().lootItem = newItem;
//	}
//
//	public void DropItem(Transform lootPosition, GameObject item) {
//		GameObject newLootHolder = Instantiate (lootHolder, lootPosition.position, Quaternion.identity) as GameObject;
//		item.transform.SetParent (newLootHolder.transform);
//		newLootHolder.GetComponent<DropScript> ().lootItem = item;
//	}
//
//	public void SetDropItem(GameObject item) {
//		specificLootItem = item;
//
//	}
//
//	public void DropSpecificItem(Transform lootPosition) {
//		GameObject newLootHolder = Instantiate (lootHolder, lootPosition.position, Quaternion.identity) as GameObject;
//		specificLootItem.transform.SetParent (newLootHolder.transform);
//		newLootHolder.GetComponent<DropScript> ().lootItem = specificLootItem;
//		specificLootItem = null;
//	}

	public void generateLoot(Transform lootPosition) {
		GameObject newItem = Instantiate (itemTemplates [Random.Range (0, itemTemplates.Count)], lootPosition.position, lootPosition.rotation) as GameObject;
		// Insert code to randomly generate the item's properties
		switch (newItem.GetComponent<ItemTypeScript> ().itemType) {
		case EntityStatisticsScript.entitySlots.Potion:
			newItem.GetComponent<PotionUsageScript> ().GenerateStatistics (floorNumber);
			break;
		case EntityStatisticsScript.entitySlots.RightHand:
			newItem.GetComponent<WeaponScript> ().SetStatistics (floorNumber);
			break;
		default:
			break;
		}
	}


}
