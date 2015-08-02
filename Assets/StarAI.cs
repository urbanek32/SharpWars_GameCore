using UnityEngine;
using System.Collections;

using Pathfinding;
using Pathfinding.RVO;

public class StarAI : MonoBehaviour {

	public GameObject targetPosition;

	private Seeker seeker;
	//private CharacterController controller;
	private RVOController controller;


	//The calculated path
	public Path path;
	//The AI's speed per second
	public float speed = 1000;
	//The max distance from the AI to a waypoint for it to continue to the next waypoint
	public float nextWaypointDistance = 3;
	//The waypoint we are currently moving towards
	private int currentWaypoint = 0;

	// Use this for initialization
	void Start () 
	{
		seeker = GetComponent<Seeker>();
		//controller = GetComponent<CharacterController>();
		controller = GetComponent<RVOController>();

		seeker.StartPath(transform.position, targetPosition.transform.position, OnPathComplete);
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (path == null) 
		{
			//We have no path to move after yet
			return;
		}
		if (currentWaypoint >= path.vectorPath.Count) 
		{
			//Debug.Log ("End Of Path Reached");
			controller.Move(new Vector3(0,0,0));
			return;
		}

		//Direction to the next waypoint
		Vector3 dir = (path.vectorPath[currentWaypoint]-transform.position).normalized;
		dir *= speed * Time.deltaTime;
		controller.Move (dir);
		//Check if we are close enough to the next waypoint
		//If we are, proceed to follow the next waypoint
		if (Vector3.Distance (transform.position,path.vectorPath[currentWaypoint]) < nextWaypointDistance) 
		{
			currentWaypoint++;
			return;
		}
	}

	public void OnPathComplete(Path p)
	{
		Debug.Log ("Yay, we got a path back. Did it have an error? "+p.error);
		if(!p.error)
		{
			path = p;
			currentWaypoint = 0;
		}
	}
}
