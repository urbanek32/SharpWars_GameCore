using UnityEngine;
using System.Collections;



public class HUD : MonoBehaviour {

	public GUISkin resourceSkin, ordersSkin;

	private Player player;

	private const int ORDERS_BAR_WIDTH = 150, RESOURCE_BAR_HEIGHT = 40;

	// Use this for initialization
	void Start () 
	{
		player = transform.root.GetComponent< Player >();
	}
	
	// called each frame to handle any drawing our script is responsible for
	void OnGUI () 
	{
		if(player && player.human)
		{
			DrawOrdersBar();
			DrawResourceBar();
		}
	}



	private void DrawOrdersBar()
	{
		GUI.skin = ordersSkin;
		GUI.BeginGroup(new Rect(Screen.width-ORDERS_BAR_WIDTH,RESOURCE_BAR_HEIGHT,ORDERS_BAR_WIDTH,Screen.height-RESOURCE_BAR_HEIGHT));
		GUI.Box(new Rect(0,0,ORDERS_BAR_WIDTH,Screen.height-RESOURCE_BAR_HEIGHT),"");
		GUI.EndGroup();
	}

	private void DrawResourceBar()
	{
		GUI.skin = resourceSkin;
		GUI.BeginGroup(new Rect(0,0,Screen.width,RESOURCE_BAR_HEIGHT));
		GUI.Box(new Rect(0,0,Screen.width,RESOURCE_BAR_HEIGHT),"");
		GUI.EndGroup();
	}
}
