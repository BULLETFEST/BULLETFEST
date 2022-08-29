using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using UnityEngine;
using System.Collections;

public class Firebase : MonoBehaviour
{
  // private static readonly HttpClient client = new HttpClient();

  private static WebClient webClient = new WebClient();

  // bool testMode;
  // static bool testMode = false;
#if UNITY_EDITOR
  static bool testMode = true;
#else
    static bool testMode = false;
#endif

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

    // https://stackoverflow.com/a/4148390
    webClient.Headers["User-Agent"] = "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; WOW64; Trident/4.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; MDDC)";

    // JsonUtility.FromJson<Dictionary<string, string>>();

    byte[] res = new byte[0];
    try
    {
      res = await webClient.UploadValuesTaskAsync("https://joobot.glitch.me/createLobby", "POST", data);
    }
    catch
    {
      // Debug.Log(ex.Message);
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

    // https://stackoverflow.com/a/4148390
    webClient.Headers["User-Agent"] = "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; WOW64; Trident/4.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; MDDC)";

    // JsonUtility.FromJson<Dictionary<string, string>>();


    byte[] res = new byte[0];
    try
    {
      res = await webClient.UploadValuesTaskAsync(new System.Uri(testMode ? "http://localhost:3000/joinLobby" : "https://joobot.glitch.me/joinLobby"), "POST", data);
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

  public static void KeepAlive(string code)
  {
    NameValueCollection data = new NameValueCollection();

    data["code"] = code;

    webClient.UploadValuesTaskAsync(new System.Uri(testMode ? "http://localhost:3000/keepLobbyAlive" : "https://JooBot.eliasval.repl.co/keepLobbyAlive"), "POST", data);
  }

  public struct Response
  {
    public string code;
    public bool success;
    public string message;
  }
}