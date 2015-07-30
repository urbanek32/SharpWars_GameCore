using UnityEngine;
using System.Collections;

using RTS;

public class UserInput : MonoBehaviour {
	
    private Player player;

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
vec = Vector3(25, 0, 0)
PanzerVor(vec)
vec.z = 25
PanzerVor(vec)
vec.x = 0
PanzerVor(vec)
vec.z = 0
PanzerVor(vec)
";
                }
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                Unit u = (Unit)player.SelectedObject;
                u.userControlScript = "";
            }
		}
	}


    void Awake()
    {
    }

	private void MouseHover()
	{
		if(player.hud.MouseInBounds())
		{
			if(player.IsFindingBuildingLocation())
			{
				player.FindBuildingLocation();
			}
			else
			{
				GameObject hoverObject = WorkManager.FindHitObject(Input.mousePosition);
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
			if(player.IsFindingBuildingLocation())
			{
				if(player.CanPlaceBuilding())
				{
					player.StartConstruction();
				}
			}
			else
			{
				GameObject hitObj = WorkManager.FindHitObject(Input.mousePosition);
				Vector3 hitPoint = WorkManager.FindHitPoint(Input.mousePosition);

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
	}

	private void RightMouseClick()
	{
		if(player.hud.MouseInBounds() && !Input.GetKey(KeyCode.LeftAlt) && player.SelectedObject)
		{
			if(player.IsFindingBuildingLocation())
			{
				player.CancelBuildingPlacement();
			}
			else
			{
				player.SelectedObject.SetSelection(false, player.hud.GetPlayingArea());
				player.SelectedObject = null;
			}
		}
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

        //Poruszanie kamery za pomocą strzałek
        if(Input.GetKey(KeyCode.UpArrow))
        {
            movement.y += ResourceManager.ScrollSpeed;
        }
        if(Input.GetKey(KeyCode.DownArrow))
        {
            movement.y -= ResourceManager.ScrollSpeed;
        }
        if(Input.GetKey(KeyCode.LeftArrow))
        {
            movement.x -= ResourceManager.ScrollSpeed;
        }
        if(Input.GetKey(KeyCode.RightArrow))
        {
            movement.x += ResourceManager.ScrollSpeed;
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

}
