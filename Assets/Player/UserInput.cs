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
					WorldObject worldObject = hitObj.transform.root.GetComponent<WorldObject>();

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

		// ruszanie kamery w poziomie
		if(xpos >= 0 && xpos < ResourceManager.ScrollWidth)
		{
			movement.x -= ResourceManager.ScrollSpeed;
		}
		else
		if(xpos <= Screen.width && xpos > Screen.width - ResourceManager.ScrollWidth)
		{
			movement.x += ResourceManager.ScrollSpeed;
		}

		// ruszanie kamery w pionie
		if(ypos >= 0 && ypos < ResourceManager.ScrollWidth)
		{
			movement.y -= ResourceManager.ScrollSpeed;
		}
		else
		if(ypos <= Screen.height && ypos > Screen.height - ResourceManager.ScrollWidth)
		{
			movement.y += ResourceManager.ScrollSpeed;
		}

		//make sure movement is in the direction the camera is pointing
		//but ignore the vertical tilt of the camera to get sensible scrolling
		movement = Camera.main.transform.TransformDirection(movement);
		movement.z = 0;

		// zoom in | zoom out
		movement.z -= ResourceManager.ZoomSpeed * Input.GetAxis("Mouse ScrollWheel");

		// obliczanie pozycji kamery na podstawie inputu
		Vector3 origin = Camera.main.transform.position;
		Vector3 destination = origin;
		destination.x += movement.x;
		destination.y += movement.y;
		destination.z += movement.z;

		// limitowanie zooma
		if(destination.z > ResourceManager.MaxCameraHeight)
		{
			destination.z = ResourceManager.MaxCameraHeight;
		}
		else
		if(destination.z < ResourceManager.MinCameraHeight)
		{
			destination.z = ResourceManager.MinCameraHeight;
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
