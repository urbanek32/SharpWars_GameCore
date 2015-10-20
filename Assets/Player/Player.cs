using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

using RTS;
using STL;

public class Player : NetworkBehaviour {

	[SyncVar]public string username;
    //public string token;
	public bool human;
	public HUD hud;
	public WorldObject SelectedObject { get; set; }
	public int startMoney, startMoneyLimit, startPower, startPowerLimit;
	public Material notAllowedMaterial, allowedMaterial;
	public Color teamColor;
    public WebsiteCommunication WebsiteCommunication;

	private Dictionary< ResourceType, int > resources, resourceLimits;
	private Building tempBuilding;
	//private Unit tempCreator;
	private bool findingPlacement = false;
    private GameManager _gameManager;

    public List<Pair<string, string>> scriptList = new List<Pair<string,string>>();
    public GetScriptStatus ScriptFromCloudStatus { get; set; }

	// works like constructor
	void Awake() 
	{
		resources = InitResourceList();
		resourceLimits = InitResourceList();
		AddStartResourceLimits();
		AddStartResources();
	}

	// Use this for initialization
	void Start () 
	{
		hud = GetComponentInChildren< HUD >();
        _gameManager = FindObjectOfType(typeof(GameManager)) as GameManager;

	    if (isLocalPlayer)
	    {
            //dodanie komponentu do komunikacji ze stroną
            gameObject.AddComponent<WebsiteCommunication>();
	        WebsiteCommunication = GetComponent<WebsiteCommunication>();
            
            // gowno potrzebne, zeby dac graczowi budynek startowy
	        Cmd_SpawnStartBuilding();
	        Cmd_SpawnStartUnit();

            
	    }
	    _gameManager.enabled = true;
        _gameManager.LoadDetails();
	}
	
	// Update is called once per frame
	void Update () 
	{
	    if (!human) return;
	    hud.SetResourceValues(resources, resourceLimits);

	    if(findingPlacement)
	    {
	        tempBuilding.CalculateBounds();
	        tempBuilding.SetTransparentMaterial(CanPlaceBuilding() ? allowedMaterial : notAllowedMaterial, false);
	    }

        if (ScriptFromCloudStatus == GetScriptStatus.NotYet && !string.IsNullOrEmpty(ResourceManager.PlayerToken))
        {
            var wc = GetComponentInChildren<WebsiteCommunication>();

            wc.GetScriptsFromCloud(ResourceManager.PlayerName, ResourceManager.PlayerToken, null, WebsiteCommunication.HandleScriptList, this);
            ScriptFromCloudStatus = GetScriptStatus.Downloading;
        }
	}



	public void AddResource(ResourceType type, int amount) 
	{
		resources[type] += amount;
	}

    [Command]
    public void Cmd_AddResource(NetworkInstanceId playerId, ResourceType type, int amount)
    {
        var player = ClientScene.objects[playerId].gameObject.GetComponent<Player>();
        player.AddResource(type, amount);
    }
	
	public void IncrementResourceLimit(ResourceType type, int amount) 
	{
		resourceLimits[type] += amount;
	}

	[Command] 
	public void Cmd_AddUnit(NetworkInstanceId identity, string unitName, Vector3 spawnPoint, Vector3 rallyPoint, Quaternion rotation /*,Building creator*/)
	{
		var newUnit = (GameObject)Instantiate(ResourceManager.GetUnit(unitName), spawnPoint, rotation);
        newUnit.GetComponent<WorldObject>().ownerId = identity;
	    NetworkServer.Spawn(newUnit);
		var unitObject = newUnit.GetComponent< Unit >();

		if(unitObject)
		{
			if(spawnPoint != rallyPoint)
			{
			    unitObject.BirthMoveTarget = rallyPoint;
			}
		}
	}

	[Command]
    public void Cmd_TakeDamage(NetworkInstanceId ownerId, NetworkInstanceId targetId, int damage)
	{
        Rpc_TakeDamage(ownerId, targetId, damage);
	}

	[ClientRpc]
    public void Rpc_TakeDamage(NetworkInstanceId ownerId, NetworkInstanceId targetId, int damage)
	{
        var target = ClientScene.objects[targetId].gameObject.GetComponent<WorldObject>();

	    if (target is Resource)
	    {
	        return;
	    }

        target.TakeDamage(damage);
        Debug.Log("HIT");
	}

