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
  public Button joinBtn;
  public Button hostBtn;
  public TMP_Text buildNumber;
  public TMP_InputField playerName;

  bool isConnecting = false;

  void Start()
  {
    EOSSDKComponent.Initialize();

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

  private void Update()
  {
    joinBtn.interactable = EOSSDKComponent.Initialized;
    hostBtn.interactable = EOSSDKComponent.Initialized;

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

    // In the context of hosting, code is equal to the
    // room code generated on the server.
    Firebase.Response res = await Firebase.HostGame();

    if (res.success)
    {
      nm.RoomCode = res.code;
      // nm.rounds = int.Parse(rounds.text == "" ? "11" : rounds.text);

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
    // SceneManager.LoadScene("Settings", LoadSceneMode.Additive);
    SaveSystem.settingsUI.GetComponent<Canvas>().enabled = true;
    SaveSystem.IsSettingsOpen = true;
  }

  public void ChangeRoomCode(string newCode)
  {
    code = newCode;
  }
}