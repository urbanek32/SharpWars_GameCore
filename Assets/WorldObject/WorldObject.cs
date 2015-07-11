using UnityEngine;
using System.Collections;

public class WorldObject : MonoBehaviour {

	public string objectName;
	public Texture2D buildImage;
	public int cost, sellValue, hitPoints, maxHitPoints;

	protected Player player;
	protected string[] actions = {};
	protected bool currentlySelected = false;



	protected virtual void Awake()
	{

	}

	// Use this for initialization
	protected virtual void Start () 
	{
	
	}
	
	// Update is called once per frame
	protected virtual void Update () 
	{
	
	}

	protected virtual void OnGUI()
	{

	}



	private void ChangeSelection(WorldObject worldObject, Player controller)
	{
		SetSelection(false);

		if(controller.SelectedObject)
		{
			controller.SelectedObject.SetSelection(false);
		}

		controller.SelectedObject = worldObject;
		worldObject.SetSelection(true);
	}

	public void SetSelection(bool selected)
	{
		currentlySelected = selected;
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
			WorldObject worldObject = hitObject.transform.root.GetComponent< WorldObject >();

			// kliknieto na inna zaznaczalna jednostke
			if(worldObject)
			{
				ChangeSelection(worldObject, controller);
			}
		}
	}










}
