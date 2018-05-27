using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManagerScript : MonoBehaviour {

	// serialized private variables
//	[SerializeField]
//	private GameObject selectedItem { private get { 
//			return selectedItem; 
//		} private set { 
//			selectedItem = value;
//			if (value != null) {
//				// Drop button scriptage
//				buttonDrop.SetActive (true);
//			} else {
//				buttonDrop.SetActive (false);
//			}
//		} }

	[SerializeField]
	private GameObject buttonDrop;

	// public variables
	public GameObject assignedHero;


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void DropItem(GameObject itemToDrop) {
		
	}


}
