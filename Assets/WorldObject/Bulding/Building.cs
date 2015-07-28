﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using RTS;

public class Building : WorldObject 
{
	public float maxBuildProgress;
	public Texture2D rallyPointImage;
	public Texture2D sellImage;

	protected Queue< string > buildQueue;

	private float currentBuildProgress = 0.0f;
	private Vector3 spawnPoint;
	private Vector3 rallyPoint;



	protected override void Awake()
	{
		base.Awake();

		buildQueue = new Queue<string>();
		float spawnX = selectionBounds.center.x + transform.forward.x * selectionBounds.extents.x + transform.forward.x * 10;
		float spawnZ = selectionBounds.center.z + transform.forward.z + selectionBounds.extents.z + transform.forward.z * 10;
		spawnPoint = new Vector3(spawnX, 0.0f, spawnZ);

		rallyPoint = spawnPoint;
	}

	protected override void Start()
	{
		base.Start();
	}

	protected override void Update()
	{
		base.Update();

		ProcessBuildQueue();
	}

	protected override void OnGUI()
	{
		base.OnGUI();
	}

	protected void CreateUnit(string unitName)
	{
		buildQueue.Enqueue(unitName);
	}

	protected void ProcessBuildQueue()
	{
		if(buildQueue.Count > 0)
		{
			currentBuildProgress += Time.deltaTime * ResourceManager.BuildSpeed;
			if(currentBuildProgress > maxBuildProgress)
			{
				if(player)
				{
					player.AddUnit(buildQueue.Dequeue(), spawnPoint, rallyPoint, transform.rotation, this);
				}
				currentBuildProgress = 0.0f;
			}
		}
	}




	public string[] getBuildQueueValues()
	{
		string[] values = new string[buildQueue.Count];
		int pos = 0;
		foreach(string unit in buildQueue)
		{
			values[pos++] = unit;
		}
		return values;
	}

	public float getBuildPercentage()
	{
		return currentBuildProgress / maxBuildProgress;
	}

	public override void SetSelection(bool selected, Rect playingArea)
	{
		base.SetSelection(selected, playingArea);
		if(player)
		{
			RallyPoint flag = player.GetComponentInChildren< RallyPoint >();
			if(selected)
			{
				if(flag && player.human && spawnPoint != ResourceManager.InvalidPosition && rallyPoint != ResourceManager.InvalidPosition)
				{
					flag.transform.localPosition = rallyPoint;
					//flag.transform.forward = transform.forward;
					flag.Enable();
				}
			}
			else
			{
				if(flag && player.human)
				{
					flag.Disable();
				}
			}
		}
	}

	public bool hasSpawnPoint()
	{
		return spawnPoint != ResourceManager.InvalidPosition && rallyPoint != ResourceManager.InvalidPosition;
	}

	public override void SetHoverState(GameObject hoverObject)
	{
		base.SetHoverState(hoverObject);

		// only handle input if owned by a human player and currently selected
		if(player && player.human && currentlySelected)
		{
			if(hoverObject.name == "Ground")
			{
				if(player.hud.GetPreviousCursorState() == CursorState.RallyPoint)
				{
					player.hud.SetCursorState(CursorState.RallyPoint);
				}
			}
		}
	}

	public override void MouseClick(GameObject hitObject, Vector3 hitPoint, Player controller)
	{
		base.MouseClick(hitObject, hitPoint, controller);

		//only handle iput if owned by a human player and currently selected
		if(player && player.human && currentlySelected) 
		{
			if(hitObject.name == "Ground") 
			{
				if((player.hud.GetCursorState() == CursorState.RallyPoint || player.hud.GetPreviousCursorState() == CursorState.RallyPoint) && hitPoint != ResourceManager.InvalidPosition) 
				{
					SetRallyPoint(hitPoint);

					player.hud.SetCursorState(CursorState.PanRight);
					player.hud.SetCursorState(CursorState.Select);
				}
			}
		}
	}

	public void SetRallyPoint(Vector3 position)
	{
		rallyPoint = position;

		if(player && player.human && currentlySelected)
		{
			RallyPoint flag = player.GetComponentInChildren< RallyPoint >();
			if(flag)
			{
				flag.transform.localPosition = rallyPoint;
			}
		}
	}

	public void Sell()
	{
		if(player)
		{
			player.AddResource(ResourceType.Money, sellValue);
			if(currentlySelected)
			{
				SetSelection(false, playingArea);
			}
			Destroy(this.gameObject);
		}
	}







}
