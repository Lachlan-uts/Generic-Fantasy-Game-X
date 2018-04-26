using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamagePlayer : MonoBehaviour {

	public int damage;

	void Start () {
		
	}

	void Update () {
		
	}


	//INSERT DAMAGE CODE OR MODIFY
	void OnCollisionEnter(Collision other)
	{
		if (other.gameObject.name == "Hero") {
			//other.gameObject.GetComponent<PlayerStatManager> ().damageHealth (damage);
		}
	}

}
