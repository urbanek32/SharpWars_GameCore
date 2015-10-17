﻿using UnityEngine;
using System.Collections;

using RTS;

public class GameObjectList : MonoBehaviour 
{
	public GameObject[] buildings;
	public GameObject[] units;
	public GameObject[] worldObjects;
	public GameObject player;

	private static bool created = false;

	void Awake()
	{
		if(!created)
		{
			DontDestroyOnLoad(transform.gameObject);
			ResourceManager.SetGameObjectList(this);
			created = true;

			ScriptManager.Init();
            // Non-blocking functions
            // Register CompareF(val1f, val2f)
            // Compares 2 numbers up to 1 digit after dot
            ScriptManager.RegisterNBCustomFuntion("CompareF", "function CompareF(val1, val2)\n return string.format(\"%.1f\", val1) == string.format(\"%.1f\", val2)\n end");
			// Register IsDestinationReached
            ScriptManager.RegisterNBCustomFuntion("IsDestinationReached", "function IsDestinationReached()\n if this:isUnit() then\n	local dst = this:GetDestination()\n	local src = this.transform.position\n	if CompareF(dst.x, src.x) and CompareF(dst.z, src.z) then\n		return true\n	else\n		return false\n	end\n else return true\n end \n end");
			
            // TO DO, przerobić na synchroniczną funkcję
            // Attack Object
            ScriptManager.RegisterNBCustomFuntion("Attack", "function Attack(obj)\n this:ScriptAttackObject(obj) \n end");

            // Blocking functions
            // Register PanzerVor
            ScriptManager.RegisterCustomFunction("PanzerVor", "function PanzerVor(pos)\n if this:isUnit() then this:StartMove(pos)\n end end", "IsDestinationReached");
		} 
		else 
		{
			Destroy(this.gameObject);
		}
	}

	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}



	public GameObject GetBuilding(string name)
	{
		for(int i = 0; i < buildings.Length; i++)
		{
			Building building = buildings[i].GetComponent< Building >();
			if(building && building.name == name) 
			{
				return buildings[i];
			}
		}
		return null;
	}

	public GameObject GetUnit(string name)
	{
		for(int i = 0; i < units.Length; i++)
		{
			Unit unit = units[i].GetComponent< Unit >();
			if(unit && unit.name == name)
			{
				return units[i];
			}
		}
		return null;
	}

	public GameObject GetWorldObject(string name)
	{
		foreach(GameObject worldObject in worldObjects)
		{
			if(worldObject.name == name)
			{
				return worldObject;
			}
		}
		return null;
	}

	public GameObject GetPlayerObject()
	{
		return player;
	}

	public Texture2D GetBuildImage(string name)
	{
		for(int i = 0; i < buildings.Length; i++) 
		{
			Building building = buildings[i].GetComponent< Building >();
			if(building && building.name == name) return building.buildImage;
		}

		for(int i = 0; i < units.Length; i++) 
		{
			Unit unit = units[i].GetComponent< Unit >();
			if(unit && unit.name == name) return unit.buildImage;
		}
		return null;
	}




}
