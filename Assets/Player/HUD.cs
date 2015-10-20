using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

using RTS;
using UnityEngine.Networking;


public class HUD : MonoBehaviour {

	public GUISkin resourceSkin, ordersSkin, selectBoxSkin, mouseCursorSkin;
	public Texture2D activeCursor;
	public Texture2D selectCursor, leftCursor, rightCursor, upCursor, downCursor;
	public Texture2D[] moveCursors, attackCursors, harvestCursors;
	public Texture2D[] resources;
	public Texture2D buttonHover, buttonClick;
	public Texture2D buildFrame, buildMask;
	public Texture2D smallButtonHover, smallButtonClick;
	public Texture2D rallyPointCursor;
	public Texture2D healthy, damaged, critical;
	public Texture2D[] resourceHealthBars;
    public string scriptErrorString;

	private Player player;
	private CursorState activeCursorState;
	private int currentFrame = 0;
	private Dictionary< ResourceType, int > resourceValues, resourceLimits;
	private Dictionary< ResourceType, Texture2D > resourceImages;
	private WorldObject lastSelection;
	private float sliderValue;
	private int buildAreaHeight = 0;
	private CursorState previousCursorState;

    private static GUIStyle comboboxStyle = new GUIStyle();
    private ComboBox scriptSelectionBox = new ComboBox(new Rect(5, 20, 150, 20), new GUIContent("Wybierz skrypt"), null, comboboxStyle);
	private bool scriptWindowOpen = false;
    private Rect scriptWindowRect = new Rect(50, 50, 600, 540);
    private bool showCreateNewScriptWindow = false;
    private string newScriptName = "";

    private const int ORDERS_BAR_WIDTH = 150, RESOURCE_BAR_HEIGHT = 40;
	private const int SELECTION_NAME_HEIGHT = 15;
	private const int ICON_WIDTH = 32, ICON_HEIGHT = 32, TEXT_WIDTH = 128, TEXT_HEIGHT = 32;
	private const int BUILD_IMAGE_WIDTH = 64, BUILD_IMAGE_HEIGHT = 64;
	private const int BUTTON_SPACING = 7;
	private const int SCROLL_BAR_WIDTH = 22;
	private const int BUILD_IMAGE_PADDING = 8;

    private const int SCRIPT_DEBUG_MSG_AREA_HEIGHT = 150;

    public const string STR_PROGRAM_OBJECT = "Zaprogramuj\nobiekt";
    public const string STR_STOP_SCRIPT_EXECUTION = "Zatrzymaj\nprogram";

    private const float createScriptWindowHeight = 64f;

    //HOOK
    private int tabInsertPos = -1;

	// Use this for initialization
	void Start () 
	{
		player = transform.root.GetComponent< Player >();

		ResourceManager.StoreSelectBoxItems(selectBoxSkin, healthy, damaged, critical);
		SetCursorState(CursorState.Select);

		resourceValues = new Dictionary< ResourceType, int >();
		resourceLimits = new Dictionary< ResourceType, int >();
		resourceImages = new Dictionary< ResourceType, Texture2D >();

		for(int i = 0; i < resources.Length; i++) 
		{
			switch(resources[i].name) 
			{
			case "Money":
				resourceImages.Add(ResourceType.Money, resources[i]);
				resourceValues.Add(ResourceType.Money, 0);
				resourceLimits.Add(ResourceType.Money, 0);
				break;
			case "Power":
				resourceImages.Add(ResourceType.Power, resources[i]);
				resourceValues.Add(ResourceType.Power, 0);
				resourceLimits.Add(ResourceType.Power, 0);
				break;
			default: break;
			}
		}

		buildAreaHeight = Screen.height - RESOURCE_BAR_HEIGHT - SELECTION_NAME_HEIGHT - 2 * BUTTON_SPACING;
		
		Dictionary< ResourceType, Texture2D > resourceHealthBarTextures = new Dictionary< ResourceType, Texture2D >();
		for(int i = 0; i < resourceHealthBars.Length; i++) 
		{
			switch(resourceHealthBars[i].name) 
			{
			case "ore":
				resourceHealthBarTextures.Add(ResourceType.Ore, resourceHealthBars[i]);
				break;
			default: break;
			}
		}
		ResourceManager.SetResourceHealthBarTextures(resourceHealthBarTextures);

        Texture2D wh = new Texture2D(1, 1);
        wh.SetPixel(0, 0, Color.white);
        wh.Apply();
        Texture2D bl = new Texture2D(1, 1);
        bl.SetPixel(0, 0, Color.black);
        bl.Apply();


        //Set ComboBox style
        comboboxStyle.active.textColor =
        comboboxStyle.onNormal.textColor =
        comboboxStyle.normal.textColor = Color.white;
        comboboxStyle.onActive.background =
        comboboxStyle.onNormal.background =
        comboboxStyle.normal.background = bl;
        comboboxStyle.onHover.background =
        comboboxStyle.hover.background = wh;
        comboboxStyle.onHover.textColor =
        comboboxStyle.hover.textColor = Color.black;
        comboboxStyle.padding.left =
        comboboxStyle.padding.right =
        comboboxStyle.padding.top =
        comboboxStyle.padding.bottom = 4;
	}
	
