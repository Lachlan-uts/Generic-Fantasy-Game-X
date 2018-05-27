using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponScript : MonoBehaviour {

	[SerializeField]
	private Collider weaponCollider;
	private Rigidbody weaponRGBDY;
	private Mesh weaponM;
	private MeshRenderer rend;
	private GameObject equiper;

	// public variables
	public int damage { get; private set; }

	// Use this for initialization
	void Start () {
		weaponRGBDY = GetComponent<Rigidbody> ();
		rend = GetComponent<MeshRenderer> ();
		if (!weaponCollider) {
			weaponCollider = GetComponent<Collider> ();
		}
		equiper = null;
		//SetState (this.transform.parent);
		damage = 10;
		//workaround for enabling the weapon collider until the setstate is updated.
		weaponCollider.enabled = false;
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
			//Debug.Log ("Trigger the attack/kill!");
			Debug.Log (this.gameObject.name + " Hit " + other.gameObject.name);
			other.gameObject.GetComponent<Collider>().enabled = false;
			GetComponentInParent<EntityTargetScript> ().targetedEntity = null;
			//Will use the navigation script for now, but in future will need to be updated to use the entity controller/statistics script as that makes more sense.
			other.gameObject.GetComponent<EntityTargetScript> ().Die();
		}
	}

	void DealDamage(GameObject target) {
		target.GetComponent<EntityStatisticsScript> ().TakeDamage (damage);
	}

	public void SetStatistics(int floorNumber) {
		damage = floorNumber * 4 + Random.Range (1, (int)(5 * (floorNumber / 2)));
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

	public void ToggleCollider(bool state) {
		weaponCollider.enabled = state;
	}
}
