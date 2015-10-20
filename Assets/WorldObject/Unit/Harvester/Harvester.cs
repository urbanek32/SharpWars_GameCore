﻿using UnityEngine;
using System.Collections;

using RTS;

public class Harvester : Unit {

	public float capacity;
	public Building resourceStore;
	public float collectionAmount, depositAmount;

	private bool harvesting = false, emptying = false;
	private float currentLoad = 0.0f;
	private ResourceType harvestType;
	private Resource resourceDeposit;
	private float currentDeposit = 0.0f;
    public static readonly float resourceMaxScanDistance = 70.0f;


	// Game Engine methods, all can be overridden by subclass

	protected override void Start()
	{
		base.Start();
		harvestType = ResourceType.Unknown;
	}

	protected override void Update()
	{
		base.Update();

		/*if(agent.remainingDistance < 5.0f)
		{
			moving = false;
			agent.Stop();
			agent.ResetPath();
			agent.avoidancePriority = 60;

		}
		else
		{
			moving = true;
			agent.Resume();
			agent.avoidancePriority = 50;
		}*/

		if(!rotating && !moving)
		{
			if(harvesting || emptying)
			{
                player.Cmd_SetHarvesterArms(netId, true);

				if(harvesting)
				{
					agent.avoidancePriority = 60;
					Collect();
					if(currentLoad >= capacity || resourceDeposit.isEmpty())
					{
						//make sure that we have a whole number to avoid bugs
						//caused by floating point numbers
						currentLoad = Mathf.Floor(currentLoad);
						harvesting = false;
						emptying = true;
	
                        player.Cmd_SetHarvesterArms(netId, false);
						agent.avoidancePriority = 50;
						FindClosestRafinery();
						StartMove(resourceStore.transform.position, resourceStore.gameObject);
					}
				}
				else
				{
					agent.avoidancePriority = 60;
					Deposit();
					if(currentLoad <= 0)
					{
						emptying = false;
                        player.Cmd_SetHarvesterArms(netId, false);

						if(!resourceDeposit.isEmpty() && resourceDeposit)
						{
							harvesting = true;
							agent.avoidancePriority = 50;
							StartMove(resourceDeposit.transform.position, resourceDeposit.gameObject);
						}
					}
				}
			}
		}
	}

	protected override void DrawSelectionBox(Rect selectBox)
	{
		base.DrawSelectionBox(selectBox);

		float percentFull = currentLoad / capacity;
		float maxHeight = selectBox.height - 4;
		float height = maxHeight * percentFull;
		float leftPos = selectBox.x + selectBox.width - 7;
		float topPos = selectBox.y + 2 + (maxHeight - height);
		float width = 5;
		Texture2D resourceBar = ResourceManager.GetResourceHealthBar(harvestType);
		if(resourceBar)
		{
			GUI.DrawTexture(new Rect(leftPos, topPos, width, height), resourceBar);
		}
	}

    protected override bool ShouldMakeDecision()
    {
        if (harvesting || emptying) return false;
        return base.ShouldMakeDecision();
    }


	// Public methods

	public override void SetHoverState(GameObject hoverObject)
	{
		base.SetHoverState(hoverObject);

		// only handle input if owned by a human player and currently selected
		if(player && player.human && currentlySelected)
		{
			if(hoverObject.name != "Ground")
			{
				Resource resource = hoverObject.transform.parent.GetComponentInChildren< Resource >();
				if(resource && !resource.isEmpty())
				{
					player.hud.SetCursorState(CursorState.Harvest);
				}
			}

		}
	}

	public override void MouseClick(GameObject hitObject, Vector3 hitPoint, Player controller)
	{
		base.MouseClick(hitObject, hitPoint, controller);

		// only handle input if owned by a human player and currently selected
		if(player && player.human)
		{
			if(hitObject.name != "Ground")
			{
				Resource resource = hitObject.transform.parent.GetComponentInChildren< Resource >();
				if(resource && !resource.isEmpty())
				{
					// make sure that we select harvester remains selected
					if(player.SelectedObject)
					{
						player.SelectedObject.SetSelection(false, playingArea);
					}

					SetSelection(true, playingArea);
					player.SelectedObject = this;
					StartHarvest(resource);
				}
			}
			else
			{
				StopHarvest();
			}
		}

	}

	public override void SetBuilding(Building creator)
	{
		base.SetBuilding(creator);
		resourceStore = creator;
	}



	// Private Methods

	protected override void StartHarvest(Resource resource)
	{
		resourceDeposit = resource;
		if(currentLoad >= capacity)
		{
			StartMove(resourceStore.transform.position, resourceStore.gameObject);
			harvesting = false;
			emptying = true;
		}
		else
		{
			StartMove(resource.transform.position, resource.gameObject);
			harvesting = true;
			emptying = false;
		}

		//we can only collect one resource at a time, other resources are lost
		if(harvestType == ResourceType.Unknown || harvestType != resource.GetResourceType()) 
		{
			harvestType = resource.GetResourceType();
			currentLoad = 0.0f;
		}


	}

	private void StopHarvest()
	{
		harvesting = false;
		emptying = false;

        player.Cmd_SetHarvesterArms(netId, false);
	}

	private void Collect()
	{
		float collect = collectionAmount * Time.deltaTime;

		//make sure that the harvester cannot collect more than it can carry
		if(currentLoad + collect > capacity) 
		{
			collect = capacity - currentLoad;
		}
        player.Cmd_DecreaseResourceAmount(resourceDeposit.netId, collect);
		currentLoad += collect;
	}

	private void Deposit()
	{
		currentDeposit += depositAmount * Time.deltaTime;
		int deposit = Mathf.FloorToInt(currentDeposit);
		if(deposit >= 1)
		{
			if(deposit > currentLoad)
			{
				deposit = Mathf.FloorToInt(currentLoad);
			}
			currentDeposit -= deposit;
			currentLoad -= deposit;
			ResourceType depositType = harvestType;
			if(harvestType == ResourceType.Ore) 
			{
				depositType = ResourceType.Money;
			}
            player.Cmd_AddResource(player.netId, depositType, deposit);
		}
	}

	private void FindClosestRafinery()
	{
		var builds = player.GetComponentsInChildren<Building>();
		foreach(var b in builds)
		{
			if(b is Rafinery)
			{
				resourceStore = b;
				break;
			}
		}
	}













}
