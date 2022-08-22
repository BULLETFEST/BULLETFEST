using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using EpicTransport;

// using ParrelSync;
public class MainMenu : MonoBehaviour
{
  MyNetworkManager nm;

  [HideInInspector]
  public string code = "0000";

  // [HideInInspector]
  // public string playerName;

  private string localIp;

  private EOSSDKComponent eos;

  [Header("UI Elements")]
  public Button connectBtn;
  public Button hostBtn;
  public TMP_Text roundsDefault;
  public TMP_Text buildNumber;

  [Header("Host UI Elements")]
  // public TMP_InputField port;
  public TMP_InputField rounds;
  public TMP_InputField playerName;
  public TMP_Dropdown deathmatchTime;

  bool isConnecting = false;

  void Start()
  {
    buildNumber.text = "Build " + Application.version;

    IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
    foreach (IPAddress ip in host.AddressList)
    {
      if (ip.AddressFamily == AddressFamily.InterNetwork)
      {
        localIp = ip.ToString();
      }
    }

    eos = GetComponent<EOSSDKComponent>();
    nm = FindObjectOfType<MyNetworkManager>();
    playerName.text = PlayerPrefs.GetString("PlayerName", "");

    Application.targetFrameRate = Screen.currentResolution.refreshRate;

    nm.networkAddress = EpicTransport.EOSSDKComponent.LocalUserProductIdString;//localIp;
  }

  public async void Connect()
  {
    if (isConnecting) return;

    isConnecting = true;
    connectBtn.interactable = false;
    PlayerPrefs.SetString("PlayerName", playerName.text);

    // In the context of joining, code is equal to the
    // host's IP.
    Firebase.Response res = await Firebase.JoinGame(code);

    if (res.success)
    {
      nm.RoomCode = code;
      nm.networkAddress = res.code;
      nm.StartClient();
    }
    else
    {
      if (res.message != "sErr")
      {
        Message.DisplayMessage("Something went wrong!", res.message, HorizontalAlignmentOptions.Center);
      }
      connectBtn.interactable = true;
      isConnecting = false;
    }
  }

  public Regex nonNumbers = new Regex(@"\D");

  public async void Host()
  {
    hostBtn.interactable = false;
    PlayerPrefs.SetString("PlayerName", playerName.text);


    bool toReturn = false;

    if (nonNumbers.IsMatch(rounds.text))
    {
      rounds.text = "";
      toReturn = true;
    }

    if (toReturn) return;

    // In the context of hosting, code is equal to the
    // room code generated on the server.
    Firebase.Response res = await Firebase.HostGame();

    if (res.success)
    {
      nm.RoomCode = res.code;
      // nm.rounds = int.Parse(rounds.text == "" ? "11" : rounds.text);
      if (int.TryParse(rounds.text, out int rnds))
      {
        nm.rounds = rnds;
      }
      else nm.rounds = 11;
      nm.StartHost();
    }
    else
    {
      if (res.message != "sErr")
      {
        Message.DisplayMessage("Something went wrong!", res.message, HorizontalAlignmentOptions.Center);
      }
      hostBtn.interactable = true;
    }
  }

  public void ChangeSceneToSettings()
  {
    SceneManager.LoadScene("Settings", LoadSceneMode.Additive);
    SaveSystem.IsSettingsOpen = true;
  }

  public void ChangeRoomCode(string newCode)
  {
    code = newCode;
  }

  public void PrintOnSubmit()
  {
    print("A");
  }
}