	// called each frame to handle any drawing our script is responsible for
	void OnGUI () 
	{
		if(player && player.human)
		{
            DrawMouseCursor();

		    if (ResourceManager.MenuOpen)
		    {
		        return;
		    }
		    DrawOrdersBar();
		    DrawResourceBar();
		    DrawScriptWindow();

            if (showCreateNewScriptWindow)
            {
                Rect createWndPos = new Rect(scriptWindowRect.position.x,
                    scriptWindowRect.position.y + (scriptWindowRect.height - createScriptWindowHeight) / 2,
                    scriptWindowRect.width,
                    createScriptWindowHeight);

                GUI.Window(0, createWndPos, DrawCreateNewScriptWindow, "Enter script name:");
            }
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

        //is in script window?
        if (insideHeight && insideWidth && scriptWindowOpen && player.SelectedObject != null)
        {
            insideWidth = mousePos.x >= scriptWindowRect.x && mousePos.x <= scriptWindowRect.xMax;
            insideHeight = mousePos.y <= Screen.height - scriptWindowRect.y && mousePos.y >= Screen.height - scriptWindowRect.yMax;

            return !(insideWidth && insideHeight);
        }

		return insideWidth && insideHeight;
	}

	public void SetCursorState(CursorState newState)
	{
		if(activeCursorState != newState)
		{
			previousCursorState = activeCursorState;
		}

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
		case CursorState.RallyPoint:
			activeCursor = rallyPointCursor;
			break;
		default:
			break;
		}
	}

	public void SetResourceValues(Dictionary< ResourceType, int > resourceValues, Dictionary< ResourceType, int > resourceLimits) 
	{
		this.resourceValues = resourceValues;
		this.resourceLimits = resourceLimits;
	}

	public CursorState GetPreviousCursorState()
	{
		return previousCursorState;
	}

	public CursorState GetCursorState()
	{
		return activeCursorState;
	}

    public void DisplayResultScreen(NetworkInstanceId playerId, string descriptionWon)
    {
            var playerTheWinner = ClientScene.objects[playerId].gameObject.GetComponent<Player>();
            var resultsScreen = GetComponent<ResultsScreen>();
            //resultsScreen.SetMetVictoryCondition(victoryCondition);
            resultsScreen.Player = player;
            resultsScreen.DescriptionWin = descriptionWon;
            resultsScreen.PlayerWinner = playerTheWinner.username;
            resultsScreen.LocalPlayerWin = playerTheWinner.isLocalPlayer;
            resultsScreen.enabled = true;
            Time.timeScale = 0.0f;
            Cursor.visible = true;
            ResourceManager.MenuOpen = true;
            this.enabled = false;      
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
			if(!player.IsFindingBuildingLocation())
			{
				GUI.skin = mouseCursorSkin;
				GUI.BeginGroup(new Rect(0, 0, Screen.width, Screen.height));
				UpdateCursorAnimation();
				Rect cursorPos = GetCursorDrawPosition();
				//GUI.Label(cursorPos, activeCursor);
				GUI.DrawTexture(cursorPos, activeCursor);
				GUI.EndGroup();
			}
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
		else if(activeCursorState == CursorState.RallyPoint)
		{
			topPos -= activeCursor.height;
		}

		return new Rect(leftPos, topPos, activeCursor.width + cursorSizer, activeCursor.height + cursorSizer);
	}

