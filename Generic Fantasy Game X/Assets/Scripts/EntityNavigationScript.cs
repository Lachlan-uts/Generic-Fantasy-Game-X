using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EntityNavigationScript : MonoBehaviour {

	[SerializeField]
	private Transform goal;

	//debug information to visual the path of the unit.
	private LineRenderer line;
	private NavMeshAgent agent;

	// Use this for initialization
	void Start () {
		line = GetComponent<LineRenderer> ();
		agent = GetComponent<NavMeshAgent> ();
		//agent.destination = goal.position;
	}
	
	// Update is called once per frame
	void Update () {
		if (goal != null)
			agent.destination = goal.position;
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

	public void SetDestination(Vector3 goal) {
		line.SetPosition (0, transform.position);
		agent.destination = goal;
//		DrawPath (agent.path); // <- use this draw path to see a single set path

	}
}
