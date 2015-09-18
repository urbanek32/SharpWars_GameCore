using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

using RTS;

public class Unit : WorldObject {

	public float moveSpeed, rotateSpeed;

	protected bool moving, rotating;

	private Vector3 destination;
	private Quaternion targetRotation;
	private GameObject destinationTarget;
	private Vector3 startPos;

	public NavMeshAgent agent;

    [SyncVar] public Vector3 BirthMoveTarget = Vector3.zero;
    private bool _performBirthMove = true;


	protected override void Awake() 
	{
		base.Awake();
	}
	
	protected override void Start () 
	{
		base.Start();
		agent = GetComponent<NavMeshAgent>();

        if (BirthMoveTarget != Vector3.zero)
        {
            _performBirthMove = true;
        }
	}
	
	protected override void Update () 
	{
		base.Update();

        if (_performBirthMove)
	    {
            _performBirthMove = false;
            StartMove(BirthMoveTarget);
	    }

		if(rotating)
		{
			//if(agent.velocity.sqrMagnitude <= 0)
				//TurnToTarget();

		}
		else if(moving)
		{
		    MakeMove();

		}

		CalculateBounds(); // navmesh
	}
	
	protected override void OnGUI() 
	{
		base.OnGUI();
	}


	 

	private void TurnToTarget()
	{
		//agent.updateRotation = false;
		transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotateSpeed);
		CalculateBounds();

		//sometimes it gets stuck exactly 180 degrees out in the calculation and does nothing, this check fixes that
		Quaternion inverseTargetRotation = new Quaternion(-targetRotation.x, -targetRotation.y, -targetRotation.z, -targetRotation.w);
		if(transform.rotation == targetRotation || transform.rotation == inverseTargetRotation) 
		{
			rotating = false;
			moving = true;

			if(destinationTarget)
			{
				CalculateTargetDestination();
			}
		}
	}

	private void MakeMove()
	{
		player.Cmd_MoveUnit(GetComponent<WorldObject>().netId, transform.position, transform.rotation);

		if(startPos.magnitude != transform.position.magnitude)
		{
			if(agent.velocity.magnitude <= 0.0f)
			{
				moving = false;
				movingIntoPosition = false;
				//Debug.Log("Dojechalem");
			}
		}
		//CalculateBounds();
	}

	private void CalculateTargetDestination() 
	{
		// calculate number of unit vectors from unit centre to unit edge of bounds
		Vector3 originalExtents = selectionBounds.extents;
		Vector3 normalExtents = originalExtents;
		normalExtents.Normalize();
		float numberOfExtents = originalExtents.x / normalExtents.x;
		int unitShift = Mathf.FloorToInt(numberOfExtents);

		// calculate number of unit vectors from target centre to target edge of bounds
		WorldObject worldObject = destinationTarget.GetComponent< WorldObject >();
		if(worldObject) originalExtents = worldObject.GetSelectionBounds().extents;
		else originalExtents = new Vector3(0.0f, 0.0f, 0.0f);
		normalExtents = originalExtents;
		normalExtents.Normalize();
		numberOfExtents = originalExtents.x / normalExtents.x;
		int targetShift = Mathf.FloorToInt(numberOfExtents);

		// calculate number of unit vectors between unit centre and destination centre with bounds just touching
		int shiftAmount = targetShift + unitShift;

		// calculate direction unit needs to travel to reach destination in straight line and normalize to unit vector
		Vector3 origin = transform.position;
		Vector3 direction = new Vector3(destination.x - origin.x, 0.0f, destination.z - origin.z);
		direction.Normalize();

		// destination = center of destination - number of unit vectors calculated above
		// this should give us a destination where the unit will not quite collide with the target
		// giving the illusion of moving to the edge of the target and then stopping
		for(int i = 0; i < shiftAmount; i++)
		{
			destination -= direction;
		}
		destination.y = destinationTarget.transform.position.y;

		destinationTarget = null;
	}






	protected override void FixedUpdate()
	{
		base.FixedUpdate();

	}

	public virtual void StartMove(Vector3 destination)
	{
		if(player.isLocalPlayer)
		{
	        // if nothing to deal with
	        if (this.destination == destination)
	        	return; 


			this.destination = destination;
			destinationTarget = null;
			targetRotation = Quaternion.LookRotation(destination - transform.position);
			startPos = transform.position;

			moving = true;
			//rotating = true;
			agent.ResetPath();
			agent.SetDestination(destination);

			/*if(agent.velocity.sqrMagnitude <= 0)
			{
				rotating = true;
				agent.ResetPath();
			}
			else
			{
				agent.SetDestination(destination);
			}*/
			//agent.velocity = new Vector3(0f,0f,0f);
			//agent.SetDestination(destination);
			//agent.updatePosition = false;
			//agent.updateRotation = false;
		}
	}

	public void StartMove(Vector3 destination, GameObject destinationTarget)
	{
		StartMove(destination);
		this.destinationTarget = destinationTarget;
	}

	public override void MouseClick(GameObject hitObject, Vector3 hitPoint, Player controller)
	{
		base.MouseClick(hitObject, hitPoint, controller);

		//only handle input if owned by a human player and currently selected
		if(player && player.human && currentlySelected) 
		{
			if(hitObject.name == "Ground" && hitPoint != ResourceManager.InvalidPosition) {
				float x = hitPoint.x;
				//makes sure that the unit stays on top of the surface it is on
				float y = hitPoint.y;
				float z = hitPoint.z;
				Vector3 destination = new Vector3(x, y, z);
				StartMove(destination);
			}
		}
	}

	public override void SetHoverState(GameObject hoverObject)
	{
		base.SetHoverState(hoverObject);

		//only handle input if owned by a human player and currently selected
		if(player && player.human && currentlySelected) 
		{
			if(hoverObject.name == "Ground") player.hud.SetCursorState(CursorState.Move);
		}

	}

	public virtual void SetBuilding(Building creator)
	{
		//specific initialization for a unit can be specified here
	}

    public Vector3 GetDestination()
    {
        return destination;
    }

}
