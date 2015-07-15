using UnityEngine;
using System.Collections;

using RTS;
using NLua;

public class UserInput : MonoBehaviour {

    string user_input_lua_source_code = @"
import 'System'
import 'UnityEngine'
import 'Assembly-CSharp'

-- Use below function to move your lazy ass!
function PanzerVor(pos)
    unit:StartMove(pos)
end
";

	private Player player;
    static public Lua env = new Lua();    //the environment of lua

	// Use this for initialization
	void Start () 
	{
		player = transform.root.GetComponent<Player>();
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(player && player.human) 
		{
			MoveCamera();
			RotateCamera(); // useless?
			MouseActivity();

            if (Input.GetKeyDown(KeyCode.W))
            {
                if (player.SelectedObject && player.SelectedObject is Unit)
                {
                    Unit u = (Unit)player.SelectedObject;
                    u.userControlScript = @"
vec = Vector3(100, 0, 0)
PanzerVor(vec)
";
                }
            }
		}
	}


    void Awake()
    {
        env = new Lua();
        env.LoadCLRPackage();

        env["this"] = this;
        env["unit"] = null;

        try
        {
            env.DoString(user_input_lua_source_code);
        } catch(NLua.Exceptions.LuaException e)
        {
            Debug.Log("[LUA-EXCEPTION] " + e.ToString());
        }
    }

	private void MouseHover()
	{
		if(player.hud.MouseInBounds())
		{
			GameObject hoverObject = FindHitObject();
			if(hoverObject)
			{
				if(player.SelectedObject)
				{
					player.SelectedObject.SetHoverState(hoverObject);
				}
				else if(hoverObject.name != "Ground")
				{
					Player owner = hoverObject.transform.root.GetComponent< Player >();
					if(owner)
					{
						Unit unit = hoverObject.transform.parent.GetComponent< Unit >();
						Building building = hoverObject.transform.parent.GetComponent< Building >();
						if(owner.username == player.username && (unit || building))
						{
							player.hud.SetCursorState(CursorState.Select);
						}
					}
				}
			}
		}
	}

	private void MouseActivity()
	{
		if(Input.GetMouseButtonDown(0))
		{
			LeftMouseClick();
		}
		else if(Input.GetMouseButtonDown(1))
		{
			RightMouseClick();
		}

		MouseHover();
	}

	private void LeftMouseClick()
	{
		// tylko gdy klikamy w obszarze gry, nie HUD
		if(player.hud.MouseInBounds())
		{
			GameObject hitObj = FindHitObject();
			Vector3 hitPoint = FindHitPoint();

			if(hitObj && hitPoint != ResourceManager.InvalidPosition)
			{
				// mielismy juz zaznaczony obiekt i kliknelismy gdzies na mapie
				// jednostka wykona akcje zwiazana z tym kliknieciem
				if(player.SelectedObject)
				{
					player.SelectedObject.MouseClick(hitObj, hitPoint, player);
				}
				else if(hitObj.name != "Ground") // nie kliknelismy w ziemie
				{
					WorldObject worldObject = hitObj.transform.parent.GetComponent<WorldObject>();

					if(worldObject)
					{
						// wiemy ze gracz nie ma zaznaczonych obiektów
						player.SelectedObject = worldObject;
						worldObject.SetSelection(true, player.hud.GetPlayingArea());
					}
				}
			}
		}
	}

	private void RightMouseClick()
	{
		if(player.hud.MouseInBounds() && !Input.GetKey(KeyCode.LeftAlt) && player.SelectedObject)
		{
			player.SelectedObject.SetSelection(false, player.hud.GetPlayingArea());
			player.SelectedObject = null;
		}
	}

	private Vector3 FindHitPoint()
	{
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;

		if(Physics.Raycast(ray, out hit))
		{
			return hit.point;
		}

		return ResourceManager.InvalidPosition;
	}

	private GameObject FindHitObject()
	{
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;

		if(Physics.Raycast(ray, out hit))
		{
			return hit.collider.gameObject;
		}

		return null;
	}

	private void MoveCamera()
	{
		float xpos = Input.mousePosition.x;
		float ypos = Input.mousePosition.y;
		Vector3 movement = new Vector3(0,0,0);
		bool mouseScroll = false;

		// ruszanie kamery w poziomie
		if(xpos >= 0 && xpos < ResourceManager.ScrollWidth)
		{
			movement.x -= ResourceManager.ScrollSpeed;
			player.hud.SetCursorState(CursorState.PanLeft);
			mouseScroll = true;
		}
		else if(xpos <= Screen.width && xpos > Screen.width - ResourceManager.ScrollWidth)
		{
			movement.x += ResourceManager.ScrollSpeed;
			player.hud.SetCursorState(CursorState.PanRight);
			mouseScroll = true;
		}

		// ruszanie kamery w pionie
		if(ypos >= 0 && ypos < ResourceManager.ScrollWidth)
		{
			movement.y -= ResourceManager.ScrollSpeed;
			player.hud.SetCursorState(CursorState.PanDown);
			mouseScroll = true;
		}
		else if(ypos <= Screen.height && ypos > Screen.height - ResourceManager.ScrollWidth)
		{
			movement.y += ResourceManager.ScrollSpeed;
			player.hud.SetCursorState(CursorState.PanUp);
			mouseScroll = true;
		}

		if(!mouseScroll)
		{
			player.hud.SetCursorState(CursorState.Select);
		}

		//make sure movement is in the direction the camera is pointing
		//but ignore the vertical tilt of the camera to get sensible scrolling
		movement = Camera.main.transform.TransformDirection(movement);
		movement.y = 0;

		// zoom in | zoom out
		movement.y -= ResourceManager.ZoomSpeed * Input.GetAxis("Mouse ScrollWheel");

		// obliczanie pozycji kamery na podstawie inputu
		Vector3 origin = Camera.main.transform.position;
		Vector3 destination = origin;
		destination.x += movement.x;
		destination.y += movement.y;
		destination.z += movement.z;

		// limitowanie zooma
		if(destination.y > ResourceManager.MaxCameraHeight)
		{
			destination.y = ResourceManager.MaxCameraHeight;
		}
		else if(destination.y < ResourceManager.MinCameraHeight)
		{
			destination.y = ResourceManager.MinCameraHeight;
		}

		// przesuń kamere tylko gdy zmienila sie pozycja
		if(destination != origin)
		{
			Camera.main.transform.position = Vector3.MoveTowards(origin, destination, Time.deltaTime * ResourceManager.ScrollSpeed);
		}
	}

	private void RotateCamera()
	{

	}

    public System.Object[] Call(string function, params System.Object[] args)
    {
        System.Object[] result = new System.Object[0];
        if (env == null) return result;
        LuaFunction lf = env.GetFunction(function);
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

    public System.Object[] Call(string function)
    {
        return Call(function, null);
    }

}
