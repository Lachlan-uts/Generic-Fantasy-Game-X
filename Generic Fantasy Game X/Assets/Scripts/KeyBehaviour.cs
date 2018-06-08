using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyBehaviour : MonoBehaviour {

	// Use this for initialization
	public float speed = 10f;
	public GameObject key;
    DataCollector dataCollector;

    
    void Update ()
    {
        transform.Rotate(Vector3.up, speed * Time.deltaTime);
        dataCollector = GameObject.FindGameObjectWithTag("GameManagers").GetComponent<DataCollector>();

    }

    void OnCollisionEnter(Collision col){

    	if(col.gameObject.tag == "Hero"){
            dataCollector.AddKey();
    		Destroy(key);
    	}
    }
}