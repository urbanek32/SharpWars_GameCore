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
			RotateCamera();
		}
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
