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

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        Debug.Log("uciekł");
        base.OnClientDisconnect(conn);
    }

    public override void OnLobbyClientDisconnect(NetworkConnection conn)
    {
        Debug.Log("uciekł 2");
        base.OnLobbyClientDisconnect(conn);
    }

    public override void OnLobbyClientExit()
    {
        Debug.Log("uciekł 3");
        base.OnLobbyClientExit();
    }
}