	private void DrawOrdersBar()
	{
		GUI.skin = ordersSkin;
		GUI.BeginGroup(new Rect(Screen.width - ORDERS_BAR_WIDTH - BUILD_IMAGE_WIDTH, RESOURCE_BAR_HEIGHT, ORDERS_BAR_WIDTH + BUILD_IMAGE_WIDTH, Screen.height - RESOURCE_BAR_HEIGHT));
		GUI.Box(new Rect(BUILD_IMAGE_WIDTH + SCROLL_BAR_WIDTH, 0, ORDERS_BAR_WIDTH, Screen.height - RESOURCE_BAR_HEIGHT),"");
		string selectionName = "";
		if(player.SelectedObject)
		{
			selectionName = player.SelectedObject.objectName;
			if(player.SelectedObject.IsOwnedBy(player))
			{
				// reset slider value if selected object has changed
				if(lastSelection && lastSelection != player.SelectedObject)
				{
					sliderValue = 0.0f;
				}
				DrawScriptButton();
                DrawRunScriptButton();
				DrawActions(player.SelectedObject.GetActions());
				// store the current selection
				lastSelection = player.SelectedObject;

				Building selectedBuilding = lastSelection.GetComponent< Building >();
				if(selectedBuilding)
				{
					DrawBuildQueue(selectedBuilding.getBuildQueueValues(), selectedBuilding.getBuildPercentage());
					DrawStandardBuildingOptions(selectedBuilding);
				}
			}
		}

		if(!selectionName.Equals(""))
		{
			int topPos = buildAreaHeight + BUTTON_SPACING;
			int leftPos = BUILD_IMAGE_WIDTH + SCROLL_BAR_WIDTH / 2;
			GUI.Label(new Rect(leftPos, topPos, ORDERS_BAR_WIDTH, SELECTION_NAME_HEIGHT), selectionName);
		}

		GUI.EndGroup();
	}

	private void DrawResourceBar()
	{
		GUI.skin = resourceSkin;
		GUI.BeginGroup(new Rect(0,0,Screen.width,RESOURCE_BAR_HEIGHT));
		GUI.Box(new Rect(0,0,Screen.width,RESOURCE_BAR_HEIGHT),"");

		int topPos = 4, iconLeft = 4, textLeft = 20;
		DrawResourceIcon(ResourceType.Money, iconLeft, textLeft, topPos);
		iconLeft += TEXT_WIDTH;
		textLeft += TEXT_WIDTH;
		DrawResourceIcon(ResourceType.Power, iconLeft, textLeft, topPos);

		GUI.EndGroup();
	}

	private void DrawResourceIcon(ResourceType type, int iconLeft, int textLeft, int topPos)
	{
		Texture2D icon = resourceImages[type];
		string text = resourceValues[type].ToString() + "/" + resourceLimits[type].ToString();
		GUI.DrawTexture(new Rect(iconLeft, topPos, ICON_WIDTH, ICON_HEIGHT), icon);
		GUI.Label (new Rect(textLeft, topPos, TEXT_WIDTH, TEXT_HEIGHT), text);
	}

	private void DrawActions(string[] actions)
	{
		GUIStyle buttons = new GUIStyle();
		buttons.hover.background = buttonHover;
		buttons.active.background = buttonClick;
		GUI.skin.button = buttons;
		int numActions = actions.Length;
		//define the area to draw the actions inside
		GUI.BeginGroup(new Rect(BUILD_IMAGE_WIDTH, 128, ORDERS_BAR_WIDTH, buildAreaHeight)); // 0->64 bo scriptWindow
		// draw scroll bar for the list of actions if need be
		if(numActions >= MaxNumRows(buildAreaHeight)) 
		{
			DrawSlider(buildAreaHeight, numActions / 2.0f);
		}
		// display possible actions as buttons and handle the button click for each
		for(int i = 0; i < numActions; i++)
		{
			int column = i % 2;
			int row = i / 2;
			Rect pos = GetButtonPos(row, column);
			Texture2D action = ResourceManager.GetBuildImage(actions[i]);
			if(action)
			{
				// create the button and handle the click of that button
				if(GUI.Button(pos, action))
				{
					if(player.SelectedObject)
					{
						player.SelectedObject.PreformAction(actions[i]);
					}
				}
			}
		}
		GUI.EndGroup();
	}

	private void DrawScriptButton()
	{
		GUIStyle buttons = new GUIStyle();
		buttons.hover.background = buttonHover;
		buttons.active.background = buttonClick;
		buttons.alignment = TextAnchor.MiddleCenter;
		GUI.skin.button = buttons;
		GUI.BeginGroup(new Rect(BUILD_IMAGE_WIDTH, 0, ORDERS_BAR_WIDTH, 64)); // 0->64 bo scriptWindow
		if(GUI.Button(new Rect(22, 10, 128, 60), "Skryptowe okno"))
		{
            player.Cmd_AddResource(player.netId, ResourceType.Money, 5);
			scriptWindowOpen = !scriptWindowOpen;
		}
		GUI.EndGroup();
	}

