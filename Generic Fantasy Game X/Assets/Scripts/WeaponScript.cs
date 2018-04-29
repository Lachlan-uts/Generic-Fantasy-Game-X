using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponScript : MonoBehaviour {


	private Rigidbody weaponRGBDY;
	private Mesh weaponM;
	private MeshRenderer rend;

	// Use this for initialization
	void Start () {
		weaponRGBDY = GetComponentInChildren<Rigidbody> ();
		rend = GetComponentInChildren<MeshRenderer> ();
		SetState (this.transform.parent);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private void SetState(Transform owner = null) {
		if (owner == null) {
			SetUnequiped ();
		} else if (this.gameObject.transform.parent.name == "EquipedItems") {
			SetEquiped ();
		} else {
			SetInInventory ();
		}
	}

	private void SetEquiped() {
		weaponRGBDY.useGravity = false;
		weaponRGBDY.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
	}

	private void SetUnequiped() {
		weaponRGBDY.useGravity = true;
		weaponRGBDY.constraints = RigidbodyConstraints.None;
	}

	private void SetInInventory() {
		rend.enabled = false;
	}
}