    [Command]
    public void Cmd_SpawnBullet(string name, Vector3 newPos, Quaternion newRot, NetworkInstanceId ownerId, NetworkInstanceId targetId)
    {
        var gameObject = (GameObject)Instantiate(ResourceManager.GetWorldObject(name), newPos, newRot);
        var projectile = gameObject.GetComponentInChildren<Projectile>();
        projectile.SetRange(20);
        projectile.SetTarget(targetId);
        projectile.SetOwner(ownerId);
        NetworkServer.Spawn(gameObject);
    }

	[Command]
	public void Cmd_MoveUnit(NetworkInstanceId id, Vector3 newPos, Quaternion newRot)
	{
		Rpc_MoveUnit(id, newPos, newRot);
	}

	[ClientRpc]
	public void Rpc_MoveUnit(NetworkInstanceId id, Vector3 newPos, Quaternion newRot)
	{
		if(isLocalPlayer)
			return;

	    NetworkIdentity netObj = null;
	    if (ClientScene.objects.TryGetValue(id, out netObj))
	    {
	        var unit = netObj.gameObject.GetComponent<Unit>();
	        if (unit)
	        {
	            if (unit.agent.isActiveAndEnabled)
	            {
	                unit.agent.Stop();
	            }

	            unit.transform.position = Vector3.MoveTowards(unit.transform.position, newPos, Time.deltaTime * unit.agent.speed);
                unit.transform.rotation = Quaternion.Lerp(unit.transform.rotation, newRot, Time.deltaTime * unit.agent.angularSpeed); 
	        }
	    }
	}

	public void CreateBuilding(string buildingName, Vector3 buildPoint, Unit creator, Rect playingArea)
	{
		GameObject newBuilding = (GameObject)Instantiate(ResourceManager.GetBuilding(buildingName), buildPoint, new Quaternion());
		//if(tempBuilding) Destroy(tempBuilding.gameObject); // prevent spawning ghosty buildings
		tempBuilding = newBuilding.GetComponent< Building >();
		if (tempBuilding) 
		{
			//tempCreator = creator;
			findingPlacement = true;
			tempBuilding.SetTransparentMaterial(notAllowedMaterial, true);
			tempBuilding.SetColliders(false);
			tempBuilding.SetPlayingArea(playingArea);
            tempBuilding.hitPoints = 0;
		} 
		else 
		{
			Destroy(newBuilding); // ?useless?
		}
	}

	public bool IsFindingBuildingLocation()
	{
		return findingPlacement;
	}

	public void FindBuildingLocation()
	{
		Vector3 newLocation = WorkManager.FindHitPoint(Input.mousePosition);
		//newLocation.y = 0;
		tempBuilding.transform.position = newLocation;
	}

