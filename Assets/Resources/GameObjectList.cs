using UnityEngine;
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
            ScriptManager.RegisterNBCustomFunction("CompareF", "function CompareF(val1, val2)\n return string.format(\"%.1f\", val1) == string.format(\"%.1f\", val2)\n end");
			// Register IsDestinationReached
            ScriptManager.RegisterNBCustomFunction("IsDestinationReached", "function IsDestinationReached()\n if this:isUnit() then\n	local dst = this:GetDestination()\n	local src = this.transform.position\n	if CompareF(dst.x, src.x) and CompareF(dst.z, src.z) then\n		return true\n	else\n		return false\n	end\n else return true\n end \n end");

            // ScanForResources
            ScriptManager.RegisterNBCustomFunction("ScanForResources", "function ScanForResources()\n return this:ScriptGetArrayOfResources() \n end");
            // ScanForEnemies
            ScriptManager.RegisterNBCustomFunction("ScanForEnemies", "function ScanForEnemies()\n return this:ScriptGetArrayOfEnemies() \n end");
            // GetPosition
            ScriptManager.RegisterNBCustomFunction("GetPosition", "function GetPosition()\n return this.transform.position end");
            // GoNorth
            ScriptManager.RegisterNBCustomFunction("GoNorth", "function GoNorth(x_times)\n if x_times == nil then x_times = 1 end local going_north_position12345 = this.transform.position \n going_north_position12345.z = going_north_position12345.z + (5*x_times) \n return going_north_position12345\n end ");
            // GoSouth
            ScriptManager.RegisterNBCustomFunction("GoSouth", "function GoSouth(x_times)\n if x_times == nil then x_times = 1 end local going_south_position12345 = this.transform.position \n going_south_position12345.z = going_south_position12345.z - (5*x_times) \n return going_south_position12345\n end ");
            // GoEast
            ScriptManager.RegisterNBCustomFunction("GoEast", "function GoEast(x_times)\n if x_times == nil then x_times = 1 end local going_east_position12345 = this.transform.position \n going_east_position12345.x = going_east_position12345.x + (5*x_times) \n return going_east_position12345\n end ");
            // GoWest
            ScriptManager.RegisterNBCustomFunction("GoWest", "function GoWest(x_times)\n if x_times == nil then x_times = 1 end local going_west_position12345 = this.transform.position \n going_west_position12345.x = going_west_position12345.x - (5*x_times) \n return going_west_position12345\n end ");
            // DebugPrint
            ScriptManager.RegisterNBCustomFunction("DebugPrint", "function DebugPrint(to_be_printed)\n this:GetPlayer().hud.scriptErrorString = tostring(to_be_printed) .. \"\\n\" .. this:GetPlayer().hud.scriptErrorString \n end");

            // TO DO, przerobić na synchroniczne funkcje
            // Attack Object
            ScriptManager.RegisterNBCustomFunction("Attack", "function Attack(obj)\n this:ScriptAttackObject(obj) \n end");
            // Harvest Resource
            ScriptManager.RegisterNBCustomFunction("Harvest", "function Harvest(obj)\n this:ScriptHarvestResource(obj) \n end");


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
