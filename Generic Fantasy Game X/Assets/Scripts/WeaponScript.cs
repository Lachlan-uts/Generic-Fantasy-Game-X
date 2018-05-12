using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponScript : MonoBehaviour {


	private Rigidbody weaponRGBDY;
	private Mesh weaponM;
	private MeshRenderer rend;
	private GameObject equiper;

	// Use this for initialization
	void Start () {
		weaponRGBDY = GetComponent<Rigidbody> ();
		rend = GetComponent<MeshRenderer> ();
		equiper = null;
		//SetState (this.transform.parent);
	}

	void Update () {
//		StartCoroutine ("ReturnToOriginPosition");
//		StartCoroutine ("ReturnToOriginRotation");
//		if (Input.GetKeyDown(KeyCode.Mouse2)) {
//			StartCoroutine ("AttackTarget");
//		}
	}

	void OnCollisionEnter(Collision collision) {
		if (collision.collider.gameObject == equiper) {
			Debug.Log ("can't hit myself");
			return;
		}
		if (collision.collider.gameObject.layer == LayerMask.NameToLayer ("Units")) {
			Debug.Log ("Trigger the attack/kill!");
		}
	}

	void OnTriggerEnter(Collider other) {
		if (other.gameObject.layer == LayerMask.NameToLayer ("Units")) {
			Debug.Log ("Trigger the attack/kill!");
		}
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
		equiper = GetComponentInParent<EntityTargetScript> ().gameObject;
		weaponRGBDY.useGravity = false;
		//weaponRGBDY.isKinematic = true;
		weaponRGBDY.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
	}

	private void SetUnequiped() {
		weaponRGBDY.useGravity = true;
		weaponRGBDY.constraints = RigidbodyConstraints.None;
	}

	private void SetInInventory() {
		rend.enabled = false;
	}

	private IEnumerator ReturnToOriginPosition() {
		//Debug.Log ("where it currently is " + transform.localPosition);
		if (transform.localPosition != Vector3.zero) {
			//Debug.Log ("gotta get back");
			if (weaponRGBDY.velocity.magnitude <= 0.1) {
				yield return null;
			}
			//Debug.Log ("added force" + (Vector3.zero - transform.localPosition));
			weaponRGBDY.AddRelativeForce ((Vector3.zero - transform.localPosition), ForceMode.Force);
//			if (weaponRGBDY.velocity.magnitude >= 4) {
//				weaponRGBDY.velocity = Vector3.zero;
//				weaponRGBDY.AddForce ((Vector3.zero - weaponRGBDY.velocity.normalized), ForceMode.Force);
//				weaponRGBDY.drag = weaponRGBDY.drag
//			}
			//weaponRGBDY.AddTorque ((Vector3.zero - transform.localPosition), ForceMode.Force);
		}
		yield return null;
	}

	private IEnumerator ReturnToOriginRotation() {
		//transform.rotation = Quaternion.Slerp (transform.rotation, Quaternion.Euler (0,0,0), Time.deltaTime * 5.0f);
		yield return null;
	}

	private IEnumerator AttackTarget() {
		weaponRGBDY.AddRelativeForce (Vector3.forward*16, ForceMode.Force);
		//weaponRGBDY.AddRelativeTorque (Vector3.up * 4);
		yield return null;
	}
}
