using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropScript : MonoBehaviour {

	// public variables
	public GameObject lootItem;

	// Use this for initialization
	//void Start () {
	//	
	//}
	
	// Update is called once per frame
	//void Update () {
	//	
	//}

	public void Pickup(Transform inventory) {
		lootItem.transform.SetParent (inventory);
		Destroy (this.gameObject);
	}
}
