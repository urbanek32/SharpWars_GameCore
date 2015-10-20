using System;
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
        @"sharpwars://master=0&username=test2&token=eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJ1c2VybmFtZSI6InRlc3QyIiwiaWF0IjoxNDQ1MzY1MTk4LCJleHAiOjE0NDUzODMxOTh9.tSujcV3-9isS6ihsls1zSDEfGLkFgJoEZN2_E-4ZCEk&server_ip=62.61.60.7&server_port=1234/";



    void Awake()
    {
        
    }

	void Start () 
    {
	    //var param = Environment.GetCommandLineArgs();
	    //Debug.Log(param[0]); // exe path
        //Debug.Log(Environment.GetCommandLineArgs()[1]); // our params
	    if (Environment.GetCommandLineArgs().Length >= 2)
	    {
            _tempParams = Environment.GetCommandLineArgs()[1];
	    }
	    //_tempParams = Environment.GetCommandLineArgs()[1];

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

	    // TODO Uncomment on deploy
	    /*var customLobby = GameObject.FindObjectOfType(typeof(CustomLobbyManager)) as CustomLobbyManager;
	    if (customLobby != null)
	    {
	        customLobby.StartClient();
	    }*/

	    /*var nm = GameObject.FindObjectOfType(typeof (CustomNetworkManager)) as CustomNetworkManager;
	    if (nm != null)
	    {
	        nm.networkAddress = Parameters["server_ip"];
	        nm.networkPort = int.Parse(Parameters["server_port"]);
	    }*/
    }
}
