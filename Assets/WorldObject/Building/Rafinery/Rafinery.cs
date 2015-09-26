using UnityEngine;
using System.Collections;

public class Rafinery : Building 
{
	
	protected override void Start () 
	{
		base.Start();
		actions = new string[] { "Harvester" };
	}
	
	public override void PreformAction(string actionToPreform)
	{
		base.PreformAction(actionToPreform);
		CreateUnit(actionToPreform);
	}
}
