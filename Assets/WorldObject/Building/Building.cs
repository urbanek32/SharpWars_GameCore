using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

using RTS;

public class Building : WorldObject 
{ 
	public float maxBuildProgress;
	public Texture2D rallyPointImage;
	public Texture2D sellImage;

    public AudioClip finishedJobSound;
    public float finishedJobVolume = 1.0f;

    protected Queue< string > buildQueue;

	private float currentBuildProgress = 0.0f;
	private Vector3 spawnPoint;
	private Vector3 rallyPoint;
	[SyncVar] private bool needsBuilding = false;
    [SyncVar] public bool instantbuild = false;

    public int GetBuildQueueSize()
    {
        return buildQueue.Count;
    }

    public bool IsBuilt
    {
        get
        {
            return needsBuilding;
        }
    }

    protected override void Awake()
	{
		base.Awake();

		buildQueue = new Queue<string>();
		SetSpawnPoint();
	}

	protected override void Start()
	{
		base.Start();
        if(needsBuilding)
        {
            StartConstruction();
            SetTransparentMaterial(player.allowedMaterial, true);
        }

	    if (instantbuild)
	    {
	        CompleteConstruction();
	    }
	}

	protected override void Update()
	{
		base.Update();

        if(hitPoints >= maxHitPoints && UnderConstruction())
        {
            CompleteConstruction();
        }

		ProcessBuildQueue();
	}

    protected override void InitialiseAudio()
    {
        base.InitialiseAudio();

        if (finishedJobVolume < 0.0f) finishedJobVolume = 0.0f;
        if (finishedJobVolume > 1.0f) finishedJobVolume = 1.0f;

        var sounds = new List<AudioClip>();
        var volumes = new List<float>();
        sounds.Add(finishedJobSound);
        volumes.Add(finishedJobVolume);

        audioElement.Add(sounds, volumes);
    }

    protected override void OnGUI()
	{
		base.OnGUI();
		if(needsBuilding)
		{
			DrawBuildProgress();
		}
	}

	protected void CreateUnit(string unitName)
	{
	    if (player.GetResourceAmount(ResourceType.Money) >= cost)
	    {
	        player.Cmd_AddResource(player.netId, ResourceType.Money, -cost);
            buildQueue.Enqueue(unitName);
        }
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
					player.Cmd_AddUnit(player.netId, buildQueue.Dequeue(), spawnPoint, rallyPoint, transform.rotation/*, this*/);

				    if (audioElement != null)
				    {
				        audioElement.Play(finishedJobSound);
				    }
				}
				currentBuildProgress = 0.0f;
			}
		}
	}

    protected override bool ShouldMakeDecision()
    {
        return false;
    }



	private void DrawBuildProgress()
	{
		GUI.skin = ResourceManager.SelectBoxSkin;
		Rect selectBox = WorkManager.CalculateSelectionBox(selectionBounds, playingArea);
		//Draw the selection box around the currently selected object, within the bounds of the main draw area
		GUI.BeginGroup(playingArea);
		CalculateCurrentHealth(0.5f, 0.99f);
		DrawHealthBar(selectBox, "Building ...");
		GUI.EndGroup();
	}

	private void SetSpawnPoint()
	{
		float spawnX = selectionBounds.center.x + transform.forward.x * selectionBounds.extents.x + transform.forward.x * 10;
		float spawnZ = selectionBounds.center.z + transform.forward.z + selectionBounds.extents.z + transform.forward.z * 10;
		spawnPoint = new Vector3(spawnX, 0.0f, spawnZ);
		
		rallyPoint = spawnPoint;
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
            player.Cmd_AddResource(player.netId, ResourceType.Money, sellValue);
			if(currentlySelected)
			{
				SetSelection(false, playingArea);
			}
			Destroy(this.gameObject);
		}
	}

	public void StartConstruction()
	{
		CalculateBounds();
		needsBuilding = true;
		hitPoints = 0;
		SetSpawnPoint();
	}

	public bool UnderConstruction()
	{
		return needsBuilding;
	}

	public void Construct(int amount)
	{
		hitPoints += amount;
		if(hitPoints >= maxHitPoints)
		{
            CompleteConstruction();
            player.Cmd_BuildingCompleted(player.netId, this.netId);
		}
	}

    public void CompleteConstruction()
    {
        hitPoints = maxHitPoints;
        needsBuilding = false;
        RestoreMaterials();
        SetTeamColor();
    }







}
