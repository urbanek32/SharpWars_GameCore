using UnityEngine;
using System.Collections;

public class Worker : Unit {

	public int buildSpeed;

	private Building currentProject;
	private bool building = false;
	private float amountBuilt = 0.0f;

	/*** Game Engine methods, all can be overridden by subclass ***/

	protected override void Start()
	{
		base.Start();
		actions = new string[] { "Rafinery", "WarFactory" };
	}

	protected override void Update()
	{
		base.Update();
	}


	/*** Public Methods ***/

	public override void SetBuilding(Building project)
	{
		base.SetBuilding(project);
		currentProject = project;
		StartMove(currentProject.transform.position, currentProject.gameObject);
		building = true;
	}

	public override void PreformAction(string actionToPreform)
	{
		base.PreformAction(actionToPreform);
		CreateBuilding(actionToPreform);
	}

	public override void StartMove(Vector3 destination)
	{
		base.StartMove(destination);
		amountBuilt = 0.0f;
	}





	/*** Private Methods ***/


	private void CreateBuilding(string buildingName)
	{

	}





}
