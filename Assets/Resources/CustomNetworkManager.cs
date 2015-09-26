using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class CustomNetworkManager : NetworkManager
{
    public int PlayersToStartGame = 2;

    private GameManager _gameManager;

    void Start()
    {
        Debug.Log("CNM Start");
        _gameManager = FindObjectOfType(typeof(GameManager)) as GameManager;
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        base.OnServerAddPlayer(conn, playerControllerId);

        if (NetworkServer.connections.Count >= PlayersToStartGame)
        {
            _gameManager.LoadDetails();
            Debug.LogFormat("Ilosc graczy: {0}", NetworkServer.connections.Count);
        }
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log("Server started");

        _gameManager.enabled = true;
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        Debug.Log("Server stopped");

        _gameManager.enabled = false;
    }

    public override void OnStartClient(NetworkClient client)
    {
        base.OnStartClient(client);
        Debug.Log("Client started");

        _gameManager.enabled = true;
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        Debug.Log("Client stopped");

        _gameManager.enabled = false;
    }
}
