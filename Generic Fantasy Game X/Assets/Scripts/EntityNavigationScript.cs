using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EntityNavigationScript : MonoBehaviour {

	[SerializeField]
	private Transform goal;

	NavMeshAgent agent;

	// Use this for initialization
	void Start () {
		agent = GetComponent<NavMeshAgent> ();
		//agent.destination = goal.position;
	}
	
	// Update is called once per frame
	void Update () {
		if (goal != null)
			agent.destination = goal.position;
	}

	public void SetDestination(Vector3 goal) {
		agent.destination = goal;
	}
}
