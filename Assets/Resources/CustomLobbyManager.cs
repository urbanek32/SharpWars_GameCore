using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class CustomLobbyManager : NetworkLobbyManager 
{
   
    void Start ()
    {
        var gp = GameObject.FindObjectOfType(typeof (GameParameters)) as GameParameters;
        gp.GetComponent<GameParameters>().enabled = true;
    }

    public override bool OnLobbyServerSceneLoadedForPlayer(GameObject lobbyPlayer, GameObject gamePlayer)
    {
        return base.OnLobbyServerSceneLoadedForPlayer(lobbyPlayer, gamePlayer);
    }

}
