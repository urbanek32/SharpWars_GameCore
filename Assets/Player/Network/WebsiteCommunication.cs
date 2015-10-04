//Zrobione w oparciu o wiki z dnia 03-04.10.2015

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using STL;

//JSON token response format
class TokenResponse
{
    public string token { get; set; }
}

class ScriptBody
{
    public string name { get; set; }
    public string description { get; set; }
    public string code { get; set; }
}

class ScriptStatusResponse
{
    public int status { get; set; }
    public string group { get; set; }
    public string ID { get; set; }
}

class ListScriptResponse
{
    public string name { get; set; }
    public string description { get; set; }
    public string code { get; set; }
    public string _id { get; set; }
}

//[03.10.2015] Aktualnie blokujące wykonanie
public class WebsiteCommunication : MonoBehaviour
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
    public static string GetToken(string username, string password)
    {
        string url = SOCIAL_WEBSITE + SOCIAL_GETTOKEN;

        Dictionary<string, string> headers = new Dictionary<string, string>();

        headers["Content-Type"] = "application/json";

        string ascii_data = "{ \"username\": \"" + username + "\", \"password\": \"" + password + "\" }";
        byte[] rawData = System.Text.Encoding.ASCII.GetBytes(ascii_data);
        
        WWW www_req = new WWW(url, rawData, headers);

        while (!www_req.isDone) { }

        TokenResponse token = JsonConvert.DeserializeObject<TokenResponse>(www_req.text);

        return token.token;
    }

    //GET
    //null lub lista par<string, string> gdzie <nazwa, kod>
    public static List<Pair<string, string>> GetScriptsFromCloud(string username, string token)
    {
        string url = SOCIAL_WEBSITE + SOCIAL_AUTH_BASE + username + "/scripts/list";

        Dictionary<string, string> headers = new Dictionary<string, string>();

        headers["Content-Type"] = "application/json";
        headers["Authorization"] = "Bearer " + token;

        WWW www_req = new WWW(url, null, headers);

        while (!www_req.isDone) { }


        if (www_req.text != "Unauthorized")
        {
            List<ListScriptResponse> lsr = JsonConvert.DeserializeObject<List<ListScriptResponse>>(www_req.text);

            if (lsr.Count > 0)
            {
                List<Pair<string, string>> ret_val = new List<Pair<string, string>>();
                foreach (ListScriptResponse i in lsr)
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
    public static bool AddScriptToCloud(string username, string token, string script_name, string script_description, string script_code)
    {
        bool ret_val = false;

        string url = SOCIAL_WEBSITE + SOCIAL_AUTH_BASE + username + "/scripts/add";

        Dictionary<string, string> headers = new Dictionary<string, string>();

        headers["Content-Type"] = "application/json";
        headers["Authorization"] = "Bearer " + token;

        //check name, description, code
        script_name = ValidateString(script_name);
        script_description = ValidateString(script_description);
        script_code = ValidateString(script_code);

        string ascii_data = "{ \"name\": \"" + script_name + "\", \"description\": \"" + script_description + "\", \"code\": \"" + script_code + "\" }";
        byte[] rawData = System.Text.Encoding.ASCII.GetBytes(ascii_data);

        WWW www_req = new WWW(url, rawData, headers);

        while (!www_req.isDone) { }

        Debug.Log(www_req.text);

        ScriptStatusResponse ssr = JsonConvert.DeserializeObject<ScriptStatusResponse>(www_req.text);

        if (ssr.status == 201)
        {
            ret_val = true;
        }

        return ret_val;
    }

    //POST
    //zwraca true jeśli chmura zatwierdzi, inaczej false
    public static bool EditScriptInCloud(string username, string token, string script_name, string script_description, string script_code)
    {
        bool ret_val = false;

        //check name, description, code
        script_name = ValidateString(script_name);
        script_description = ValidateString(script_description);
        script_code = ValidateString(script_code);

        string url = SOCIAL_WEBSITE + SOCIAL_AUTH_BASE + username + "/scripts/update/" + script_name;

        Dictionary<string, string> headers = new Dictionary<string, string>();

        headers["Content-Type"] = "application/json";
        headers["Authorization"] = "Bearer " + token;

        string ascii_data = "{ \"name\": \"" + script_name + "\", \"description\": \"" + script_description + "\", \"code\": \"" + script_code + "\" }";
        byte[] rawData = System.Text.Encoding.ASCII.GetBytes(ascii_data);

        WWW www_req = new WWW(url, rawData, headers);

        while (!www_req.isDone) { }

        ScriptStatusResponse ssr = JsonConvert.DeserializeObject<ScriptStatusResponse>(www_req.text);

        if (ssr.status == 204)
        {
            ret_val = true;
        }

        return ret_val;
    }

    //GET
    //zwraca null jak coś pójdzie źle, inaczej skrypt <nazwa, kod>
    public static Pair<string, string> GetScriptFromCloud(string username, string token, string script_name)
    {
        script_name = ValidateString(script_name);

        string url = SOCIAL_WEBSITE + SOCIAL_AUTH_BASE + username + "/scripts/" + script_name;

        Dictionary<string, string> headers = new Dictionary<string, string>();

        headers["Content-Type"] = "application/json";
        headers["Authorization"] = "Bearer " + token;

        WWW www_req = new WWW(url, null, headers);

        while (!www_req.isDone) { }

        ListScriptResponse lsr = JsonConvert.DeserializeObject<ListScriptResponse>(www_req.text);

        if (lsr != null)
        {
            return new Pair<string,string>(lsr.name, lsr.code);
        }

        return null;
    }
}
