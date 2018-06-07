using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyBehaviour : MonoBehaviour {

	// Use this for initialization
	public float speed = 10f;
	public GameObject key;
    
    
    void Update ()
    {
        transform.Rotate(Vector3.up, speed * Time.deltaTime);
    }

    void OnCollisionEnter(Collision col){

    	if(col.gameObject.tag == "Hero"){
    		HatchScript.keysFound++;
    		Destroy(key);
    	}
    }
}