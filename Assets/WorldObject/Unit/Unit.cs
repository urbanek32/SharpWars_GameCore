using UnityEngine;
using System.Collections;

using RTS;
using NLua;

public class Unit : WorldObject {

	protected bool moving, rotating;

	private Vector3 destination;
	private Quaternion targetRotation;

	public float moveSpeed, rotateSpeed;

    private string callingName;
    private string _userControlScript;
    public string userControlScript
    {
        get { return _userControlScript; }
        set
        {
            if (value.Length == 0)
            {
                callingName = "";
                _userControlScript = "";
            }
            else
            {
                callingName = "UnitFunc" + System.DateTime.Now.Ticks.ToString();
                string _userControlScript = value;
                string userFunc = "function " + callingName + "(u)\n unit = u\n" + _userControlScript + "\nend";
                UserInput.env.DoString(userFunc);
            }
        }
    }

	protected override void Awake() 
	{
		base.Awake();
	}
	
	protected override void Start () 
	{
		base.Start();
	}
	
	protected override void Update () 
	{
		base.Update();

        if (callingName != null && callingName.Length > 0)
        {
            object[] arg = {this};
            Call(callingName, arg);
        }

		if(rotating)
		{
			TurnToTarget();
		}
		else if(moving)
		{
			MakeMove();
		}
	}
	
	protected override void OnGUI() 
	{
		base.OnGUI();
	}


	 

	private void TurnToTarget()
	{
		transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotateSpeed);

		//sometimes it gets stuck exactly 180 degrees out in the calculation and does nothing, this check fixes that
		Quaternion inverseTargetRotation = new Quaternion(-targetRotation.x, -targetRotation.y, -targetRotation.z, -targetRotation.w);
		if(transform.rotation == targetRotation || transform.rotation == inverseTargetRotation) 
		{
			rotating = false;
			moving = true;
		}
		CalculateBounds();
	}

	private void MakeMove()
	{
		transform.position = Vector3.MoveTowards(transform.position, destination, Time.deltaTime * moveSpeed);
		if(transform.position == destination)
		{
			moving = false;
		}
		CalculateBounds();
	}


	public void StartMove(Vector3 destination)
	{
        //if nothing to deal with
        if (this.destination == destination)
            return;

		this.destination = destination;
		targetRotation = Quaternion.LookRotation(destination - transform.position);
		rotating = true;
		moving = false;
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

    public System.Object[] Call(string function, params System.Object[] args)
    {
        System.Object[] result = new System.Object[0];
        if (UserInput.env == null) return result;
        LuaFunction lf = UserInput.env.GetFunction(function);
        if (lf == null) return result;
        try
        {
            // Note: calling a function that does not
            // exist does not throw an exception.
            if (args != null)
            {
                result = lf.Call(args);
            }
            else
            {
                result = lf.Call();
            }
        }
        catch (NLua.Exceptions.LuaException e)
        {
            Debug.Log("[LUA-EX]" + e.ToString());
        }
        return result;
    }

}
