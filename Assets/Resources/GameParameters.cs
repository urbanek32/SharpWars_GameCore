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

    private string _tempParams = @"sharpwars://master=true&username=test&token=SUPERTOKENBULWO123321!@#%^OLE&server_ip=62.61.60.7&server_port=4321/";
	
	void Start () 
    {
	    //var param = Environment.GetCommandLineArgs();
	    //Debug.Log(param[0]); // exe path
	    //Debug.Log(param[1]); // our params

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

        // TODO Uncomment on deploy
	    /*var nm = GameObject.FindObjectOfType(typeof (CustomNetworkManager)) as CustomNetworkManager;
	    if (nm != null)
	    {
	        nm.networkAddress = Parameters["server_ip"];
	        nm.networkPort = int.Parse(Parameters["server_port"]);
	    }*/
    }
}
