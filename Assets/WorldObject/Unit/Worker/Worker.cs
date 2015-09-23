﻿using UnityEngine;
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
		actions = new string[] { "Rafinery", "WarFactory", "Wonder" };
	}

	protected override void Update()
	{
		base.Update();

		if(!moving && !rotating)
		{
			if(building && currentProject && currentProject.UnderConstruction())
			{
				amountBuilt += buildSpeed * Time.deltaTime;
				int amount = Mathf.FloorToInt(amountBuilt);
				if(amount > 0)
				{
					amountBuilt -= amount;
					//currentProject.Construct(amount);
                    player.Cmd_ConstructingBuilding(currentProject.netId, amount);
					if(!currentProject.UnderConstruction())
					{
						building = false;
					}
				}
			}
		}
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
		building = false;
	}

	public override void MouseClick(GameObject hitObject, Vector3 hitPoint, Player controller)
	{
		bool doBase = true;

		//only handle input if owned by a human player and currently selected
		if(player && player.human && currentlySelected && hitObject && hitObject.name != "Ground") 
		{
			Building building = hitObject.transform.parent.GetComponent< Building >();
			if(building) 
			{
				if(building.UnderConstruction()) 
				{
					SetBuilding(building);
					doBase = false;
				}
			}
		}

		if(doBase)
		{
			base.MouseClick(hitObject, hitPoint, controller);
		}
	}
	
	
	
	
	
	/*** Private Methods ***/
	
	
	private void CreateBuilding(string buildingName)
	{
		Vector3 buildPoint = new Vector3(transform.position.x, transform.position.y, transform.position.z + 10);
		if(player)
		{
			player.CreateBuilding(buildingName, buildPoint, this, playingArea);
		}
	}





}
