using UnityEngine;
using System.Collections;

using RTS;

public class Harvester : Unit {

	public float capacity;

	private bool harvestring = false, emptying = false;
	private float currentLoad = 0.0f;
	private ResourceType harvestType;


	// Game Engine methods, all can be overridden by subclass

	protected override void Start()
	{
		base.Start();
		harvestType = ResourceType.Unknown;
	}

	protected override void Update()
	{
		base.Update();
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



	// Private Methods

	private void StartHarvest(Resource resource)
	{

	}

	private void StopHarvest()
	{

	}













}
