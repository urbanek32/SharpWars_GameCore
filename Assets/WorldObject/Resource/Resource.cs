using UnityEngine;
using System.Collections;

using RTS;
using UnityEngine.Networking;

public class Resource : WorldObject 
{
	// Public variables
	public float capacity;

	// Variables accessible by subclass
	[SyncVar]protected float amountLeft;
	protected ResourceType resourceType;


	// Game Engine methods, all can be overridden by subclass

	protected override void Start()
	{
		base.Start();
		amountLeft = capacity;
		resourceType = ResourceType.Unknown;
	}

	protected override void CalculateCurrentHealth(float lowSplit, float highSplit)
	{
		healthPercentage = amountLeft / capacity;
		healthStyle.normal.background = ResourceManager.GetResourceHealthBar(resourceType);
	}

    protected override bool ShouldMakeDecision()
    {
        return false;
    }


	// Public methods

	public void Remove(float amount)
	{
		amountLeft -= amount;
		if(amountLeft < 0)
		{
			amountLeft = 0;
		}
	}

	public bool isEmpty()
	{
		return amountLeft <= 0;
	}

	public ResourceType GetResourceType()
	{
		return resourceType;
	}







}
