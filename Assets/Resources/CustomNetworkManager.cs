using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class CustomNetworkManager : NetworkManager
{
    public int PlayersToStartGame = 2;

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        base.OnServerAddPlayer(conn, playerControllerId);

        if (NetworkServer.connections.Count >= PlayersToStartGame)
        {
            var gameManager = GameObject.FindObjectOfType(typeof (GameManager)) as GameManager;
            if (!gameManager) return;

            gameManager.Initialise();;
            Debug.LogFormat("Ilosc graczy: {0}",NetworkServer.connections.Count);
        }
    }
}
