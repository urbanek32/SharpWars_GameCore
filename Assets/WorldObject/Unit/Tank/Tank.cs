using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

using RTS;

public class Tank : Unit 
{

	private Quaternion aimRotation;



	// Use this for initialization
	protected override void Start () 
	{
		base.Start ();

        enemyMaxScanDistance = 100.0f;


	}
	
	// Update is called once per frame
	protected override void Update () 
	{
		base.Update();
	
		if(aiming)
		{
			transform.rotation = Quaternion.RotateTowards(transform.rotation, aimRotation, weaponAimSpeed);
            player.Cmd_MoveUnit(netId, transform.position, transform.rotation);
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

	protected override void UseWeapon()
	{
		base.UseWeapon();
		Vector3 spawnPoint = transform.position;
		spawnPoint.x += (2.1f * transform.forward.x);
		spawnPoint.y += 2.4f;
		spawnPoint.z += (2.1f * transform.forward.z);

        player.Cmd_SpawnBullet("TankProjectile", spawnPoint, transform.rotation, netId, target.netId);

		/*GameObject gameObject = (GameObject)Instantiate(ResourceManager.GetWorldObject("TankProjectile"), spawnPoint, transform.rotation);
        Debug.Log("spawn na: " + spawnPoint);
        Debug.Log("rot na: " + transform.rotation);
		Projectile projectile = gameObject.GetComponentInChildren< Projectile >();
		projectile.SetRange( 20 );
		projectile.SetTarget(target);
		projectile.SetOwner(this);
        NetworkServer.Spawn(gameObject);*/
	}




	public override bool CanAttack()
	{
		return true;
	}
}
