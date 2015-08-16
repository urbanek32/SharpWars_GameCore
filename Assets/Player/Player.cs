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
	private Unit tempCreator;
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
		//Units units = GetComponentInChildren<Units>();
		//newUnit.transform.parent = units.transform;
	newUnit.GetComponent<WorldObject>().ownerId = this.netId;
	NetworkServer.Spawn(newUnit);
		Unit unitObject = newUnit.GetComponent< Unit >();
		if(unitObject)
		{
			//unitObject.SetBuilding(creator);
			if(spawnPoint != rallyPoint)
			{
				unitObject.StartMove(rallyPoint);
			}
		}
	}

	[Command]
	public void Cmd_TakeDamage(NetworkInstanceId id, int damage)
	{
		Rpc_TakeDamage(id, damage);
	}

	[ClientRpc]
	public void Rpc_TakeDamage(NetworkInstanceId id, int damage)
	{

		GameObject[] go = GameObject.FindGameObjectsWithTag("Tank");
		foreach(GameObject g in go)
		{
			Unit u = g.GetComponent<Unit>();
			Debug.Log(id +"  " +u.netId);
			if(u.netId.Equals(id))
			{
				u.TakeDamage(damage);
				Debug.Log("HIT");
				break;
			}
		}
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
				unit.agent.Stop();
				wo.transform.position = Vector3.MoveTowards(wo.transform.position, newPos, Time.deltaTime * unit.agent.speed);
				wo.transform.rotation = Quaternion.Lerp(wo.transform.rotation, newRot, Time.deltaTime * unit.agent.angularSpeed);
				break;
			}	
		}
		
	}

	public void CreateBuilding(string buildingName, Vector3 buildPoint, Unit creator, Rect playingArea)
	{
		GameObject newBuilding = (GameObject)Instantiate(ResourceManager.GetBuilding(buildingName), buildPoint, new Quaternion());
		if(tempBuilding) Destroy(tempBuilding.gameObject); // prevent spawning ghosty buildings
		tempBuilding = newBuilding.GetComponent< Building >();
		if (tempBuilding) 
		{
			tempCreator = creator;
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
		Buildings buildings = GetComponentInChildren< Buildings >();
		if(buildings)
		{
			tempBuilding.transform.parent = buildings.transform;
		}
		tempBuilding.SetPlayer();
		tempBuilding.SetColliders(true);
		tempCreator.SetBuilding(tempBuilding);
		tempBuilding.StartConstruction();

		tempBuilding.ownerId = this.netId;
		Cmd_StartConstruction(tempBuilding.ownerId, tempBuilding.GetType().ToString(), tempBuilding.transform.position, tempBuilding.transform.rotation);
	}

	[Command]
    public void Cmd_StartConstruction(NetworkInstanceId ownerId, string name, Vector3 pos, Quaternion rot)
	{
		Rpc_StartConstruction(ownerId, name, pos, rot);
	}

	[ClientRpc]
    public void Rpc_StartConstruction(NetworkInstanceId ownerId, string name, Vector3 pos, Quaternion rot)
	{
		if(isLocalPlayer)
			return;

		GameObject newBuilding = (GameObject)Instantiate(ResourceManager.GetBuilding(name), pos, rot);
        newBuilding.GetComponent<Building>().ownerId = ownerId;
	}

	public void CancelBuildingPlacement()
	{
		findingPlacement = false;
		Destroy(tempBuilding.gameObject);
		tempBuilding = null;
		tempCreator = null;
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



}
