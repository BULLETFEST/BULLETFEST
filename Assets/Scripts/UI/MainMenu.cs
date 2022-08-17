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

    roundsDefault.text = $"Default: {SceneManager.sceneCountInBuildSettings - MyNetworkManager.menuScenes}";
  }

  public async void Connect()
  {
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

  public void ChangeRoomCode(string newCode)
  {
    code = newCode;
  }

  public void ChangeGameMode(int option)
  {
    nm.gameMode = (MyNetworkManager.GameMode)option;

    if (option == 1)
    {
      rounds.transform.parent.gameObject.SetActive(false);
      deathmatchTime.transform.parent.gameObject.SetActive(true);
    }
    else
    {
      rounds.transform.parent.gameObject.SetActive(true);
      deathmatchTime.transform.parent.gameObject.SetActive(false);
    }
  }

  public void ChangeDeathmatchTime(int option)
  {
    switch (option)
    {
      case 0:
      default:
        nm.deathmatchLength = 1;
        break;
      case 1:
        nm.deathmatchLength = 1.5f;
        break;
      case 2:
        nm.deathmatchLength = 2;
        break;
      case 3:
        nm.deathmatchLength = 2.5f;
        break;
      case 4:
        nm.deathmatchLength = 3;
        break;
      case 5:
        nm.deathmatchLength = 3.5f;
        break;
      case 6:
        nm.deathmatchLength = 4;
        break;
      case 7:
        nm.deathmatchLength = 4.5f;
        break;
      case 8:
        nm.deathmatchLength = 5;
        break;
    }
  }

  public void ChangeSceneToControls()
  {
    SceneManager.LoadScene("Controls");
  }
}