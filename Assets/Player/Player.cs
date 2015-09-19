using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

using RTS;

public class Player : NetworkBehaviour {

	public string username;
	public bool human;
	public HUD hud;
	public WorldObject SelectedObject { get; set; }
	public int startMoney, startMoneyLimit, startPower, startPowerLimit;
	public Material notAllowedMaterial, allowedMaterial;
	public Color teamColor;

	private Dictionary< ResourceType, int > resources, resourceLimits;
	private Building tempBuilding;
	//private Unit tempCreator;
	private bool findingPlacement = false;

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

        // gowno potrzebne, zeby dac graczowi budynek startowy
        if(isLocalPlayer)
            Cmd_SpawnStartBuilding();
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(human)
		{
			hud.SetResourceValues(resources, resourceLimits);

			if(findingPlacement)
			{
				tempBuilding.CalculateBounds();
				if(CanPlaceBuilding())
				{
					tempBuilding.SetTransparentMaterial(allowedMaterial, false);
				}
				else
				{
					tempBuilding.SetTransparentMaterial(notAllowedMaterial, false);
				}
			}
		}

	}



	public void AddResource(ResourceType type, int amount) 
	{
		resources[type] += amount;
	}
	
	public void IncrementResourceLimit(ResourceType type, int amount) 
	{
		resourceLimits[type] += amount;
	}

	[Command] 
	public void Cmd_AddUnit(NetworkInstanceId identity, string unitName, Vector3 spawnPoint, Vector3 rallyPoint, Quaternion rotation /*,Building creator*/)
	{
		GameObject newUnit = (GameObject)Instantiate(ResourceManager.GetUnit(unitName), spawnPoint, rotation);
	    newUnit.GetComponent<WorldObject>().ownerId = this.netId;
	    NetworkServer.Spawn(newUnit);
		Unit unitObject = newUnit.GetComponent< Unit >();

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
            return;

        target.TakeDamage(damage);
        Debug.Log("HIT");


	}

    [Command]
    public void Cmd_SpawnBullet(string name, Vector3 newPos, Quaternion newRot, NetworkInstanceId ownerId, NetworkInstanceId targetId)
    {
        GameObject gameObject = (GameObject)Instantiate(ResourceManager.GetWorldObject(name), newPos, newRot);
        Projectile projectile = gameObject.GetComponentInChildren<Projectile>();
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

		Units units = GetComponentInChildren<Units>();
		Unit[] unitss = units.GetComponentsInChildren<Unit>();

		foreach(Unit u in unitss)
		{
			WorldObject wo = u.GetComponent<WorldObject>();

			if(wo.netId.Equals(id))
			{
				Unit unit = wo.GetComponent<Unit>();
				if(unit.agent.isActiveAndEnabled) unit.agent.Stop();
				wo.transform.position = Vector3.MoveTowards(wo.transform.position, newPos, Time.deltaTime * unit.agent.speed);
				wo.transform.rotation = Quaternion.Lerp(wo.transform.rotation, newRot, Time.deltaTime * unit.agent.angularSpeed);
				break;
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
		newLocation.y = 0;
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

		/*Buildings buildings = GetComponentInChildren< Buildings >();
		if(buildings)
		{
			tempBuilding.transform.parent = buildings.transform;
		}
		tempBuilding.SetPlayer();
		tempBuilding.SetColliders(true);
		tempCreator.SetBuilding(tempBuilding);
		tempBuilding.StartConstruction();*/

   
        string name = tempBuilding.GetType().ToString();
        Transform tr = tempBuilding.transform;
        CancelBuildingPlacement();

        Cmd_StartConstruction(this.netId, name, tr.position, tr.rotation);
	}

	[Command]
    public void Cmd_StartConstruction(NetworkInstanceId ownerId, string name, Vector3 pos, Quaternion rot)
	{
		GameObject newBuilding = (GameObject)Instantiate(ResourceManager.GetBuilding(name), pos, rot);
        Building build = newBuilding.GetComponent<Building>();
        build.ownerId = ownerId;
        build.StartConstruction();
        NetworkServer.Spawn(newBuilding);

		//Rpc_StartConstruction(ownerId, build.netId, name, pos, rot);
	}

	[ClientRpc]
    public void Rpc_StartConstruction(NetworkInstanceId ownerId, NetworkInstanceId buildingId, string name, Vector3 pos, Quaternion rot)
	{
		if(isLocalPlayer)
			return;


        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject go in players)
        {
            Player p = go.GetComponent<Player>();
            if (p.netId.Equals(ownerId))
            {
                Building[] builds = p.GetComponentsInChildren<Building>();
                foreach (Building b in builds)
                {
                    Debug.Log(b.netId);
                    if (b.netId.Equals(buildingId))
                    {
                        Debug.Log("Started Construct");
                        b.StartConstruction();
                        b.SetTransparentMaterial(allowedMaterial, true);
                        break;
                    }
                }
            }
        }

// 		GameObject newBuilding = (GameObject)Instantiate(ResourceManager.GetBuilding(name), pos, rot);
//      Building build = newBuilding.GetComponent<Building>();
//      build.ownerId = ownerId;
//      build.StartConstruction();
//      build.SetTransparentMaterial(allowedMaterial, true);
	}

    [Command]
    public void Cmd_ConstructingBuilding(NetworkInstanceId buildingId, int amount)
    {
        foreach(Building b in GetComponentsInChildren<Building>())
        {
            if(b.netId.Equals(buildingId))
            {
                b.Construct(amount);
                break;
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

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject go in players)
        {
            Player p = go.GetComponent<Player>();
            if(p.netId.Equals(ownerId))
            {
                Building[] builds = p.GetComponentsInChildren<Building>();
                foreach(Building b in builds)
                {
                    if(b.netId.Equals(buildingId))
                    {
                        b.CompleteConstruction();
                        break;
                    }
                }
            }
        }
    }

	public void CancelBuildingPlacement()
	{
		findingPlacement = false;
		Destroy(tempBuilding.gameObject);
		tempBuilding = null;
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
        GameObject newBuilding = (GameObject)Instantiate(ResourceManager.GetBuilding("WarFactory"), transform.position, transform.rotation);
        Building build = newBuilding.GetComponent<Building>();
        build.ownerId = netId;
        build.StartConstruction();
        build.instantbuild = true;
        //build.CompleteConstruction();
        NetworkServer.Spawn(newBuilding);
    }



}
