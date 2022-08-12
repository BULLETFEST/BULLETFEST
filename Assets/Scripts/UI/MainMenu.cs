using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Net;
using System.Net.Sockets;
using TMPro;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using EpicTransport;

// using ParrelSync;
public class MainMenu : MonoBehaviour
{
  MyNetworkManager nm;

  [HideInInspector]
  public string code = "0000";

  [HideInInspector]
  public string playerName;

  private string localIp;

  private EOSSDKComponent eos;

  [Header("UI Elements")]
  public Button connectBtn;
  public Button hostBtn;
  public TMP_Text roundsDefault;

  [Header("Host UI Elements")]
  public TMP_InputField port;
  public TMP_InputField rounds;

  void Start()
  {
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
    playerName = PlayerPrefs.GetString("PlayerName", "Guest");

    Application.targetFrameRate = Screen.currentResolution.refreshRate;

    nm.networkAddress = EpicTransport.EOSSDKComponent.LocalUserProductIdString;//localIp;

    roundsDefault.text = $"Default: {SceneManager.sceneCountInBuildSettings - 3}";
  }

  public async void Connect()
  {
    connectBtn.interactable = false;
    PlayerPrefs.SetString("PlayerName", playerName);

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
    }
    connectBtn.interactable = true;
  }

  public Regex numbers = new Regex(@"\D");

  public async void Host()
  {
    hostBtn.interactable = false;
    PlayerPrefs.SetString("PlayerName", playerName);


    bool toReturn = false;

    if (numbers.IsMatch(rounds.text))
    {
      rounds.text = "";
      toReturn = true;
    }

    if (numbers.IsMatch(port.text))
    {
      port.text = "";
      toReturn = true;
    }

    if (toReturn) return;

    nm.GetComponent<kcp2k.KcpTransport>().Port = ushort.Parse(port.text != "" ? port.text : "7777");

    // In the context of hosting, code is equal to the
    // room code generated on the server.
    Firebase.Response res = await Firebase.HostGame();

    if (res.success)
    {
      nm.RoomCode = res.code;
      nm.StartHost();
    }
    else
    {
      if (res.message != "sErr")
      {
        Message.DisplayMessage("Something went wrong!", res.message, HorizontalAlignmentOptions.Center);
      }
    }
    hostBtn.interactable = true;
  }


  public void ChangeName(string newPlayerName)
  {
    playerName = newPlayerName;
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
    }
    else
    {
      rounds.transform.parent.gameObject.SetActive(true);
    }
  }

  public void ChangeSceneToControls()
  {
    SceneManager.LoadScene("Controls");
  }
}