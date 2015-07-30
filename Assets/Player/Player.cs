using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using RTS;

public class Player : MonoBehaviour {

	public string username;
	public bool human;
	public HUD hud;
	public WorldObject SelectedObject { get; set; }
	public int startMoney, startMoneyLimit, startPower, startPowerLimit;
	public Material notAllowedMaterial, allowedMaterial;

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
        ScriptManager.Init();
        //Register PanzerVor
        ScriptManager.RegisterCustomFunction("PanzerVor", "function PanzerVor(pos)\n unit:StartMove(pos)\n end");
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

	public void AddUnit(string unitName, Vector3 spawnPoint, Vector3 rallyPoint, Quaternion rotation, Building creator)
	{
		Units units = GetComponentInChildren< Units >();
		GameObject newUnit = (GameObject)Instantiate(ResourceManager.GetUnit(unitName), spawnPoint, rotation);
		newUnit.transform.parent = units.transform;
		Unit unitObject = newUnit.GetComponent< Unit >();
		if(unitObject)
		{
			unitObject.SetBuilding(creator);
			if(spawnPoint != rallyPoint)
			{
				unitObject.StartMove(rallyPoint);
			}
		}
	}

	public void CreateBuilding(string buildingName, Vector3 buildPoint, Unit creator, Rect playingArea)
	{
		GameObject newBuilding = (GameObject)Instantiate(ResourceManager.GetBuilding(buildingName), buildPoint, new Quaternion());
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
			Destroy(newBuilding);
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



}
