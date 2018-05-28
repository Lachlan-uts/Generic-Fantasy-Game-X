using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lighting : MonoBehaviour {

	public Light[] lights;

	//set lights to disabled on generation
	void Awake() {
		foreach (var light in lights) {
			light.enabled = false;
		}
	}
	//set lights to enable on encountering a hero for the first time.
	void OnTriggerEnter(Collider other){
		if(other.CompareTag("Hero")){
			foreach (var light in lights) {
				light.enabled = true;
			}
		}
	}
}