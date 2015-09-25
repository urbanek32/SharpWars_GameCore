using UnityEngine;
using System.Collections;
using System.Linq;

using RTS;

public class GameManager : MonoBehaviour
{
    private static bool created = false;
    private bool initialised = false;
    private VictoryCondition[] victoryConditions;
    private HUD hud;


	void Awake ()
	{
	    /*Initialise();*/
	}

    public void Initialise()
    {
        Debug.Log("Init game managera");
        if (!created)
        {
            DontDestroyOnLoad(transform.gameObject);
            created = true;
            initialised = true;
        }
        else
        {
            Destroy(this.gameObject);
        }

        if (initialised)
        {
            LoadDetails();
        }
    }

    void OnLevelWasLoaded()
    {
        /*if (initialised)
        {
            LoadDetails();
            Time.timeScale = 1.0f;
            ResourceManager.MenuOpen = false;
        }*/
    }

    private void LoadDetails()
    {
        var players = GameObject.FindObjectsOfType(typeof(Player)) as Player[];
        if (players == null) return;

        foreach (var player in players.Where(player => player.human))
        {
            hud = player.GetComponentInChildren<HUD>();
        }

        victoryConditions = GameObject.FindObjectsOfType(typeof(VictoryCondition)) as VictoryCondition[];
        if (victoryConditions == null) return;

        foreach (var victoryCondition in victoryConditions)
        {
            victoryCondition.SetPlayers(players);
        }
    }
	
	void Update ()
	{
	    if (victoryConditions == null) return;
	    foreach (var victoryCondition in victoryConditions.Where(victoryCondition => victoryCondition.GameFinished()))
	    {
	        ResultsScreen resultsScreen = hud.GetComponent<ResultsScreen>();
	        resultsScreen.SetMetVictoryCondition(victoryCondition);
	        resultsScreen.enabled = true;
	        Time.timeScale = 0.0f;
	        Cursor.visible = true;
	        ResourceManager.MenuOpen = true;
	        hud.enabled = false;
	    }
	}
}
