using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Projectile : NetworkBehaviour 
{

	public float velocity = 1;
	public int damage = 1;

	[SyncVar] private float range = 1;
	private WorldObject target;
	private WorldObject owner;

    [SyncVar]
    private NetworkInstanceId targetId;

    [SyncVar]
    private NetworkInstanceId ownerId;

    void Start()
    {
        Debug.Log(transform.position + " " + transform.rotation);
        target = ClientScene.objects[targetId].gameObject.GetComponent<WorldObject>();
    }

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

	public void SetTarget(NetworkInstanceId targetId)
	{
		//this.target = target;
	    this.targetId = targetId;
	}

	public void SetOwner(WorldObject owner)
	{
		this.owner = owner;
	    ownerId = owner.ownerId;
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
			//owner.GetPlayer().Cmd_TakeDamage(target.ownerId, target.netId, damage);
		}
	}
}