	private void DrawScriptWindow()
	{
		if(scriptWindowOpen && player.SelectedObject)
		{
            if (tabInsertPos > -1)
            {
                player.SelectedObject.unitScript = player.SelectedObject.unitScript.Insert(tabInsertPos, "\t");
                tabInsertPos = -1;
            }

            //Hook for GUI.TextArea to force adding horizontal tab
            if (Event.current.type == EventType.keyDown && Event.current.keyCode == KeyCode.Tab)
            {
                TextEditor editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
                tabInsertPos = editor.pos;
                editor.pos++;
                editor.selectPos++;
            }

            scriptWindowRect = GUI.Window(0, scriptWindowRect, DrawDraggableScriptWindow, "Script window");
		}
	}

    private void DrawDraggableScriptWindow(int wid)
    {
        if (!scriptSelectionBox.IsClicked())
        {
            GUI.SetNextControlName("ScriptTextArea");
            player.SelectedObject.unitScript = GUI.TextArea(new Rect(5, 40, scriptWindowRect.width - 10, scriptWindowRect.height - SCRIPT_DEBUG_MSG_AREA_HEIGHT - 50), player.SelectedObject.unitScript);
            GUI.FocusControl("ScriptTextArea");

            GUI.TextArea(new Rect(5, scriptWindowRect.height - SCRIPT_DEBUG_MSG_AREA_HEIGHT - 5, scriptWindowRect.width - 10, SCRIPT_DEBUG_MSG_AREA_HEIGHT), scriptErrorString);

            //create new script
            if (GUI.Button(new Rect(scriptWindowRect.width - 230, 20, 75, 20), "Nowy"))
            {
                showCreateNewScriptWindow = true;
            }

            //save script to current selection
            if (GUI.Button(new Rect(scriptWindowRect.width - 155, 20, 75, 20), "Zapisz"))
            {
                if (scriptSelectionBox.SelectedItemIndex >= 0 && player.scriptList.Count > 0)
                {
                    player.scriptList[scriptSelectionBox.SelectedItemIndex].Second = player.SelectedObject.unitScript;

                    var wc = GetComponentInParent<WebsiteCommunication>();

                    wc.EditScriptInCloud(ResourceManager.PlayerName,
                        ResourceManager.PlayerToken,
                        player.scriptList[scriptSelectionBox.SelectedItemIndex].First,
                        "Default",
                        player.scriptList[scriptSelectionBox.SelectedItemIndex].Second,
                        null, null, null);
                }
            }

            //delete selected script
            if (GUI.Button(new Rect(scriptWindowRect.width - 80, 20, 75, 20), "Usuń"))
            {
                if (scriptSelectionBox.SelectedItemIndex >= 0 && player.scriptList.Count > 0)
                {
                    player.scriptList.RemoveAt(scriptSelectionBox.SelectedItemIndex);
                    player.SelectedObject.unitScript = "";
                    scriptSelectionBox.Deselect(player);
                }
            }
        }

        if (player.SelectedObject != null && player.SelectedObject.GetPlayer() == player)
            scriptSelectionBox.Show(player.scriptList.ToArray(), player);

        GUI.DragWindow();
    }

	private int MaxNumRows(int areaHeight)
	{
		return areaHeight / BUILD_IMAGE_HEIGHT;
	}

	private Rect GetButtonPos(int row, int column)
	{
		int left = SCROLL_BAR_WIDTH + column * BUILD_IMAGE_WIDTH;
		float top = row * BUILD_IMAGE_HEIGHT - sliderValue * BUILD_IMAGE_HEIGHT;
		return new Rect(left, top, BUILD_IMAGE_WIDTH, BUILD_IMAGE_HEIGHT);
	}

	private void DrawSlider(int groupHeight, float numRows)
	{
		//slider goes from 0 to the number of rows that do not fit on screen
		sliderValue = GUI.VerticalSlider(GetScrollPos(groupHeight), sliderValue, 0.0f, numRows - MaxNumRows(groupHeight));
	}

	private Rect GetScrollPos(int groupHeight)
	{
		return new Rect(BUTTON_SPACING, BUTTON_SPACING, SCROLL_BAR_WIDTH, groupHeight - 2 * BUTTON_SPACING);
	}

