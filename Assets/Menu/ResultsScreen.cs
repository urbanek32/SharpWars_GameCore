using UnityEngine;
using System.Collections;

using RTS;

public class ResultsScreen : MonoBehaviour 
{
    public GUISkin skin;
    public AudioClip clickSound;
    public float clickVolume = 1.0f;

    private Player winner;
    private VictoryCondition metVictoryCondition;


	// Use this for initialization
	void Start () 
    {
	  
	}
	
	// Update is called once per frame
	void Update () 
    {
	
	}

    void OnGUI()
    {
        GUI.skin = skin;

        GUI.BeginGroup(new Rect(0, 0, Screen.width, Screen.height));

        // display
        float padding = ResourceManager.Padding;
        float itemHeight = ResourceManager.ButtonHeight;
        float buttonWidth = ResourceManager.ButtonWidth;
        float leftPos = padding;
        float topPos = padding;
        GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "");
        string message = "Game Over";
        if (winner)
        {
            message = "Congratulations " + winner.username + "! You have won by " 
                + metVictoryCondition.GetDescription();
        }
        GUI.Label(new Rect(leftPos, topPos, Screen.width - 2 * padding, itemHeight), message);
        leftPos = Screen.width / 2 - padding / 2 - buttonWidth;
        topPos += itemHeight + padding;
        if (GUI.Button(new Rect(leftPos, topPos, buttonWidth, itemHeight), "New Game"))
        {
            PlayClick();
            //makes sure that the loaded level runs at normal speed
            Time.timeScale = 1.0f;
            ResourceManager.MenuOpen = false;
            //Application.LoadLevel("Map");
            Debug.Log("Load new map");
        }
        leftPos += padding + buttonWidth;
        if (GUI.Button(new Rect(leftPos, topPos, buttonWidth, itemHeight), "Main Menu"))
        {
            ResourceManager.LevelName = "";
            //Application.LoadLevel("MainMenu");
            Debug.Log("Load MainMenu");
            Cursor.visible = true;
        }

        GUI.EndGroup();
    }

    private void PlayClick()
    {

    }

    public void SetMetVictoryCondition(VictoryCondition victoryCondition)
    {
        if (!victoryCondition) return;
        metVictoryCondition = victoryCondition;
        winner = metVictoryCondition.GetWinner();
    }
}
