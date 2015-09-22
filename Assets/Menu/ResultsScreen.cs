using UnityEngine;
using System.Collections;

using RTS;

public class ResultsScreen : MonoBehaviour
{
    public GUISkin Skin;

    private Player winner;
    private VictoryCondition metVictoryCondition;

	void Start () 
    {
	
	}

    void OnGUI()
    {
        GUI.skin = Skin;
        GUI.BeginGroup(new Rect(0, 0, Screen.width, Screen.height));

        //display 
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
        leftPos = Screen.width / 2f - padding / 2f - buttonWidth;
        topPos += itemHeight + padding;
        if (GUI.Button(new Rect(leftPos, topPos, buttonWidth, itemHeight), "Send score"))
        {
            //makes sure that the loaded level runs at normal speed
            Time.timeScale = 1.0f;
            ResourceManager.MenuOpen = false;
            //Application.LoadLevel("Map");
            Debug.Log("Do smtg usefull");
        }
        leftPos += padding + buttonWidth;
        if (GUI.Button(new Rect(leftPos, topPos, buttonWidth, itemHeight), "Quit"))
        {
            ResourceManager.LevelName = "";
            //Application.LoadLevel("MainMenu");
            Cursor.visible = true;
            Debug.Log("Do smtg usefull and QUIT!");
        }

        GUI.EndGroup();
    }

    public void SetMetVictoryCondition(VictoryCondition victoryCondition)
    {
        if (!victoryCondition) return;
        metVictoryCondition = victoryCondition;
        winner = metVictoryCondition.GetWinner();
    }
}
