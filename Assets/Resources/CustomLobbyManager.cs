using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class CustomLobbyManager : NetworkLobbyManager
{
    protected AudioSource MainAudioSource;

    public AudioClip MainThemeAudioClip;
    public AudioClip MainAmbientAudioClip;

    void Start ()
    {
        var gp = GameObject.FindObjectOfType(typeof (GameParameters)) as GameParameters;
        gp.GetComponent<GameParameters>().enabled = true;

        MainAudioSource = GetComponent<AudioSource>();
        MainAudioSource.clip = MainThemeAudioClip;
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
        //Debug.Log("uciekł 3");
        base.OnLobbyClientExit();
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        base.OnServerSceneChanged(sceneName);
        //Debug.Log(sceneName);
    }

    public override void OnClientSceneChanged(NetworkConnection conn)
    {
        base.OnClientSceneChanged(conn);
        //Debug.Log("cos");
    }

    void OnLevelWasLoaded(int level)
    {
        if (MainAudioSource == null)
        {
            return;
        }

        if (Application.loadedLevelName == "Mapa_Online")
        {
            MainAudioSource.clip = MainAmbientAudioClip;
            MainAudioSource.Play();
        }
        else if (Application.loadedLevelName == "Mapa_Offline")
        {
            MainAudioSource.clip = MainThemeAudioClip;
            MainAudioSource.Play();
        }
    }
}
