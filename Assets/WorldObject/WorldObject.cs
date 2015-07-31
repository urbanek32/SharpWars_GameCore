using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using RTS;
using STL;
using NLua;

public class WorldObject : MonoBehaviour {

	public string objectName;
	public Texture2D buildImage;
	public int cost, sellValue, hitPoints, maxHitPoints;
	public string unitScript;

	protected Player player;
	protected string[] actions = {};
	protected bool currentlySelected = false;
	protected Bounds selectionBounds;
	protected Rect playingArea = new Rect(0.0f, 0.0f, 0.0f, 0.0f); // ekran bez HUD
	protected GUIStyle healthStyle = new GUIStyle();
	protected float healthPercentage = 1.0f;

	private List< Material > oldMaterials = new List< Material >();
	//splited script <blocking func part, execution checker that return true or false>
	protected List<Pair<LuaFunction, LuaFunction>> scriptExecutionQueue = new List<Pair<LuaFunction, LuaFunction>>();
	protected string _userControlScript;


    public string userControlScript
    {
        get { return _userControlScript; }
        set
        {
            if (value.Length == 0)
            {
                scriptExecutionQueue.Clear();
                _userControlScript = "";
            }
            else
            {
                string _userControlScript = value;
                try
                {
                    scriptExecutionQueue = ScriptManager.RegisterUserIngameScript(_userControlScript);
                }
                catch (NLua.Exceptions.LuaException e)
                {
                    Debug.LogError("Custom script error: " + e.ToString());
                }
            }
        }
    }


	protected virtual void Awake()
	{
		selectionBounds = ResourceManager.InvalidBounds;
		CalculateBounds();
	}

	// Use this for initialization
	protected virtual void Start () 
	{
		SetPlayer();
	}
	
	// Update is called once per frame
	protected virtual void Update () 
	{
        ProcecssScriptQueue();
	}

	protected virtual void OnGUI()
	{
		if(currentlySelected)
		{
			DrawSelection();
		}
	}

	protected virtual void DrawSelectionBox(Rect selectBox)
	{
		GUI.Box(selectBox, "");
		CalculateCurrentHealth(0.35f, 0.65f);
		//GUI.Label(new Rect(selectBox.x, selectBox.y - 7, selectBox.width * healthPercentage, 5), "", healthStyle);
		DrawHealthBar(selectBox, "");
	}

	protected virtual void CalculateCurrentHealth(float lowSplit, float highSplit)
	{
		healthPercentage = (float)hitPoints / (float)maxHitPoints;
		if(healthPercentage > highSplit) 
		{
			healthStyle.normal.background = ResourceManager.HealthyTexture;
		}
		else if(healthPercentage > lowSplit) 
		{
			healthStyle.normal.background = ResourceManager.DamagedTexture;
		}
		else
		{
			healthStyle.normal.background = ResourceManager.CriticalTexture;
		}
	}

	protected void DrawHealthBar(Rect selectBox, string label)
	{
		healthStyle.padding.top = -20;
		healthStyle.fontStyle = FontStyle.Bold;
		GUI.Label(new Rect(selectBox.x, selectBox.y - 7, selectBox.width * healthPercentage, 5), label, healthStyle);
	}





	private void ChangeSelection(WorldObject worldObject, Player controller)
	{
		//this should be called by the following line, but there is an outside chance it will not
		SetSelection(false, playingArea);

		if(controller.SelectedObject)
		{
			controller.SelectedObject.SetSelection(false, playingArea);
		}

		controller.SelectedObject = worldObject;
		worldObject.SetSelection(true, controller.hud.GetPlayingArea());
	}

	private void DrawSelection()
	{
		GUI.skin = ResourceManager.SelectBoxSkin;
		Rect selectBox = WorkManager.CalculateSelectionBox(selectionBounds, playingArea);
		//Draw the selection box around the currently selected object, within the bounds of the playing area
		GUI.BeginGroup(playingArea);
		DrawSelectionBox(selectBox);
		GUI.EndGroup();
	}







	public void SetPlayer()
	{
		player = transform.root.GetComponentInChildren< Player >();
	}

