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
  static bool testMode = false;
  // #if UNITY_EDITOR
  //   static bool testMode = true;
  // #else
  //     static bool testMode = false;
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

    // https://stackoverflow.com/a/4148390
    webClient.Headers["User-Agent"] = "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; WOW64; Trident/4.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; MDDC)";

    // JsonUtility.FromJson<Dictionary<string, string>>();

    byte[] res = new byte[0];
    try
    {
      res = await webClient.UploadValuesTaskAsync(testMode ? "http://localhost:3000/createLobby" : "https://joobot.glitch.me/createLobby", "POST", data);
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

    // https://stackoverflow.com/a/4148390
    webClient.Headers["User-Agent"] = "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; WOW64; Trident/4.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; MDDC)";

    // JsonUtility.FromJson<Dictionary<string, string>>();


    byte[] res = new byte[0];
    try
    {
      res = await webClient.UploadValuesTaskAsync(new System.Uri(testMode ? "http://localhost:3000/joinLobby" : "https://joobot.glitch.me/joinLobby"), "POST", data);
    }
    catch (System.Exception ex)
    {
      Debug.LogError(ex.Message);
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

  public static void KeepAlive()
  {
    NameValueCollection data = new NameValueCollection();

    data["userId"] = SystemInfo.deviceUniqueIdentifier;
    webClient.Headers["User-Agent"] = "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; WOW64; Trident/4.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; MDDC)";

    try
    {
      webClient.UploadValuesTaskAsync(new System.Uri(testMode ? "http://localhost:3000/keepLobbyAlive" : "https://joobot.glitch.me/keepLobbyAlive"), "POST", data);
    }
    catch { }
  }

  public static async Task<Match[]> getLobbies()
  {
    // https://stackoverflow.com/a/4148390
    webClient.Headers["User-Agent"] = "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; WOW64; Trident/4.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; MDDC)";

    byte[] res = new byte[0];
    try
    {
      res = await webClient.UploadValuesTaskAsync(new System.Uri(testMode ? "http://localhost:3000/getLobbies" : "https://joobot.glitch.me/getLobbies"), "POST", new NameValueCollection());
    }
    catch (System.Exception ex)
    {
      Debug.LogError(ex.Message);
      Message.DisplayMessage("Failed to connect to game", "Client failed to connect to server!", TMPro.HorizontalAlignmentOptions.Center);
    }

    if (res.Length == 0) return new Match[0];

    string responseInString = Encoding.UTF8.GetString(res);

    GetLobbiesResponse response = JsonUtility.FromJson<GetLobbiesResponse>(responseInString);

    return response.matches;
  }

  public static void UpdateLobby(int playerCount, string gameMode, string type)
  {
    NameValueCollection data = new NameValueCollection();

    webClient.Headers["User-Agent"] = "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; WOW64; Trident/4.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; MDDC)";

    data["gameMode"] = gameMode;
    data["playerCount"] = playerCount.ToString();
    data["userId"] = SystemInfo.deviceUniqueIdentifier;
    data["type"] = type;

    try
    {
      webClient.UploadValuesTaskAsync(new System.Uri(testMode ? "http://localhost:3000/updateLobby" : "https://joobot.glitch.me/updateLobby"), "POST", data);
    }
    catch { }
  }

  public static void CloseLobby()
  {
    NameValueCollection data = new NameValueCollection();

    data["userId"] = SystemInfo.deviceUniqueIdentifier;
    webClient.Headers["User-Agent"] = "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; WOW64; Trident/4.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; MDDC)";

    try
    {
      webClient.UploadValuesTaskAsync(new System.Uri(testMode ? "http://localhost:3000/closeLobby" : "https://joobot.glitch.me/closeLobby"), "POST", data);
    }
    catch { }
  }

  public struct Response
  {
    public string code;
    public bool success;
    public string message;
  }

  [System.Serializable]
  public class GetLobbiesResponse
  {
    public Match[] matches;
    public bool success;
  }

  [System.Serializable]
  public class Match
  {
    public string code;
    public string gameMode;
    public string playerCount;

    public override string ToString()
    {
      return $"Code: {code}; Mode: {gameMode}; Players: {playerCount}";
    }
  }
}