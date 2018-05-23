using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lighting : MonoBehaviour {

	public Light[] lights;
	private bool on = true;
	

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		
		if(Input.GetKeyDown(KeyCode.Space) && on){
			for(int i = 0; i < lights.Length; i++){
				lights[i].enabled = false;
				on = false;
			}
		// if(Input.GetKeyDown(KeyCode.Space) && !on) {
		// 	for(int i = 0; i < lights.Length; i++){
		// 		lights[i].enabled = true;
		// 		on = true;
		// 	}
		// }

		}
	}
}
