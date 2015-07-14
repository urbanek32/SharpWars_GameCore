using UnityEngine;
using System.Collections;

using RTS;

public class HUD : MonoBehaviour {

	public GUISkin resourceSkin, ordersSkin, selectBoxSkin, mouseCursorSkin;
	public Texture2D activeCursor;
	public Texture2D selectCursor, leftCursor, rightCursor, upCursor, downCursor;
	public Texture2D[] moveCursors, attackCursors, harvestCursors;

	private Player player;
	private CursorState activeCursorState;
	private int currentFrame = 0;

	private const int ORDERS_BAR_WIDTH = 150, RESOURCE_BAR_HEIGHT = 40;
	private const int SELECTION_NAME_HEIGHT = 15;


	// Use this for initialization
	void Start () 
	{
		player = transform.root.GetComponent< Player >();

		ResourceManager.StoreSelectBoxItems(selectBoxSkin);
		SetCursorState(CursorState.Select);
	}
	
	// called each frame to handle any drawing our script is responsible for
	void OnGUI () 
	{
		if(player && player.human)
		{
			DrawOrdersBar();
			DrawResourceBar();
			DrawMouseCursor();
		}
	}


	public Rect GetPlayingArea()
	{
		return new Rect(0, RESOURCE_BAR_HEIGHT, Screen.width - ORDERS_BAR_WIDTH, Screen.height - RESOURCE_BAR_HEIGHT);
	}

	public bool MouseInBounds()
	{
		//Screen coordinates start in the lower-left corner of the screen
		//not the top-left of the screen like the drawing coordinates do
		Vector3 mousePos = Input.mousePosition;
		bool insideWidth = mousePos.x >= 0 && mousePos.x <= Screen.width - ORDERS_BAR_WIDTH;
		bool insideHeight = mousePos.y >= 0 && mousePos.y <= Screen.height - RESOURCE_BAR_HEIGHT;
		return insideWidth && insideHeight;
	}

	public void SetCursorState(CursorState newState)
	{
		activeCursorState = newState;

		switch(newState)
		{
		case CursorState.Select:
			activeCursor = selectCursor;
			break;
		case CursorState.Attack:
			currentFrame = (int)Time.time % attackCursors.Length;
			activeCursor = attackCursors[currentFrame];
			break;
		case CursorState.Harvest:
			currentFrame = (int)Time.time % harvestCursors.Length;
			activeCursor = harvestCursors[currentFrame];
			break;
		case CursorState.Move:
			currentFrame = (int)Time.time % moveCursors.Length;
			activeCursor = moveCursors[currentFrame];
			break;
		case CursorState.PanLeft:
			activeCursor = leftCursor;
			break;
		case CursorState.PanRight:
			activeCursor = rightCursor;
			break;
		case CursorState.PanUp:
			activeCursor = upCursor;
			break;
		case CursorState.PanDown:
			activeCursor = downCursor;
			break;
		default:
			break;
		}
	}






	private void DrawMouseCursor()
	{
		bool mouseOverHud = !MouseInBounds() && activeCursorState != CursorState.PanRight && activeCursorState != CursorState.PanUp;

		if(mouseOverHud)
		{
			Cursor.visible = true;
		}
		else
		{
			Cursor.visible = false;
			GUI.skin = mouseCursorSkin;
			GUI.BeginGroup(new Rect(0, 0, Screen.width, Screen.height));
			UpdateCursorAnimation();
			Rect cursorPos = GetCursorDrawPosition();
			//GUI.Label(cursorPos, activeCursor);
			GUI.DrawTexture(cursorPos, activeCursor);
			GUI.EndGroup();
		}
	}

	private void UpdateCursorAnimation()
	{
		//sequence animation for cursor (based on more than one image for the cursor)
		//change once per second, loops through array of images

		if(activeCursorState == CursorState.Move)
		{
			currentFrame = (int)Time.time % moveCursors.Length;
			activeCursor = moveCursors[currentFrame];
		}
		else if(activeCursorState == CursorState.Attack)
		{
			currentFrame = (int)Time.time % attackCursors.Length;
			activeCursor = attackCursors[currentFrame];
		}
		else if(activeCursorState == CursorState.Harvest)
		{
			currentFrame = (int)Time.time % harvestCursors.Length;
			activeCursor = harvestCursors[currentFrame];
		}
	}

	private Rect GetCursorDrawPosition()
	{
		// bo zwykly kursor jest zbyt maly
		float cursorSizer = 10f;
		// podstawowa pozycja dla naszego kursora
		float leftPos = Input.mousePosition.x;
		float topPos = Screen.height - Input.mousePosition.y;
		// dopasuj poazycję na podstawie typu kursora
		if(activeCursorState == CursorState.PanRight)
		{
			leftPos = Screen.width - activeCursor.width - cursorSizer;
		}
		else if(activeCursorState == CursorState.PanDown)
		{
			topPos = Screen.height - activeCursor.height - cursorSizer;
		}
		else if(activeCursorState == CursorState.Move || activeCursorState == CursorState.Harvest || activeCursorState == CursorState.Select)
		{
			topPos -= activeCursor.height / 2;
			leftPos -= activeCursor.width / 2;
		}

		return new Rect(leftPos, topPos, activeCursor.width + cursorSizer, activeCursor.height + cursorSizer);
	}

	private void DrawOrdersBar()
	{
		GUI.skin = ordersSkin;
		GUI.BeginGroup(new Rect(Screen.width-ORDERS_BAR_WIDTH,RESOURCE_BAR_HEIGHT,ORDERS_BAR_WIDTH,Screen.height-RESOURCE_BAR_HEIGHT));

		GUI.Box(new Rect(0,0,ORDERS_BAR_WIDTH,Screen.height-RESOURCE_BAR_HEIGHT),"");
		string selectionName = "";
		if(player.SelectedObject)
		{
			selectionName = player.SelectedObject.objectName;
		}

		if(!selectionName.Equals(""))
		{
			GUI.Label(new Rect(0, 10, ORDERS_BAR_WIDTH, SELECTION_NAME_HEIGHT), selectionName);
		}

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
