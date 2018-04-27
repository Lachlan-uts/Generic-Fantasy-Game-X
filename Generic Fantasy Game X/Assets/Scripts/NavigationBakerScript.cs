using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
//using UnityEditor.AI;

public class NavigationBakerScript : MonoBehaviour {

	//public GameObject[] surfaces;
	public NavMeshSurface[] surfaces;

	// Use this for initialization
	void Start () {
		//NavMeshBuilder.BuildNavMesh ();
		for (int i = 0; i < surfaces.Length; i++) {
			surfaces [i].BuildNavMesh ();
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
