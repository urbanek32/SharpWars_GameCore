using UnityEngine;
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


	// Game Engine methods, all can be overridden by subclass

	protected override void Start()
	{
		base.Start();
		harvestType = ResourceType.Unknown;
	}

	protected override void Update()
	{
		base.Update();

		if(!rotating && !moving)
		{
			if(harvesting || emptying)
			{
				Arms[] arms = GetComponentsInChildren< Arms >();
				foreach(Arms arm in arms)
				{
					arm.GetComponentInChildren<Renderer>().enabled = true;
				}

				if(harvesting)
				{
					Collect();
					if(currentLoad >= capacity || resourceDeposit.isEmpty())
					{
						//make sure that we have a whole number to avoid bugs
						//caused by floating point numbers
						currentLoad = Mathf.Floor(currentLoad);
						harvesting = false;
						emptying = true;
						foreach(Arms arm in arms) 
						{
							arm.GetComponent<Renderer>().enabled = false;
						}
						StartMove(resourceStore.transform.position, resourceStore.gameObject);
					}
				}
				else
				{
					Deposit();
					if(currentLoad <= 0)
					{
						emptying = false;
						foreach(Arms arm in arms) 
						{
							arm.GetComponent<Renderer>().enabled = false;
						}

						if(!resourceDeposit.isEmpty())
						{
							harvesting = true;
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

	private void StartHarvest(Resource resource)
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

		Arms[] arms = GetComponentsInChildren< Arms >();
		foreach(Arms arm in arms)
		{
			arm.GetComponent<Renderer>().enabled = false;
		}
	}

	private void Collect()
	{
		float collect = collectionAmount * Time.deltaTime;

		//make sure that the harvester cannot collect more than it can carry
		if(currentLoad + collect > capacity) 
		{
			collect = capacity - currentLoad;
		}
		resourceDeposit.Remove(collect);
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
			player.AddResource(depositType, deposit);
		}
	}













}
