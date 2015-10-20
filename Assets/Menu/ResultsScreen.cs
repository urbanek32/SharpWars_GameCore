using System;
using UnityEngine;
using System.Collections;
using Newtonsoft.Json;

using RTS;

public class ResultsScreen : MonoBehaviour
{
    public GUISkin Skin;
    public string DescriptionWin { get; set; }
    public string PlayerWinner { get; set; }
    public bool LocalPlayerWin { get; set; }

    //private VictoryCondition metVictoryCondition;
    public Player Player;


    void OnGUI()
    {
        GUI.skin = Skin;
        GUI.BeginGroup(new Rect(0, 0, Screen.width, Screen.height));
        GUI.depth = 0;

        Cursor.visible = true;
        string message;

        //display 
        float padding = ResourceManager.Padding;
        float itemHeight = ResourceManager.ButtonHeight;
        float buttonWidth = ResourceManager.ButtonWidth;
        float leftPos = padding;
        float topPos = padding;
        GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "");      
        
        if (LocalPlayerWin)
        {
            message = string.Format("Congratulations {0}!\nYou have won by {1}", PlayerWinner, DescriptionWin);
            
        }
        else
        {
            message = string.Format("You loose!\nThe {0} have won by {1}", PlayerWinner, DescriptionWin);
        }

        GUI.Label(new Rect(leftPos, topPos, Screen.width - 2 * padding, itemHeight), message);
        leftPos = Screen.width / 2f - padding / 2f - buttonWidth;
        topPos += itemHeight + padding;
        if (GUI.Button(new Rect(leftPos, topPos, buttonWidth, itemHeight), "Send score"))
        {
            //makes sure that the loaded level runs at normal speed
            Time.timeScale = 1.0f;
            //ResourceManager.MenuOpen = false;
            //Application.LoadLevel("Map");
            //var wc = GetComponentInParent<Player>().WebsiteCommunication;
            Player.WebsiteCommunication.FakeSendScoreToCloud(Convert.ToInt32(Player.netId.Value), (int)Time.timeSinceLevelLoad, LocalPlayerWin);

            //MakeGetRequest();
            //MakePostRequest();

        }
        leftPos += padding + buttonWidth;
        if (GUI.Button(new Rect(leftPos, topPos, buttonWidth, itemHeight), "Quit"))
        {
            //ResourceManager.LevelName = "";
            //Application.LoadLevel("MainMenu");
            
            Debug.Log("Do smtg usefull and QUIT!");
            Application.Quit();
        }

        GUI.EndGroup();
    }

    private void MakeGetRequest()
    {
        var client = new WWW("http://eti.endrius.tk/test/api/test_empty_body");
        StartCoroutine(WaitForRequest(client));
    }

    private void MakePostRequest()
    {
        var form = new WWWForm();
        form.AddField("word", "dupa blada nie jest zla");
        var client = new WWW("http://eti.endrius.tk/test/api/test_string_body", form);
        StartCoroutine(WaitForRequest(client));
    }

    private static IEnumerator WaitForRequest(WWW www)
    {
        yield return www;

        // check for errors
        if (www.error == null)
        {
            Debug.Log("WWW Ok!: " + www.text);
            var obj = JsonConvert.DeserializeObject<CustomJsonResponse>(www.text);
            Debug.Log(obj.Message);
        }
        else
        {
            Debug.Log("WWW Error: " + www.error);
        }  
    }
}
