using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

using RTS;
using STL;
using NLua;
using Script;


public class WorldObject : NetworkBehaviour {

	[SyncVar] public NetworkInstanceId ownerId;

	public string objectName;
	public Texture2D buildImage;
	public int cost, sellValue;
    [SyncVar]public int hitPoints, maxHitPoints;
    public string unitScript;
    public int selectedScript; //which shall be performed
	public float weaponRange = 10.0f;
	public float weaponRechargeTime = 1.0f;
	public float weaponAimSpeed = 5.0f;
    public float detectionRange = 20.0f;

	protected Player player;
	protected string[] actions = {};
	protected bool currentlySelected = false;
	protected Bounds selectionBounds;
	protected Rect playingArea = new Rect(0.0f, 0.0f, 0.0f, 0.0f); // ekran bez HUD
	protected GUIStyle healthStyle = new GUIStyle();
	protected float healthPercentage = 1.0f;
	protected WorldObject target = null;
	protected bool attacking = false;
	protected bool movingIntoPosition = false;
	protected bool aiming = false;
    protected List<WorldObject> nearbyObjects;

	private List< Material > oldMaterials = new List< Material >();
	private float currentWeaponChargeTime;
    private float timeSinceLastDecision = 0.0f, timeBetweenDecisions = 0.1f;

    protected float enemyMaxScanDistance = 10.0f;


	//splited script <blocking func part, execution checker that return true or false>
    protected List<ConditionalStatement> scriptExecutionQueue = new List<ConditionalStatement>();

    public void runScript()
    {
        try
        {
            scriptExecutionQueue = ScriptManager.RegisterUserIngameScript(unitScript);
            player.hud.scriptErrorString = string.Format("[{0}] {1}", DateTime.Now.ToString("HH:mm:ss"), "Skompilowane i działa!");
        }
        catch (NLua.Exceptions.LuaException e)
        {
            var lineWithError = ScriptManager.FindErrorInBuffer(e.Message);

            if (string.IsNullOrEmpty(lineWithError))
            {
                player.hud.scriptErrorString = string.Format("Błąd tkwi gdzieś w: \"{0}\"\n{1}", e.Message, player.hud.scriptErrorString);
            }
            else
            {
                var cut = unitScript.Remove(unitScript.IndexOf(lineWithError, StringComparison.Ordinal));
                var lineCount = cut.Split('\n').Length;

                player.hud.scriptErrorString = string.Format("Błąd w lini: {0}\n#> \"{1}\"\n\n{2}", lineCount, lineWithError, player.hud.scriptErrorString);
            }
        }
    }

    public void stopScript()
    {
        scriptExecutionQueue.Clear();
    }

    public bool isExecutingScript()
    {
        return scriptExecutionQueue.Count > 0;
    }

	protected virtual void Awake()
	{
		selectionBounds = ResourceManager.InvalidBounds;
		CalculateBounds();
        unitScript = "";
	}

	// Use this for initialization
	protected virtual void Start () 
	{
		SetPlayer();
		if(player)
		{
			SetTeamColor();
		}
        selectedScript = -1;
	}
	
	// Update is called once per frame
	protected virtual void Update () 
	{
        ProcessScriptQueue();

        if (ShouldMakeDecision())
        {
            DecideWhatToDo();
        }

		currentWeaponChargeTime += Time.deltaTime;
		if(attacking && !movingIntoPosition && !aiming)
		{
			PreformAttack();
		}
	}

	protected virtual void FixedUpdate()
	{

	}

