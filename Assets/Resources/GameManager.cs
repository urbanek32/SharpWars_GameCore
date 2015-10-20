using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.Networking;

using RTS;

public class GameManager : MonoBehaviour
{
    private static bool created = false;
    private bool initialised = false;
    private VictoryCondition[] victoryConditions;
    private Player _localPlayer;


	void Awake ()
	{
	    
	}

    void Start()
    {
        Initialise();
    }

    public void Initialise()
    {
        //Debug.Log("Game Manager Init");
        if (!created)
        {
            DontDestroyOnLoad(transform.gameObject);
            created = true;
            initialised = true;
        }
        else
        {
            Debug.Log("Game Manager Destroyed");
            Destroy(this.gameObject);
        }

        if (initialised)
        {
            LoadDetails();
        }
    }

    public void LoadDetails()
    {
        //Debug.Log("Game Manager LoadDetails");

        var players = GameObject.FindObjectsOfType(typeof(Player)) as Player[];
        if (players == null) return;

        Debug.LogFormat("Game Manager Players: {0}", players.Length);
        foreach (var player in players.Where(player => player.isLocalPlayer))
        {
            _localPlayer = player;
            break;
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
        if ((!initialised || !_localPlayer || !_localPlayer.isServer))
	    {
	        return;
	    }

	    if (victoryConditions == null) return;
	    foreach (var victoryCondition in victoryConditions.Where(victoryCondition => victoryCondition.GameFinished()))
	    {
            _localPlayer.Cmd_PlayerWin(victoryCondition.GetWinner().netId, victoryCondition.GetDescription());
	    }
	}
}
