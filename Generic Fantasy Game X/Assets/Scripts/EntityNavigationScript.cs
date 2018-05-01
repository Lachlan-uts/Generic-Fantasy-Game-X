﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using AssemblyCSharp;

public class EntityNavigationScript : MonoBehaviour {

	[SerializeField]
	private Transform goal;

	//debug information to visual the path of the unit.
	private LineRenderer line;
	private NavMeshAgent agent;


	//private enum OrderHierarchy {Low, Medium, High, Player};

	public OrderHierarchy currentOrder { get; private set; }

	// Use this for initialization
	void Start () {
		line = GetComponent<LineRenderer> ();
		agent = GetComponent<NavMeshAgent> ();
		currentOrder = OrderHierarchy.None;
		//agent.destination = goal.position;
	}
	
	// Update is called once per frame
	void Update () {
		if (goal != null) {
			agent.destination = goal.position;
			Debug.Log ("has goal");
			//if (agent.remainingDistance
		}
		//StoppedMovementCheck ();
//		if (agent.velocity.magnitude <= 0.1f) {
//			Debug.Log (agent.remainingDistance);
//		}
		DrawPath (agent.path); // <- use this drawpath to see the path updated in realtime.
	}

	private void DrawPath(NavMeshPath path) {
		
//		Debug.Log (path.corners.Length);
		if (path.corners.Length < 2)
			return;
//		line.SetPositions (path.corners);

		line.positionCount = path.corners.Length;

		line.SetPosition (0, transform.position);
		for (int i = 1; i < path.corners.Length; i++) {
			line.SetPosition (i, path.corners [i]);
		}
//		Debug.Log("set of corners start");
//		foreach (Vector3 corner in path.corners) {
//			Debug.Log (corner);
//		}
//		Debug.Log("set of corners end");
	}

	public void ProximityTrigger(OrderHierarchy newOrder) {
		if (newOrder < currentOrder)
			return;
		agent.ResetPath ();
		Debug.Log ("reset path");
	}

	public void StoppedMovementCheck() {
		//Debug.Log ("in the stopped movement check");
		if (agent.remainingDistance == 0)
			return;
		if (agent.remainingDistance <= 1.0f && agent.velocity.magnitude <= 0.1f) {
			agent.ResetPath ();
			//goal = null;
			Debug.Log ("attempting to remove current pathing");
		}
	}

	/*
	 * We want this to follow an tier of orders.
	 * Where any player commands will superceed any A.I commands.
	 * However A.I commands and player commands need to be able so overwrite previous commands of that level.
	 */
	public void SetDestination(Vector3 goal, GameObject invoker) {

		if (invoker.CompareTag (Camera.main.tag)) {
			line.SetPosition (0, transform.position);
			agent.destination = goal;
		}
		//Debug.Log (invoker.CompareTag(Camera.main.tag));
		if (!invoker.CompareTag(Camera.main.tag) && agent.hasPath) {
			//Debug.Log ("I already have an order");
			return;
		}
		line.SetPosition (0, transform.position);
		agent.destination = goal;
		agent.stoppingDistance = 0.5f;
//		DrawPath (agent.path); // <- use this draw path to see a single set path
	}
}
