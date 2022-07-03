using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Threading.Tasks;
using System.Text;
using Newtonsoft.Json;

public class Firebase// : MonoBehaviour
{
  // private static readonly HttpClient client = new HttpClient();

  private static WebClient webClient = new WebClient();

  // bool testMode;
#if UNITY_EDITOR
  static bool testMode = true;
#else
  static bool testMode = false;
#endif

  /// <summary>
  /// Attempt to host a game
  /// </summary>
  /// <returns>Bool: Room ID</returns>
  public static Response HostGame()
  {
    string ipAddress = webClient.DownloadString("http://ipinfo.io/ip");

    NameValueCollection data = new NameValueCollection();

    data["address"] = ipAddress;
    data["userId"] = SystemInfo.deviceUniqueIdentifier;

    // JsonUtility.FromJson<Dictionary<string, string>>();

    byte[] res = webClient.UploadValues(testMode ? "http://localhost:3000/createLobby" : "https://JooBot.eliasval.repl.co/createLobby", "POST", data);

    if (res == null) return new Response
    {
      code = "",
      success = false,
      message = "Something went wrong!"
    };

    string responseInString = Encoding.UTF8.GetString(res);

    // Debug.Log(responseInString);

    Response response = JsonUtility.FromJson<Response>(responseInString);

    return response;
  }

  public static Response JoinGame(string code)
  {
    Debug.Log(code);
    NameValueCollection data = new NameValueCollection();

    data["code"] = code;

    // JsonUtility.FromJson<Dictionary<string, string>>();

    byte[] res = webClient.UploadValues(testMode ? "http://localhost:3000/joinLobby" : "https://JooBot.eliasval.repl.co/joinLobby", "POST", data);

    if (res == null) return new Response
    {
      code = "",
      success = false,
      message = "Something went wrong!"
    };

    string responseInString = Encoding.UTF8.GetString(res);

    Response response = JsonUtility.FromJson<Response>(responseInString);

    Debug.Log(response);
    Debug.Log(responseInString);


    return response;
  }

  public struct Response
  {
    public string code;
    // JSON support in C# is... yeah... don't ask...
    public bool success;
    public string message;
  }
}