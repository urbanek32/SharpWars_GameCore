﻿using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RTS;
using UnityEngine.Networking;

public class GameParameters : MonoBehaviour
{
    public Dictionary<string, string> Parameters = new Dictionary<string, string>();

    private string _tempParams =
        @"sharpwars://master=1&username=test&token=eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJ1c2VybmFtZSI6InRlc3QiLCJpYXQiOjE0NDc5NTI5MjYsImV4cCI6MTQ0Nzk3MDkyNn0.2GzZqS0hLgzdHndRq7EQ1h9TEuYQvWYvyzSxYg5L1o4&server_ip=62.61.60.7&server_port=1234/";



    void Awake()
    {
        
    }

	void Start () 
    {
	    //Debug.Log(param[0]); // exe path
        //Debug.Log(Environment.GetCommandLineArgs()[1]); // our params
	    if (Environment.GetCommandLineArgs().Length >= 2)
	    {
            _tempParams = Environment.GetCommandLineArgs()[1];
	    }

	    _tempParams = _tempParams.Remove(_tempParams.Length - 1); // remove '/' at the end
	    var protocol = _tempParams.Remove(12);
	    if (protocol != "sharpwars://")
	    {
	        Debug.Log("Incorrect protocol");
	        Application.Quit();
	        return;
	    }
	    var parameters = _tempParams.Substring(12);
        var groupedParams = parameters.Split('&');
	    foreach (var words in groupedParams.Select(param => param.Split('=')))
	    {
            Parameters[words[0]] = words[1];
	    }

	    ResourceManager.PlayerName = Parameters["username"];
	    ResourceManager.PlayerToken = Parameters["token"];

        Debug.Log(_tempParams);

	    // TODO Uncomment on deploy
	    /*var customLobby = GameObject.FindObjectOfType(typeof(CustomLobbyManager)) as CustomLobbyManager;
	    if (customLobby != null)
	    {
	        if (Parameters["master"] == "1")
	        {
	            customLobby.StartServer();
	        }
	        else
	        {
                customLobby.StartClient();
            }
	    }*/

	    var nm = GameObject.FindObjectOfType(typeof (CustomLobbyManager)) as CustomLobbyManager;
	    if (nm != null)
	    {
	        nm.networkAddress = Parameters["server_ip"];
	        nm.networkPort = int.Parse(Parameters["server_port"]);
	    }
    }
}
