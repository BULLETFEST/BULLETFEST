using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using UnityEngine;

public class Firebase// : MonoBehaviour
{
  // private static readonly HttpClient client = new HttpClient();

  private static WebClient webClient = new WebClient();

  // bool testMode;
  static bool testMode = false;
  // #if UNITY_EDITOR
  //   static bool testMode = true;
  // #else
  //   static bool testMode = false;
  // #endif

  /// <summary>
  /// Attempt to host a game
  /// </summary>
  /// <returns>Bool: Room ID</returns>
  public static async Task<Response> HostGame()
  {
    // string ipAddress = webClient.DownloadString("http://ipinfo.io/ip");

    NameValueCollection data = new NameValueCollection();

    data["address"] = EpicTransport.EOSSDKComponent.LocalUserProductIdString;//ipAddress;
    data["userId"] = SystemInfo.deviceUniqueIdentifier;

    // JsonUtility.FromJson<Dictionary<string, string>>();

    byte[] res = new byte[0];
    try
    {
      res = await webClient.UploadValuesTaskAsync(testMode ? "http://localhost:3000/createLobby" : "https://JooBot.eliasval.repl.co/createLobby", "POST", data);
    }
    catch
    {
      Message.DisplayMessage("Failed to create host", "Client failed to connect to server!", TMPro.HorizontalAlignmentOptions.Center);
    }

    if (res.Length == 0) return new Response
    {
      code = "",
      success = false,
      message = "sErr"
    };

    string responseInString = Encoding.UTF8.GetString(res);

    // Debug.Log(responseInString);

    Response response = JsonUtility.FromJson<Response>(responseInString);

    return response;
  }

  public static async Task<Response> JoinGame(string code)
  {
    NameValueCollection data = new NameValueCollection();

    data["code"] = code;

    // JsonUtility.FromJson<Dictionary<string, string>>();


    byte[] res = new byte[0];
    try
    {
      res = await webClient.UploadValuesTaskAsync(new System.Uri(testMode ? "http://localhost:3000/joinLobby" : "https://JooBot.eliasval.repl.co/joinLobby"), "POST", data);
    }
    catch
    {
      Message.DisplayMessage("Failed to connect to game", "Client failed to connect to server!", TMPro.HorizontalAlignmentOptions.Center);
    }

    if (res.Length == 0) return new Response
    {
      code = "",
      success = false,
      message = "sErr"
    };

    string responseInString = Encoding.UTF8.GetString(res);

    Response response = JsonUtility.FromJson<Response>(responseInString);

    return response;
  }

  public struct Response
  {
    public string code;
    public bool success;
    public string message;
  }
}