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

    [SyncVar] private Vector3 syncPosition;
    [SyncVar] private Quaternion syncRotation;

    void Start()
    {
        //Debug.Log(transform.position + " " + transform.rotation);
        target = ClientScene.objects[targetId].gameObject.GetComponent<WorldObject>();
        owner = ClientScene.objects[ownerId].gameObject.GetComponent<WorldObject>();
        syncPosition = transform.position;
        syncRotation = owner.transform.rotation;
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
			

		    if (isServer)
		    {
                float positionChange = Time.deltaTime * velocity;
                range -= positionChange;
                transform.position += (positionChange * transform.forward);

		        syncPosition = transform.position;
		        syncRotation = transform.rotation;
		    }
		    else
		    {
                transform.position = Vector3.MoveTowards(transform.position, syncPosition, Time.deltaTime * velocity);
		        transform.rotation = syncRotation;
		    }

		}
		else
		{
			Destroy(gameObject);
		}
	}



	public void SetRange(float range)
	{
		this.range = range + 3;
	}

	public void SetTarget(NetworkInstanceId targetId)
	{
	    this.targetId = targetId;
	}

    public void SetOwner(NetworkInstanceId ownerId)
	{
	    this.ownerId = ownerId;
	}



	private bool HitSomething()
	{
	    return target && target.GetSelectionBounds().Contains(transform.position);
	}

    private void InflictDamage()
	{
		if(target && isServer)
		{
			owner.GetPlayer().Cmd_TakeDamage(target.ownerId, target.netId, damage);
		}
	}
}
