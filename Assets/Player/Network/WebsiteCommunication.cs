//Zrobione w oparciu o wiki z dnia 03-04.10.2015

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using RTS;
using STL;

//[03.10.2015] Aktualnie blokujące wykonanie
public class WebsiteCommunication : MonoBehaviour
{
    public delegate void HandleOnSuccess(string response_value, object caller);
    public delegate void HandleOnError(object caller);

    //Kiedyś nastanie era HTTP/TLS...
    private const string SOCIAL_WEBSITE = "http://eti.endrius.tk";
    private const string SOCIAL_GETTOKEN = "/api/users/login";
    private const string SOCIAL_AUTH_BASE = "/auth/api/users/";

    private static string ValidateString(string str)
    {
        return string.IsNullOrEmpty(str) ? "Default" : str;
    }

    private static IEnumerator WaitForRequest(WWW wwwReq, HandleOnError hoe, HandleOnSuccess hos, object caller)
    {
        Debug.Log("Starting waiting 4 request...");
        yield return wwwReq;

        if (wwwReq.error == null)
        {
            Debug.Log("WWW Success: " + wwwReq.text);

            if (hos != null)
            {
                hos(wwwReq.text, caller);
            }
        }
        else
        {
            Debug.LogWarning("WWW error: " + wwwReq.error);

            if (hoe != null)
            {
                hoe(caller);
            }
        }
    }

    private static void HandleToken(string token, object caller)
    {
        var _token = JsonConvert.DeserializeObject<TokenResponse>(token);

        var player = caller as Player;
        if (player == null) return;

        // just 4 testing
        ResourceManager.PlayerToken = _token.token;
        //ResourceManager.PlayerName = "janusz";

        Debug.Log("Token set to: " + ResourceManager.PlayerToken);
    }

    public static void HandleScriptList(string scripts, object caller)
    {
        if (scripts != "Unauthorized")
        {
            var lsr = JsonConvert.DeserializeObject<List<ListScriptResponse>>(scripts);

            var convertedList = new List<Pair<string, string>>();

            if (lsr.Count > 0)
            {
                convertedList.AddRange(lsr.Select(i => new Pair<string, string>(i.name, i.code)));
            }

            var player = caller as Player;
            if (player == null) return;

            Player p = player;
            p.scriptList = convertedList;
            p.ScriptFromCloudStatus = GetScriptStatus.Ready;
        }
        else
        {
            Debug.LogWarning("Can not download script list! Unauthorized");
        }
    }

    void Awake()
    {
        //EXAMPLUM
        //GetToken("janusz", "testowy", null, HandleToken, gameObject.GetComponent<Player>());
    }

    //POST
    //Logowanie, zwracany token(string) lub null(null)
    public void GetToken(string username, string password, HandleOnError hoe, HandleOnSuccess hos, object caller)
    {
        const string url = SOCIAL_WEBSITE + SOCIAL_GETTOKEN;

        var headers = new Dictionary<string, string>();

        headers["Content-Type"] = "application/json";

        var playerCredentials = new LoginRequest
        {
            username = username,
            password = password
        };

        var jsonData = JsonConvert.SerializeObject(playerCredentials);
        var rawData = System.Text.Encoding.ASCII.GetBytes(jsonData);

        var wwwReq = new WWW(url, rawData, headers);

        StartCoroutine(WaitForRequest(wwwReq, null, HandleToken, caller));
    }

    //GET
    //null lub lista par<string, string> gdzie <nazwa, kod>
    public void GetScriptsFromCloud(string username, string token, HandleOnError hoe, HandleOnSuccess hos, object caller)
    {
        var url = string.Format("{0}{1}{2}{3}", SOCIAL_WEBSITE, SOCIAL_AUTH_BASE, username, "/scripts/list");

        var headers = new Dictionary<string, string>();

        headers["Content-Type"] = "application/json";
        headers["Authorization"] = "Bearer " + token;

        var wwwReq = new WWW(url, null, headers);

        StartCoroutine(WaitForRequest(wwwReq, hoe, hos, caller));
    }