	public bool CanPlaceBuilding()
	{
		bool canPlace = true;

		Bounds placeBounds = tempBuilding.GetSelectionBounds();
		//shorthand for the coordinates of the center of the selection bounds
		float cx = placeBounds.center.x;
		float cy = placeBounds.center.y;
		float cz = placeBounds.center.z;
		//shorthand for the coordinates of the extents of the selection box
		float ex = placeBounds.extents.x;
		float ey = placeBounds.extents.y;
		float ez = placeBounds.extents.z;
		
		//Determine the screen coordinates for the corners of the selection bounds
		List< Vector3 > corners = new List< Vector3 >();
		corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx+ex,cy+ey,cz+ez)));
		corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx+ex,cy+ey,cz-ez)));
		corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx+ex,cy-ey,cz+ez)));
		corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx-ex,cy+ey,cz+ez)));
		corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx+ex,cy-ey,cz-ez)));
		corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx-ex,cy-ey,cz+ez)));
		corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx-ex,cy+ey,cz-ez)));
		corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx-ex,cy-ey,cz-ez)));
		
		foreach(Vector3 corner in corners) 
		{
			GameObject hitObject = WorkManager.FindHitObject(corner);
			if(hitObject && hitObject.name != "Ground") 
			{
				WorldObject worldObject = hitObject.transform.parent.GetComponent< WorldObject >();
				if(worldObject && placeBounds.Intersects(worldObject.GetSelectionBounds()))
				{
					canPlace = false;
				}
			}
		}

		return canPlace;
		
	}

	public void StartConstruction()
	{
		findingPlacement = false;

        CancelBuildingPlacement();
        Cmd_StartConstruction(this.netId, tempBuilding.GetType().ToString(), tempBuilding.transform.position, tempBuilding.transform.rotation);
	}

	[Command]
    public void Cmd_StartConstruction(NetworkInstanceId ownerId, string name, Vector3 pos, Quaternion rot)
	{
		var newBuilding = (GameObject)Instantiate(ResourceManager.GetBuilding(name), pos, rot);
        var build = newBuilding.GetComponent<Building>();
        build.ownerId = ownerId;
        build.StartConstruction();
        NetworkServer.Spawn(newBuilding);
	}

    [Command]
    public void Cmd_ConstructingBuilding(NetworkInstanceId buildingId, int amount)
    {
        NetworkIdentity netObj = null;
        if (ClientScene.objects.TryGetValue(buildingId, out netObj))
        {
            var building = netObj.gameObject.GetComponent<Building>();
            if (building)
            {
                building.Construct(amount);
            }
        }
    }

    [Command]
    public void Cmd_BuildingCompleted(NetworkInstanceId ownerId, NetworkInstanceId buildingId)
    {
        Rpc_BuildingCompleted(ownerId, buildingId);
    }

    [ClientRpc]
    public void Rpc_BuildingCompleted(NetworkInstanceId ownerId, NetworkInstanceId buildingId)
    {
        NetworkIdentity netObj = null;
        if (ClientScene.objects.TryGetValue(buildingId, out netObj))
        {
            var building = netObj.gameObject.GetComponent<Building>();
            if (building)
            {
                building.CompleteConstruction();
            }
        }
    }

	public void CancelBuildingPlacement()
	{
		findingPlacement = false;
		Destroy(tempBuilding.gameObject);
		//tempBuilding = null;
		//tempCreator = null;
	}

    [Command]
    public void Cmd_SetHarvesterArms(NetworkInstanceId objId, bool state)
    {
        Rpc_SetHarvesterArms(objId, state);
    }

    [ClientRpc]
    public void Rpc_SetHarvesterArms(NetworkInstanceId objId, bool state)
    {
        var harvester = ClientScene.objects[objId].gameObject.GetComponent<Harvester>();
        var arms = harvester.GetComponentsInChildren<Arms>();
        foreach (var arm in arms)
        {
            arm.GetComponent<Renderer>().enabled = state;
        }
    }

    [Command]
    public void Cmd_DecreaseResourceAmount(NetworkInstanceId resourceId, float amount)
    {
        NetworkIdentity resourceObject;
        if (ClientScene.objects.TryGetValue(resourceId, out resourceObject))
        {
            var resource = resourceObject.gameObject.GetComponent<Resource>();
            if (resource)
            {
                resource.Remove(amount);
            }
        }
    }

    public bool IsDead()
    {
        var buildings = GetComponentsInChildren<Building>();
        var units = GetComponentsInChildren<Unit>();
        if (buildings != null && buildings.Length > 0) return false;
        return units == null || units.Length <= 0;
    }

    public int GetResourceAmount(ResourceType type)
    {
        return resources[type];
    }

    [Command]
    public void Cmd_PlayerWin(NetworkInstanceId playerId, string wonBy)
    {
        Rpc_PlayerWin(playerId, wonBy);
    }

    [ClientRpc]
    public void Rpc_PlayerWin(NetworkInstanceId playerId, string wonBy)
    {
        Debug.LogFormat("Called {0}  Player: {1} won by: {2}", netId, playerId, wonBy);
        
        hud.DisplayResultScreen(playerId, wonBy);
    }

    [Command]
    public void Cmd_SetPlayerName(NetworkInstanceId playerId, string newName)
    {
        ClientScene.objects[playerId].gameObject.GetComponent<Player>().username = newName;
    }
	
	
	
	
	
	private Dictionary< ResourceType, int > InitResourceList()
	{
		Dictionary< ResourceType, int > list = new Dictionary< ResourceType, int >();
		list.Add(ResourceType.Money, 0);
		list.Add(ResourceType.Power, 0);
		return list;
	}

	private void AddStartResourceLimits()
	{
		IncrementResourceLimit(ResourceType.Money, startMoneyLimit);
		IncrementResourceLimit(ResourceType.Power, startPowerLimit);
	}

	private void AddStartResources() 
	{
		AddResource(ResourceType.Money, startMoney);
		AddResource(ResourceType.Power, startPower);
	}

    [Command]
    private void Cmd_SpawnStartBuilding()
    {
        var newBuilding = (GameObject)Instantiate(ResourceManager.GetBuilding("WarFactory"), transform.position, transform.rotation);
        var build = newBuilding.GetComponent<Building>();
        build.ownerId = netId;
        build.StartConstruction();
        build.instantbuild = true;
        //build.CompleteConstruction();
        NetworkServer.Spawn(newBuilding);
    }

    [Command]
    private void Cmd_SpawnStartUnit()
    {
        var pos = transform.position + new Vector3(10, 0, 0);
        Cmd_AddUnit(this.netId, "Worker", pos, pos, transform.rotation);
    }


}