	protected virtual void OnGUI()
	{
		if(currentlySelected && !ResourceManager.MenuOpen)
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

	protected void SetTeamColor()
	{
		TeamColor[] teamColors = GetComponentsInChildren< TeamColor >();
		foreach(TeamColor teamColor in teamColors) 
		{
			Renderer renderer = teamColor.GetComponent<Renderer>();
			if(renderer)
			{
				renderer.material.color = player.teamColor;
			}
			//teamColor.renderer.material.color = player.teamColor; // lel, but werks
		}
	}

	protected virtual void BeginAttack(WorldObject target)
	{
		this.target = target;
		if(TargetInRange())
		{
			attacking = true;
			PreformAttack();
		}
		else
		{
			AdjustPosition();
		}
	}

	protected virtual void AimAtTarget()
	{
		aiming = true;
		// this behaviour needs to be specified by a specific object
	}

	protected virtual void UseWeapon()
	{
		currentWeaponChargeTime = 0.0f;
		// this behaviour needs to be specified by a specific object
	}

    protected virtual bool ShouldMakeDecision()
    {
        if(!attacking && !movingIntoPosition && !aiming)
        {
            // we are not doing anything at the moment
            if(timeSinceLastDecision > timeBetweenDecisions)
            {
                timeSinceLastDecision = 0.0f;
                return true;
            }
            timeSinceLastDecision += Time.deltaTime;
        }
        return false;
    }

    protected virtual void DecideWhatToDo()
    {
        //determine what should be done by the world object at the current point in time
        /*Vector3 currentPosition = transform.position;
        nearbyObjects = WorkManager.FindNearbyObjects(currentPosition, detectionRange);
        if (CanAttack())
        {
            List<WorldObject> enemyObjects = new List<WorldObject>();
            foreach (WorldObject nearbyObject in nearbyObjects)
            {
                Resource resource = nearbyObject.GetComponent<Resource>();
                if (resource) continue;
                if (nearbyObject.GetPlayer() != player) enemyObjects.Add(nearbyObject);
            }
            WorldObject closestObject = WorkManager.FindNearestWorldObjectInListToPosition(enemyObjects, currentPosition);
            if (closestObject) BeginAttack(closestObject);
        }*/
    }

    //czemu object? ano żeby się nie wywaliło jak n00b zapoda inta, stringa, whatever...
    public void ScriptAttackObject(object obj)
    {
        var wo = obj as WorldObject;
        if (CanAttack() && wo != null && wo.player != player)
        {
            BeginAttack(wo);
        }
    }

    public void ScriptHarvestResource(object obj)
    {
        var res = obj as Resource;
        if (isHarvester() && res != null)
        {
            StartHarvest(res);
        }
    }

    public Resource[] ScriptGetArrayOfResources()
    {
        //get all ore depos
        var resource = new List<OreDeposit>();

        if (this is Harvester)
        {
            foreach (var od in FindObjectsOfType<OreDeposit>())
            {
                if (Vector3.Distance(transform.position, od.transform.position) > Harvester.resourceMaxScanDistance)
                    continue;

                resource.Add(od);
            }
        }

        return resource.ToArray();
    }

    public WorldObject[] ScriptGetArrayOfEnemies()
    {
        var enemies = new List<WorldObject>();

        foreach(var p in FindObjectsOfType<Player>())
        {
            if (p == player)
                continue;

            //Add all enemy units
            foreach(var u in p.GetComponentsInChildren<Unit>())
            {
                if (Vector3.Distance(transform.position, u.transform.position) > enemyMaxScanDistance)
                    continue;

                enemies.Add(u);
            }

            //Add all enemy buildings
            foreach(var b in p.GetComponentsInChildren<Building>())
            {
                if (Vector3.Distance(transform.position, b.transform.position) > enemyMaxScanDistance)
                    continue;

                enemies.Add(b);
            }
        }

        return enemies.ToArray();
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

	private bool TargetInRange()
	{
		// http://docs.unity3d.com/Documentation/Manual/DirectionDistanceFromOneObjectToAnother.html
		Vector3 targetLocation = target.transform.position;
		Vector3 direction = targetLocation - transform.position;

		return direction.sqrMagnitude < weaponRange * weaponRange;
	}

	private void AdjustPosition()
	{
		Unit self = this as Unit;
		if(self)
		{
			movingIntoPosition = true;
			Vector3 attackPosition = FindNearestAttackPosition();
			self.StartMove(attackPosition);
			attacking = true;
		}
		else
		{
			attacking = false;
		}
	}

	private Vector3 FindNearestAttackPosition()
	{
		Vector3 targetLocation = target.transform.position;
		Vector3 direction = targetLocation - transform.position;
		float targetDistance = direction.magnitude;
		// podjedź na zasięg broni + 10% żeby nie musieć od razu znowu jechać
		float distanceToTravel = targetDistance - (0.9f * weaponRange);

		return Vector3.Lerp(transform.position, targetLocation, distanceToTravel / targetDistance);
	}

	private void PreformAttack()
	{
		if(!target)
		{
			attacking = false;
			return;
		}
		if(!TargetInRange())
		{
			AdjustPosition();
		}
		else if(!TargetInFrontOfWeapon())
		{
			AimAtTarget();
		}
		else if(ReadyToFire())
		{
			UseWeapon();
		}
	}

	private bool TargetInFrontOfWeapon()
	{
	    // true jezeli obiekt jest w polu widzenia o kacie 5*
	    var angle = Vector3.Angle(transform.forward, target.transform.position - transform.position);
	    return Mathf.Abs(angle) < 5;
	}

    private bool ReadyToFire()
    {
        return currentWeaponChargeTime >= weaponRechargeTime;
    }


    public void SetPlayer()
	{
		//Debug.Log(ownerId);
		player = transform.root.GetComponentInChildren< Player >();
		GameObject[] players = GameObject.FindGameObjectsWithTag("Player");


		foreach(GameObject p in players)
		{
			Player pp = p.GetComponent<Player>();
			//Debug.Log(players.Length +" : "+ pp.netId);
			if(pp.netId.Equals(ownerId))
			{
				player = pp;
				if(this is Unit)
				{
					Units units = pp.GetComponentInChildren<Units>();
					transform.parent = units.transform;
				}
				else if(this is Building)
				{
					Buildings builds = pp.GetComponentInChildren<Buildings>();
					transform.parent = builds.transform;
				}
				break;
			}
		}
		
		
	}

	public Player GetPlayer()
	{
		return player;
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
			if(r.gameObject.name.Equals("Particle System")) continue;
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
				Resource resource = hitObject.transform.parent.GetComponent< Resource >();
				if(resource && resource.isEmpty()) return;
				Player owner = hitObject.transform.root.GetComponent< Player >();
				if(owner)
				{
					// the object is controlled by a player
					if(player && player.human)
					{
						// start attack if object is not owned by the same player and this object can attack, else select
						if(player.username != owner.username && CanAttack())
						{
							BeginAttack(worldObject);
						}
						else
						{
							ChangeSelection(worldObject, controller);
						}
					}
					else
					{
						ChangeSelection(worldObject, controller);
					}
				}
				else
				{
					ChangeSelection(worldObject, controller);
				}
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
		GetComponent<NavMeshObstacle>().enabled = enabled;
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

	public void TakeDamage(int damage)
	{
		hitPoints -= damage;
		if(hitPoints <= 0)
		{ 
			//Destroy(gameObject);
			NetworkServer.Destroy(gameObject);
		}
	}

	public void StopAttacking()
	{
		target = null;
		attacking = false;
	}

	public bool isAttacking()
	{
		return attacking;
	}

    public virtual void StopAllActions()
    {
        //Stop Attacking
        StopAttacking();

        //later add more...
    }



    protected void ProcessScriptQueue()
    {
        if (scriptExecutionQueue.Count > 0)
        {
            ScriptManager.SetGlobal("this", this);
        }

        while (scriptExecutionQueue.Count > 0)
        {
            ConditionalStatement cs = scriptExecutionQueue[0];
            if (cs.Execute())
            {
                scriptExecutionQueue.RemoveAt(0);
            }
            else
            {
                break;
            }
        }
    }

    public bool isUnit()
    {
        return (this is Unit);
    }

    public bool isBuilding()
    {
        return (this is Building);
    }

    public bool isTank()
    {
        return (this is Tank);
    }

    public bool isWorker()
    {
        return (this is Worker);
    }

    public bool isHarvester()
    {
        return (this is Harvester);
    }

    public bool isRafinery()
    {
        return (this is Rafinery);
    }

    public bool isWarFactory()
    {
        return (this is WarFactory);
    }

    public bool isOreDeposit()
    {
        return (this is OreDeposit);
    }

    public bool isResource()
    {
        return (this is Resource);
    }

    public bool isWonder()
    {
        return (this is Wonder);
    }

    protected virtual void StartHarvest(Resource resource)
    {
    }

    public float GetHealth()
    {
        return healthPercentage * 100.0f;
    }

}