    //POST
    //zwraca true jeśli się wepchnie do chmury, inaczej false
    public void AddScriptToCloud(string username, string token, string script_name, string script_description, string script_code, HandleOnError hoe, HandleOnSuccess hos, object caller)
    {
        var url = string.Format("{0}{1}{2}{3}", SOCIAL_WEBSITE, SOCIAL_AUTH_BASE, username, "/scripts/add");

        var headers = new Dictionary<string, string>();

        headers["Content-Type"] = "application/json";
        headers["Authorization"] = "Bearer " + token;

        //check name, description, code
        script_name = ValidateString(script_name);
        script_description = ValidateString(script_description);
        script_code = ValidateString(script_code);

        var scriptBody = new ScriptBody
        {
            name = script_name,
            code = script_code,
            description = script_description
        };

        var scriptRequest = JsonConvert.SerializeObject(scriptBody);
        var rawData = System.Text.Encoding.ASCII.GetBytes(scriptRequest);

        var wwwReq = new WWW(url, rawData, headers);

        StartCoroutine(WaitForRequest(wwwReq, hoe, hos, caller));

        /*var ssr = JsonConvert.DeserializeObject<ScriptStatusResponse>(www_req.text);

        if (ssr.status == 201)
        {
            ret_val = true;
        }*/
    }

    //POST
    //zwraca true jeśli chmura zatwierdzi, inaczej false
    public void EditScriptInCloud(string username, string token, string script_name, string script_description, string script_code, HandleOnError hoe, HandleOnSuccess hos, object caller)
    {
        var url = string.Format("{0}{1}{2}{3}{4}", SOCIAL_WEBSITE, SOCIAL_AUTH_BASE, username, "/scripts/update/", script_name.Replace(" ", "%20"));
        
        var headers = new Dictionary<string, string>();

        headers["Content-Type"] = "application/json";
        headers["Authorization"] = "Bearer " + token;

        //check name, description, code
        script_name = ValidateString(script_name);
        script_description = ValidateString(script_description);
        script_code = ValidateString(script_code);

        var scriptBody = new ScriptBody
        {
            name = script_name,
            code = script_code,
            description = script_description
        };

        var scriptRequest = JsonConvert.SerializeObject(scriptBody);
        var rawData = System.Text.Encoding.ASCII.GetBytes(scriptRequest);

        var wwwReq = new WWW(url, rawData, headers);

        StartCoroutine(WaitForRequest(wwwReq, hoe, hos, caller));

        /*var ssr = JsonConvert.DeserializeObject<ScriptStatusResponse>(www_req.text);

        if (ssr.status == 204)
        {
            ret_val = true;
        }*/
    }

    //GET
    //zwraca null jak coś pójdzie źle, inaczej skrypt <nazwa, kod>
    public void GetScriptFromCloud(string username, string token, string script_name, HandleOnError hoe, HandleOnSuccess hos, object caller)
    {
        script_name = ValidateString(script_name);

        var url = string.Format("{0}{1}{2}{3}{4}", SOCIAL_WEBSITE, SOCIAL_AUTH_BASE, username, "/scripts/", script_name.Replace(" ", "%20"));

        var headers = new Dictionary<string, string>();

        headers["Content-Type"] = "application/json";
        headers["Authorization"] = "Bearer " + token;

        var wwwReq = new WWW(url, null, headers);

        StartCoroutine(WaitForRequest(wwwReq, hoe, hos, caller));

        /*var lsr = JsonConvert.DeserializeObject<ListScriptResponse>(www_req.text);

        if (lsr != null)
        {
            return new Pair<string, string>(lsr.name, lsr.code);
        }*/
    }

    public void SendScoreToCloud(int score, int gameTime, bool win)
    {
        Debug.Log("Sending score...");
        var url = string.Format("{0}{1}{2}{3}", SOCIAL_WEBSITE, SOCIAL_AUTH_BASE, "janusz", "/game/scores");

        var headers = new Dictionary<string, string>();

        headers["Content-Type"] = "application/json";
        headers["Authorization"] = "Bearer " + ResourceManager.PlayerToken;

        var scoreBody = new ScoreRequest
        {
            score = score,
            gameTime = gameTime,
            winner = Convert.ToInt32(win),
        };

        var scoreRequest = JsonConvert.SerializeObject(scoreBody);
        var rawData = System.Text.Encoding.ASCII.GetBytes(scoreRequest);

        var wwwReq = new WWW(url, rawData, headers);

        StartCoroutine(WaitForRequest(wwwReq, null, null, null));
    }

    public void FakeSendScoreToCloud(int score, int gameTime, bool win)
    {
        Debug.LogFormat("Wysylam: {0} {1} {2}", score, gameTime, win);
    }
}