//Zrobione w oparciu o wiki z dnia 03-04.10.2015

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using STL;

//[03.10.2015] Aktualnie blokujące wykonanie
public class WebsiteCommunication
{
    //Kiedyś nastanie era HTTP/TLS...
    private const string SOCIAL_WEBSITE = "http://eti.endrius.tk";
    private const string SOCIAL_GETTOKEN = "/api/users/login";
    private const string SOCIAL_AUTH_BASE = "/auth/api/users/";

    private static string ValidateString(string str)
    {
        if (str == null || str == "")
        {
            return "Default";
        }

        return str;
    }

    //POST
    //Logowanie, zwracany token(string) lub null(null)
    public string GetToken(string username, string password)
    {
        var url = SOCIAL_WEBSITE + SOCIAL_GETTOKEN;

        var headers = new Dictionary<string, string>();

        headers["Content-Type"] = "application/json";

        var ascii_data = "{ \"username\": \"" + username + "\", \"password\": \"" + password + "\" }";
        var rawData = System.Text.Encoding.ASCII.GetBytes(ascii_data);

        var www_req = new WWW(url, rawData, headers);

        while (!www_req.isDone) { }

        var token = JsonConvert.DeserializeObject<TokenResponse>(www_req.text);

        return token.token;
    }

    //GET
    //null lub lista par<string, string> gdzie <nazwa, kod>
    public List<Pair<string, string>> GetScriptsFromCloud(string username, string token)
    {
        var url = SOCIAL_WEBSITE + SOCIAL_AUTH_BASE + username + "/scripts/list";

        var headers = new Dictionary<string, string>();

        headers["Content-Type"] = "application/json";
        headers["Authorization"] = "Bearer " + token;

        var www_req = new WWW(url, null, headers);

        while (!www_req.isDone) { }


        if (www_req.text != "Unauthorized")
        {
            var lsr = JsonConvert.DeserializeObject<List<ListScriptResponse>>(www_req.text);

            if (lsr.Count > 0)
            {
                var ret_val = new List<Pair<string, string>>();
                foreach (var i in lsr)
                {
                    ret_val.Add(new Pair<string, string>(i.name, i.code));
                }

                return ret_val;
            }
        }

        return null;
    }

    //POST
    //zwraca true jeśli się wepchnie do chmury, inaczej false
    public bool AddScriptToCloud(string username, string token, string script_name, string script_description, string script_code)
    {
        var ret_val = false;

        var url = SOCIAL_WEBSITE + SOCIAL_AUTH_BASE + username + "/scripts/add";

        var headers = new Dictionary<string, string>();

        headers["Content-Type"] = "application/json";
        headers["Authorization"] = "Bearer " + token;

        //check name, description, code
        script_name = ValidateString(script_name);
        script_description = ValidateString(script_description);
        script_code = ValidateString(script_code);

        var ascii_data = "{ \"name\": \"" + script_name + "\", \"description\": \"" + script_description + "\", \"code\": \"" + script_code + "\" }";
        var rawData = System.Text.Encoding.ASCII.GetBytes(ascii_data);

        var www_req = new WWW(url, rawData, headers);

        while (!www_req.isDone) { }

        var ssr = JsonConvert.DeserializeObject<ScriptStatusResponse>(www_req.text);

        if (ssr.status == 201)
        {
            ret_val = true;
        }

        return ret_val;
    }

    //POST
    //zwraca true jeśli chmura zatwierdzi, inaczej false
    public bool EditScriptInCloud(string username, string token, string script_name, string script_description, string script_code)
    {
        var ret_val = false;

        //check name, description, code
        script_name = ValidateString(script_name);
        script_description = ValidateString(script_description);
        script_code = ValidateString(script_code);

        var url = SOCIAL_WEBSITE + SOCIAL_AUTH_BASE + username + "/scripts/update/" + script_name;

        var headers = new Dictionary<string, string>();

        headers["Content-Type"] = "application/json";
        headers["Authorization"] = "Bearer " + token;

        var ascii_data = "{ \"name\": \"" + script_name + "\", \"description\": \"" + script_description + "\", \"code\": \"" + script_code + "\" }";
        var rawData = System.Text.Encoding.ASCII.GetBytes(ascii_data);

        var www_req = new WWW(url, rawData, headers);

        while (!www_req.isDone) { }

        var ssr = JsonConvert.DeserializeObject<ScriptStatusResponse>(www_req.text);

        if (ssr.status == 204)
        {
            ret_val = true;
        }

        return ret_val;
    }

    //GET
    //zwraca null jak coś pójdzie źle, inaczej skrypt <nazwa, kod>
    public Pair<string, string> GetScriptFromCloud(string username, string token, string script_name)
    {
        script_name = ValidateString(script_name);

        var url = SOCIAL_WEBSITE + SOCIAL_AUTH_BASE + username + "/scripts/" + script_name;

        var headers = new Dictionary<string, string>();

        headers["Content-Type"] = "application/json";
        headers["Authorization"] = "Bearer " + token;

        var www_req = new WWW(url, null, headers);

        while (!www_req.isDone) { }

        var lsr = JsonConvert.DeserializeObject<ListScriptResponse>(www_req.text);

        if (lsr != null)
        {
            return new Pair<string, string>(lsr.name, lsr.code);
        }

        return null;
    }
}