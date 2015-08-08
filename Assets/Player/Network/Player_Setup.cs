using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Player_Setup : NetworkBehaviour {

	private Player player;

	void Start () 
	{
		if(isLocalPlayer)
		{
			player = GetComponent<Player>();
			player.human = true;
			player.teamColor = new Color(255,0,0);
			player.username = "player"+Random.Range(1,100);
		}
	}
	

}
