using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FirebaseManager : MonoBehaviour
{
  public static string uid;

  /* UTILITY METHODS */

  private static WebClient CreateWebClient()
  {
    WebClient client = new();
    client.Headers["User-Agent"] = "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; WOW64; Trident/4.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; MDDC)";

    return client;
  }

  private static async Task<Response<T>> CreateRequest<T>(string endpoint, NameValueCollection data = null, HTTPMethod method = HTTPMethod.Get)
  {
    WebClient wc = CreateWebClient();

    byte[] _res = new byte[0];
    Response<T> res = new(0, "", default);

    try
    {
      if (method == HTTPMethod.Post)
      {
        _res = await wc.UploadValuesTaskAsync(Globals._firebaseTestMode ? $"http://localhost:3000/{endpoint}" : $"https://joobot.glitch.me/{endpoint}", method.ToString().ToUpper(), data);
      }
      else if (method == HTTPMethod.Get)
      {
        _res = await wc.DownloadDataTaskAsync(Globals._firebaseTestMode ? $"http://localhost:3000/{endpoint}" : $"https://joobot.glitch.me/{endpoint}");
      }
    }
    catch (System.Exception e)
    {
      // Message.DisplayMessage("Failed to create host", "Client failed to connect to server!", TMPro.HorizontalAlignmentOptions.Center);
      Debug.Log(e.Message);
      res = new Response<T>(500, "ServerError", default);
    }

    if (_res.Length > 1)
    {
      string responseInString = Encoding.UTF8.GetString(_res);

      res = JsonUtility.FromJson<Response<T>>(responseInString);
    }

    // wc.Dispose();

    return res;
  }


  /* LOBBY METHODS */

  public static async Task<Response<string>> HostGame()
  {
    // string ipAddress = webClient.DownloadString("http://ipinfo.io/ip");

    NameValueCollection data = new()
    {
      ["address"] = EpicTransport.EOSSDKComponent.LocalUserProductIdString,//ipAddress;
      ["token"] = SaveSystem.saveData.token
    };

    Response<string> response = await CreateRequest<string>("createLobby", data, HTTPMethod.Post);

    if (response.status == 500)
    {
      Message.DisplayMessage("Failed to create host", "Client failed to connect to server!", TMPro.HorizontalAlignmentOptions.Center);
    }

    return response;
  }

  public static async Task<Response<string>> JoinGame(string code)
  {
    NameValueCollection data = new()
    {
      ["code"] = code
    };

    Response<string> response = await CreateRequest<string>("joinLobby", data, HTTPMethod.Post);

    return response;
  }

  public static void KeepAlive()
  {
    NameValueCollection data = new()
    {
      ["token"] = SaveSystem.saveData.token
    };

    _ = CreateRequest<Response<string>>("keepLobbyAlive", data, HTTPMethod.Post);
  }

  public static async Task<Match[]> GetLobbies()
  {
    return (await CreateRequest<Match[]>("getLobbies")).data;
  }

  public static async void UpdateLobby(int playerCount)
  {
    NameValueCollection data = new()
    {
      ["gameMode"] = GameManager.settings.gameMode.ToString(),
      ["playerCount"] = playerCount.ToString(),
      ["token"] = SaveSystem.saveData.token,
      ["type"] = GameManager.settings.privacyType.ToString().ToLower(),
      ["started"] = (GameManager.Instance.state == GameManager.GameState.Started).ToString(),
      ["lobbySize"] = GameManager.settings.lobbySize.ToString()
    };

    await CreateRequest<string>("updateLobby", data, HTTPMethod.Post);
  }

  public static void CloseLobby()
  {
    NameValueCollection data = new()
    {
      ["token"] = SaveSystem.saveData.token
    };

    _ = CreateRequest<string>("closeLobby", data, HTTPMethod.Post);
  }

  public static async Task<Response<string>> CheckServerStatus()
  {
    return await CreateRequest<string>("getLatestVer");
  }



  /* USER SPECIFIC METHODS */

  public static async Task<Response<bool>> ValidateToken(string token)
  {
    if (string.IsNullOrEmpty(token))
    {
      return new Response<bool>(200, "", false);
    }

    NameValueCollection data = new()
    {
      ["token"] = SaveSystem.saveData.token
    };

    return await CreateRequest<bool>("validateToken", data, HTTPMethod.Post);
  }

  public static async Task<Response<string>> Login(string token)
  {
    if (!string.IsNullOrEmpty(token))
    {
      return new Response<string>(400, "Already logged in", "");
    }

    NameValueCollection data = new()
    {
      ["token"] = SaveSystem.saveData.token
    };

    return await CreateRequest<string>("loginUserToken", data, HTTPMethod.Post);
  }

  public static async Task<Response<string>> Login(string email, string password)
  {
    if (!string.IsNullOrEmpty(SaveSystem.saveData.token))
    {
      return new Response<string>(400, "Already logged in", "");
    }

    NameValueCollection data = new()
    {
      ["email"] = email,
      ["password"] = password
    };

    return await CreateRequest<string>("loginUser", data, HTTPMethod.Post);
  }

  public static void SignOut()
  {
    SaveSystem.saveData.token = "";
    SaveSystem.SavePlayer(SaveSystem.saveData);
    SceneManager.LoadScene("Initialization");
  }

  public static async Task<Response<string>> CreateUser(string email, string password)
  {
    if (!string.IsNullOrEmpty(SaveSystem.saveData.token))
    {
      return new Response<string>(400, "Already logged in", "");
    }

    NameValueCollection data = new()
    {
      ["email"] = email,
      ["password"] = password
    };

    return await CreateRequest<string>("createUser", data, HTTPMethod.Post);
  }


  /* CLASSES, ENUMS, ETC */


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
    public string lobbySize;
    public bool started;

    public override string ToString()
    {
      return $"Code: {code}; Mode: {gameMode}; Players: {playerCount}; Lobby Size: {lobbySize}";
    }
  }

  public static bool Initialized
  {
    private set;
    get;
  }

  private enum HTTPMethod
  {
    Get,
    Post,
    Delete,
    Patch,
  }
}