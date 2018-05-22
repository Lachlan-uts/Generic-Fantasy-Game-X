using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lighting : MonoBehaviour {

	public Light[] lights;
	

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		
		if(Input.GetKeyDown(KeyCode.Space)){
			for(int i = 0; i < lights.Length; i++){
				lights[i].enabled = false;
			}
		}

	}
}
