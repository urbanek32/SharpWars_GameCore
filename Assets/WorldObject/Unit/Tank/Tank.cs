﻿using UnityEngine;
using System.Collections;

public class Tank : Unit 
{

	private Quaternion aimRotation;


	// Use this for initialization
	protected override void Start () 
	{
		base.Start ();

	}
	
	// Update is called once per frame
	protected override void Update () 
	{
		base.Update ();
		if(aiming)
		{
			transform.rotation = Quaternion.RotateTowards(transform.rotation, aimRotation, weaponAimSpeed);
			CalculateBounds();

			//sometimes it gets stuck exactly 180 degrees out in the calculation and does nothing, this check fixes that
			Quaternion inverseAimRotation = new Quaternion(-aimRotation.x, -aimRotation.y, -aimRotation.z, -aimRotation.w);
			if(transform.rotation == aimRotation || transform.rotation == inverseAimRotation) 
			{
				aiming = false;
			}

		}
	}
	
	protected override void AimAtTarget()
	{
		base.AimAtTarget();
		aimRotation = Quaternion.LookRotation(target.transform.position - transform.position);
	}

	public override bool CanAttack()
	{
		return true;
	}
}