	public virtual void SetHoverState(GameObject hoverObject)
	{
		// only handle input if owned by a human player and currently selected
		if(player && player.human && currentlySelected)
		{
			// something other than the ground is being hovered over
			if(hoverObject.name != "Ground")
			{
				Player owner = hoverObject.transform.root.GetComponent< Player >();
				Unit unit = hoverObject.transform.parent.GetComponent< Unit >();
				Building building = hoverObject.transform.parent.GetComponent< Building >();
				if(owner)
				{
					// the object is owned by a player
					if(owner.username == player.username)
					{
						player.hud.SetCursorState(CursorState.Select);
					}
					else if(CanAttack())
					{
						player.hud.SetCursorState(CursorState.Attack);
					}
					else
					{
						player.hud.SetCursorState(CursorState.Select);
					}
				}
				else if(unit || building && CanAttack())
				{
					player.hud.SetCursorState(CursorState.Attack);
				}
				else
				{
					player.hud.SetCursorState(CursorState.Select);
				}
			}
		}
	}

	public void CalculateBounds()
	{
		selectionBounds = new Bounds(transform.position, Vector3.zero);
		foreach(Renderer r in GetComponentsInChildren< Renderer >())
		{
			selectionBounds.Encapsulate(r.bounds);
		}
	}

	public virtual void SetSelection(bool selected, Rect playingArea)
	{
		currentlySelected = selected;
		if(selected)
		{
			this.playingArea = playingArea;
		}
	}

	public string[] GetActions()
	{
		return actions;
	}

	public virtual void PreformAction(string actionToPreform)
	{
		//it is up to children with specific actions to determine what to do with each of those actions
	}

	public virtual void MouseClick(GameObject hitObject, Vector3 hitPoint, Player controller)
	{
		// tylko gdy aktualnie zaznaczony
		if(currentlySelected && hitObject && hitObject.name != "Ground")
		{
			WorldObject worldObject = hitObject.transform.parent.GetComponent< WorldObject >();

			// kliknieto na inna zaznaczalna jednostke
			if(worldObject)
			{
				ChangeSelection(worldObject, controller);
			}
		}
	}

	public bool IsOwnedBy(Player owner)
	{
		if(player && player.Equals(owner)){
			return true;
		} else {
			return false;
		}
	}

	public Bounds GetSelectionBounds()
	{
		return selectionBounds;
	}

	public void SetColliders(bool enabled)
	{
		Collider[] colliders = GetComponentsInChildren< Collider >();
		foreach(Collider collider in colliders) 
		{
			collider.enabled = enabled;
		}
	}

	public void SetTransparentMaterial(Material material, bool storeExistingMaterial)
	{
		if(storeExistingMaterial)
		{
			oldMaterials.Clear();
		}
		Renderer[] renderers = GetComponentsInChildren< Renderer >();
		foreach(Renderer renderer in renderers) 
		{
			if(storeExistingMaterial)
			{
				oldMaterials.Add(renderer.material);
			}
			renderer.material = material;
		}
	}
	
	public void RestoreMaterials() 
	{
		Renderer[] renderers = GetComponentsInChildren< Renderer >();
		if(oldMaterials.Count == renderers.Length) 
		{
			for(int i = 0; i < renderers.Length; i++) 
			{
				renderers[i].material = oldMaterials[i];
			}
		}
	}
	
	public void SetPlayingArea(Rect playingArea) 
	{
		this.playingArea = playingArea;
	}

	public virtual bool CanAttack()
	{
		//default behaviour needs to be overidden by children
		return false;
	}





    protected void ProcecssScriptQueue()
    {
        bool stop_executing = false;
        if (scriptExecutionQueue.Count > 0)
        {
            ScriptManager.SetGlobal("this", this);
        }

        while (scriptExecutionQueue.Count > 0 && !stop_executing)
        {
            Pair<LuaFunction, LuaFunction> queue_item = scriptExecutionQueue[0];
            queue_item.First.Call();

            //if piece of code is non-blocking
            if (queue_item.Second == null)
            {
                scriptExecutionQueue.RemoveAt(0);
            }
            else
            {
                System.Object[] result = queue_item.Second.Call();
                bool b_result = (bool)result[0];
                if (b_result)
                {
                    scriptExecutionQueue.RemoveAt(0);
                }
                else
                {
                    stop_executing = true;
                }
            }
        }
    }








}