	private void DrawBuildQueue(string[] buildQueue, float buildPercentage)
	{
		for(int i = 0; i < buildQueue.Length; i++)
		{
			float topPos = i * BUILD_IMAGE_HEIGHT - (i+1) * BUILD_IMAGE_PADDING;
			Rect buildPos = new Rect(BUILD_IMAGE_PADDING, topPos + 10, BUILD_IMAGE_WIDTH, BUILD_IMAGE_HEIGHT);
			GUI.DrawTexture(buildPos, ResourceManager.GetBuildImage(buildQueue[i]));
			GUI.DrawTexture(buildPos, buildFrame);
			topPos += BUILD_IMAGE_PADDING;
			float width = BUILD_IMAGE_WIDTH - 2 * BUILD_IMAGE_PADDING;
			float height = BUILD_IMAGE_HEIGHT - 2 * BUILD_IMAGE_PADDING;
			if(i == 0)
			{
				//shrink the build mask on the item currently being built to give an idea of progress
				topPos += height * buildPercentage;
				height *= (1 - buildPercentage);
			}
			GUI.DrawTexture(new Rect(2 * BUILD_IMAGE_PADDING, topPos + 10, width, height), buildMask);
		}
	}

	private void DrawStandardBuildingOptions(Building building)
	{
		GUIStyle buttons = new GUIStyle();
		buttons.hover.background = smallButtonHover;
		buttons.active.background = smallButtonClick;
		GUI.skin.button = buttons;

		int leftPos = BUILD_IMAGE_WIDTH + SCROLL_BAR_WIDTH + BUTTON_SPACING;
		int topPos = buildAreaHeight - BUILD_IMAGE_HEIGHT / 2;
		int width = BUILD_IMAGE_WIDTH / 2;
		int height = BUILD_IMAGE_HEIGHT / 2;

		if(GUI.Button(new Rect(leftPos, topPos, width, height), building.sellImage))
		{
			building.Sell();
		}

		if(building.hasSpawnPoint())
		{
			leftPos += width + BUTTON_SPACING;
			if(GUI.Button(new Rect(leftPos, topPos, width, height), building.rallyPointImage))
			{
				if(activeCursorState != CursorState.RallyPoint && previousCursorState != CursorState.RallyPoint)
				{
					SetCursorState(CursorState.RallyPoint);
				}
				else
				{
					//dirty hack to ensure toggle between RallyPoint and not works ...
					SetCursorState(CursorState.PanRight);
					SetCursorState(CursorState.Select);
				}
			}
		}

	}

    private void DrawRunScriptButton()
    {
        GUIStyle buttons = new GUIStyle();
        buttons.hover.background = buttonHover;
        buttons.active.background = buttonClick;
        buttons.alignment = TextAnchor.MiddleCenter;
        GUI.skin.button = buttons;
        GUI.BeginGroup(new Rect(BUILD_IMAGE_WIDTH, 64, ORDERS_BAR_WIDTH, 64));
        if(GUI.Button(new Rect(22, 10, 128, 60), player.SelectedObject.isExecutingScript() ? STR_STOP_SCRIPT_EXECUTION : STR_PROGRAM_OBJECT))
        {
            if (player.SelectedObject.isExecutingScript())
            {
                player.SelectedObject.stopScript();
            }
            else
            {
                player.SelectedObject.StopAllActions();
                player.SelectedObject.runScript();
            }
        }
        GUI.EndGroup();
    }

    private void DrawCreateNewScriptWindow(int wid)
    {
        GUI.SetNextControlName("ScriptNameArea");
        newScriptName = GUI.TextArea(new Rect(0, 20, scriptWindowRect.width, 20), newScriptName);
        GUI.FocusControl("ScriptNameArea");

        if (GUI.Button(new Rect(10, 40, (scriptWindowRect.width - 20) / 2, 20), "Utwórz"))
        {
            if (string.IsNullOrEmpty(newScriptName) || newScriptName.Trim().Length == 0) return;

            player.scriptList.Add(new STL.Pair<string, string>(newScriptName, ""));
            showCreateNewScriptWindow = false;

            var wc = GetComponentInParent<WebsiteCommunication>();

            wc.AddScriptToCloud(ResourceManager.PlayerName, ResourceManager.PlayerToken, newScriptName, "Default", " ", null, null, this);

            newScriptName = "";
        }
        if (GUI.Button(new Rect(scriptWindowRect.width/2, 40, (scriptWindowRect.width - 20) / 2, 20), "Anuluj"))
        {
            showCreateNewScriptWindow = false;
            newScriptName = "";
        }
    }

}
