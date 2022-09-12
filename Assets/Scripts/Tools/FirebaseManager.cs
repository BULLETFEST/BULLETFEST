using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class FirebaseManager : MonoBehaviour
{
  // private static readonly HttpClient client = new HttpClient();
  public static string uid;

#if UNITY_EDITOR
  static bool testMode = true;
#else
      static bool testMode = true;
#endif

  public static bool Initialized
  {
    private set;
    get;
  }

  private static WebClient CreateWebClient()
  {
    WebClient client = new WebClient();
    client.Headers["User-Agent"] = "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; WOW64; Trident/4.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; MDDC)";

    return client;
  }


  public static async Task<Response<string>> HostGame()
  {
    // string ipAddress = webClient.DownloadString("http://ipinfo.io/ip");

    NameValueCollection data = new NameValueCollection();

    data["address"] = EpicTransport.EOSSDKComponent.LocalUserProductIdString;//ipAddress;
    data["token"] = SaveSystem.saveData.token;

    // JsonUtility.FromJson<Dictionary<string, string>>();

    byte[] res = new byte[0];
    try
    {
      res = await CreateWebClient().UploadValuesTaskAsync(testMode ? "http://localhost:3000/createLobby" : "https://joobot.glitch.me/createLobby", "POST", data);
    }
    catch
    {
      Message.DisplayMessage("Failed to create host", "Client failed to connect to server!", TMPro.HorizontalAlignmentOptions.Center);
      return new Response<string>(500, "SeverError", "");
    }

    string responseInString = Encoding.UTF8.GetString(res);

    // Debug.Log(responseInString);

    Response<string> response = JsonUtility.FromJson<Response<string>>(responseInString);

    return response;
  }

  public static async Task<Response<string>> JoinGame(string code)
  {
    NameValueCollection data = new NameValueCollection();

    data["code"] = code;

    // JsonUtility.FromJson<Dictionary<string, string>>();


    byte[] res = new byte[0];
    try
    {
      res = await CreateWebClient().UploadValuesTaskAsync(new System.Uri(testMode ? "http://localhost:3000/joinLobby" : "https://joobot.glitch.me/joinLobby"), "POST", data);
    }
    catch
    {
      Message.DisplayMessage("Failed to connect to game", "Client failed to connect to server!", TMPro.HorizontalAlignmentOptions.Center);
      return new Response<string>(500, "SeverError", "");
    }

    string responseInString = Encoding.UTF8.GetString(res);

    Response<string> response = JsonUtility.FromJson<Response<string>>(responseInString);

    return response;
  }

  public static void KeepAlive()
  {
    NameValueCollection data = new NameValueCollection();

    data["token"] = SaveSystem.saveData.token;

    try
    {
      CreateWebClient().UploadValuesTaskAsync(new System.Uri(testMode ? "http://localhost:3000/keepLobbyAlive" : "https://joobot.glitch.me/keepLobbyAlive"), "POST", data);
    }
    catch { }
  }

  public static Match[] GetLobbies()
  {
    byte[] res = new byte[0];
    try
    {
      res = CreateWebClient().DownloadData(new System.Uri(testMode ? "http://localhost:3000/getLobbies" : "https://joobot.glitch.me/getLobbies"));
    }
    catch (System.Exception ex)
    {
      Debug.LogError(ex.Message);
      Message.DisplayMessage("Failed to connect to game", "Client failed to connect to server!", TMPro.HorizontalAlignmentOptions.Center);
    }

    if (res.Length == 0) return new Match[0];

    string responseInString = System.Text.Encoding.Default.GetString(res);
    Match[] response = JsonHelper.FromJson<Match>(responseInString);
    return response;
  }

  public async static void UpdateLobby(int playerCount, string gameMode, string type, bool gameStarted)
  {
    NameValueCollection data = new NameValueCollection();

    data["gameMode"] = gameMode;
    data["playerCount"] = playerCount.ToString();
    data["token"] = SaveSystem.saveData.token;
    data["type"] = type;
    data["started"] = gameStarted.ToString();

    try
    {
      byte[] res = await CreateWebClient().UploadValuesTaskAsync(new System.Uri(testMode ? "http://localhost:3000/updateLobby" : "https://joobot.glitch.me/updateLobby"), "POST", data);

      string responseInString = System.Text.Encoding.Default.GetString(res);

      Response<string> response = JsonUtility.FromJson<Response<string>>(responseInString);

      print(response.ToString());
    }
    catch (System.Exception e)
    {
      print(e.Message);
    }
  }

  public static void CloseLobby()
  {
    NameValueCollection data = new NameValueCollection();

    data["token"] = SaveSystem.saveData.token;

    try
    {
      CreateWebClient().UploadValuesTaskAsync(new System.Uri(testMode ? "http://localhost:3000/closeLobby" : "https://joobot.glitch.me/closeLobby"), "POST", data);
    }
    catch { }
  }

  public async static Task<Response<string>> Login(string token)
  {
    if (!string.IsNullOrEmpty(token)) return new Response<string>(400, "Already logged in", "");

    NameValueCollection data = new NameValueCollection();

    data["token"] = SaveSystem.saveData.token;

    byte[] res = new byte[0];
    try
    {
      res = await CreateWebClient().UploadValuesTaskAsync(new System.Uri(testMode ? "http://localhost:3000/loginUserToken" : "https://joobot.glitch.me/loginUserToken"), "POST", data);
    }
    catch
    {
      return new Response<string>(500, "SeverError", "");
    }

    string responseInString = System.Text.Encoding.Default.GetString(res);

    Response<string> response = JsonUtility.FromJson<Response<string>>(responseInString);

    return response;
  }

  public async static Task<Response<string>> Login(string email, string password)
  {
    if (!string.IsNullOrEmpty(SaveSystem.saveData.token)) return new Response<string>(400, "Already logged in", "");

    NameValueCollection data = new NameValueCollection();

    data["email"] = email;
    data["password"] = password;

    byte[] res = new byte[0];
    try
    {
      res = await CreateWebClient().UploadValuesTaskAsync(new System.Uri(testMode ? "http://localhost:3000/loginUser" : "https://joobot.glitch.me/loginUser"), "POST", data);
    }
    catch
    {
      return new Response<string>(500, "SeverError", "");
    }

    string responseInString = System.Text.Encoding.Default.GetString(res);

    Response<string> response = JsonUtility.FromJson<Response<string>>(responseInString);

    return response;
  }

  public async static Task<Response<string>> CreateUser(string email, string password)
  {
    if (!string.IsNullOrEmpty(SaveSystem.saveData.token)) return new Response<string>(400, "Already logged in", "");

    NameValueCollection data = new NameValueCollection();

    data["email"] = email;
    data["password"] = password;


    byte[] res = new byte[0];
    try
    {
      res = await CreateWebClient().UploadValuesTaskAsync(new System.Uri(testMode ? "http://localhost:3000/createUser" : "https://joobot.glitch.me/createUser"), "POST", data);
    }
    catch
    {
      return new Response<string>(500, "SeverError", "");
    }

    string responseInString = System.Text.Encoding.Default.GetString(res);

    Response<string> response = JsonUtility.FromJson<Response<string>>(responseInString);

    return response;
  }

  public static Response<bool> ValidateToken(string token)
  {
    if (string.IsNullOrEmpty(token)) return new Response<bool>(200, "", false);

    NameValueCollection data = new NameValueCollection();

    data["token"] = SaveSystem.saveData.token;

    byte[] res = new byte[0];
    try
    {
      res = CreateWebClient().UploadValues(new System.Uri(testMode ? "http://localhost:3000/validateToken" : "https://joobot.glitch.me/validateToken"), "POST", data);
    }
    catch
    {
      return new Response<bool>(500, "SeverError", false);
    }

    string responseInString = System.Text.Encoding.Default.GetString(res);

    Response<bool> response = JsonUtility.FromJson<Response<bool>>(responseInString);

    return response;
  }

  public static void SignOut()
  {
    SaveSystem.saveData.token = "";
    SaveSystem.SavePlayer(SaveSystem.saveData);
    SceneManager.LoadScene("Initialization");
  }

  public static Response<string> CheckServerStatus()
  {
    byte[] res;
    try
    {
      res = CreateWebClient().DownloadData(new System.Uri(testMode ? "http://localhost:3000/getLatestVer" : "https://joobot.glitch.me/getLatestVer"));
    }
    catch (System.Exception e)
    {
      Debug.Log(e.Message);
      return new Response<string>(500, "", "");
    }


    string responseInString = System.Text.Encoding.Default.GetString(res);

    Response<string> response = JsonUtility.FromJson<Response<string>>(responseInString);

    return response;
  }

  [System.Serializable]
  public class Response<T>
  {
    public int status;
    public string message;
    public T data;

    public Response(int status, string message, T data)
    {
      this.status = status;
      this.message = message;
      this.data = data;
    }

    public override string ToString()
    {
      return $@"{{ 
        Status: {status},
        Message: {message},
        data: {data?.ToString()}
      }}";
    }
  }

  [System.Serializable]
  public class Match
  {
    public string code;
    public string gameMode;
    public string playerCount;
    public bool started;

    public override string ToString()
    {
      return $"Code: {code}; Mode: {gameMode}; Players: {playerCount}";
    }
  }
}