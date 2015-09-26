using UnityEngine;
using System.Collections;

public class WarFactory : Building {

	// Use this for initialization
	protected override void Start () 
	{
		base.Start();
		actions = new string[] { "Tank" };
	}
	
	// Update is called once per frame
	protected override void Update () 
	{
		base.Update();
	}



	public override void PreformAction(string actionToPerform)
	{
		base.PreformAction(actionToPerform);
		CreateUnit(actionToPerform);
	}
}
