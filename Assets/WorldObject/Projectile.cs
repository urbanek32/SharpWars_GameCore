using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour 
{

	public float velocity = 1;
	public int damage = 1;

	private float range = 1;
	private WorldObject target;
	private WorldObject owner;
	

	// Update is called once per frame
	void Update () 
	{
		if(HitSomething())
		{
			InflictDamage();
			Destroy(gameObject);
		}

		if(range > 0)
		{
			float positionChange = Time.deltaTime * velocity;
			range -= positionChange;
			transform.position += (positionChange * transform.forward);
		}
		else
		{
			Destroy(gameObject);
		}
	}



	public void SetRange(float range)
	{
		this.range = range;
	}

	public void SetTarget(WorldObject target)
	{
		this.target = target;
	}

	public void SetOwner(WorldObject owner)
	{
		this.owner = owner;
	}



	private bool HitSomething()
	{
		if(target && target.GetSelectionBounds().Contains(transform.position))
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	private void InflictDamage()
	{
		if(target)
		{
			//target.TakeDamage(damage);
			owner.GetPlayer().Cmd_TakeDamage(target.netId, damage);
		}
	}
}
