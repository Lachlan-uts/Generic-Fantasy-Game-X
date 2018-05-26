using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PowerGridInventory.PGISlotItem))]


public class DropScript : MonoBehaviour {

	// public variables
	private GameObject lootItem;

	// serialized private variables
	[SerializeField]
	private Collider dropCollider;

	private Transform owner;

	void Awake() {
		lootItem = this.gameObject;
	}

	// Use this for initialization
	//void Start () {
	//	
	//}
	
	// Update is called once per frame
	//void Update () {
	//	
	//}

//	public void Pickup(Transform inventory) {
//		//lootItem.transform.SetParent (inventory);
//		inventory.GetComponent<PowerGridInventory.PGIModel>().Pickup(lootItem.GetComponent<PowerGridInventory.PGISlotItem>());
//
//		Destroy (this.gameObject);
//
//	}

	public void Drop() {
		dropCollider.enabled = true;
		this.gameObject.layer = 10;
		this.gameObject.GetComponent<Rigidbody> ().isKinematic = false;
		this.transform.position = owner.position;
		owner = null;
	}

	public void Drop(Transform lootPosition) {
		dropCollider.enabled = true;
		this.gameObject.layer = 10;
		this.gameObject.GetComponent<Rigidbody> ().isKinematic = false;
		this.transform.position = lootPosition.position;
	}

	public void Pickup(Transform inventory) {
		inventory.GetComponent<PowerGridInventory.PGIModel>().Pickup(lootItem.GetComponent<PowerGridInventory.PGISlotItem>());
		dropCollider.enabled = false;
		this.gameObject.GetComponent<Rigidbody> ().isKinematic = true;
		this.gameObject.layer = 2;
		owner = this.gameObject.transform.parent;
	}

}
