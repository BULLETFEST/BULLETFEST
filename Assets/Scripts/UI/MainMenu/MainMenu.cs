using System.Collections;
using System.Text.RegularExpressions;
using EpicTransport;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// using ParrelSync;
public class MainMenu : MonoBehaviour
{
  private MyNetworkManager nm;

  [HideInInspector]
  public string code = "0000";

  [Header("UI Elements")]
  [SerializeField] private Button connectBtn;
  [SerializeField] private Button joinBtn;
  [SerializeField] private Button hostBtn;
  [SerializeField] private Button refreshBtn;
  [SerializeField] private Button serverBrowserBtn;
  [SerializeField] private TMP_Text buildNumber;
  [SerializeField] private TMP_InputField playerName;
  [SerializeField] private GameObject serverBrowser, serversContainer;

  [SerializeField] private RectTransform[] beans;

  public GameObject gameCard;
  private bool isConnecting = false;
  private bool isHosting = false;

  private void Start()
  {
    buildNumber.text = "Build " + Application.version;

    nm = FindObjectOfType<MyNetworkManager>();
    playerName.text = PlayerPrefs.GetString("PlayerName", "");

    Application.targetFrameRate = (int)Screen.currentResolution.refreshRateRatio.value;

    nm.networkAddress = EOSSDKComponent.LocalUserProductIdString;//localIp;

    EOSSDKComponent.Initialize();

    for (int i = 0; i < beans.Length; i++)
    {
      StartCoroutine(RotateBean(beans[i]));
    }

  }

  private IEnumerator RotateBean(RectTransform bean)
  {
    yield return new WaitForSecondsRealtime(Random.Range(1.0f, 5.0f));
    bean.rotation = Quaternion.Euler(0, bean.rotation.y == 0 ? 180 : 0, 0);
    StartCoroutine(RotateBean(bean));
  }

  private void Update()
  {
    joinBtn.interactable = EOSSDKComponent.Initialized && !isConnecting;
    hostBtn.interactable = EOSSDKComponent.Initialized && !isHosting;
    serverBrowserBtn.interactable = EOSSDKComponent.Initialized;

    if (Input.GetKeyDown(KeyCode.Slash))
    {
      FirebaseManager.SignOut();
    }
  }

  public async void Connect()
  {
    if (isConnecting)
    {
      return;
    }

    Message.DisplayMessage("", "Connecting...", false, HorizontalAlignmentOptions.Center);

    isConnecting = true;
    connectBtn.interactable = false;
    PlayerPrefs.SetString("PlayerName", playerName.text);

    // In the context of joining, code is equal to the
    // host's IP.
    FirebaseManager.Response<string> res = await FirebaseManager.JoinGame(code);

    if (res.status == 200)
    {
      nm.roomCode = code;
      nm.networkAddress = res.data;

      if (string.IsNullOrWhiteSpace(nm.networkAddress))
      {
        Message.DisplayMessage("Failed not connect to server!", "Invalid Server Address.", true, HorizontalAlignmentOptions.Center);
        return;
      }
      nm.StartClient();
    }
    else
    {
      Message.HideMessage();

      if (res.message != "sErr")
      {
        Message.DisplayMessage("Something went wrong!", res.message, HorizontalAlignmentOptions.Center);
      }

      connectBtn.interactable = true;
      isConnecting = false;
    }
  }

  public Regex nonNumbers = new(@"\D");

  public async void Host()
  {
    if (isHosting)
    {
      return;
    }

    Message.DisplayMessage("", "Connecting...", false, HorizontalAlignmentOptions.Center);

    isHosting = true;
    PlayerPrefs.SetString("PlayerName", playerName.text);

    // In the context of hosting, code is equal to the
    // room code generated on the server.
    FirebaseManager.Response<string> res = await FirebaseManager.HostGame();

    if (res.status == 200)
    {
      nm.roomCode = res.data;
      // nm.rounds = int.Parse(rounds.text == "" ? "11" : rounds.text);
      DiscordController.partyId = DiscordController.now.ToUnixTimeMilliseconds().ToString();
      nm.StartHost();
    }
    else
    {
      Message.HideMessage();

      if (res.message != "sErr")
      {
        Message.DisplayMessage("Something went wrong!", res.message, HorizontalAlignmentOptions.Center);
      }

      isHosting = false;
    }
  }

  public void OpenSettings()
  {
    FindObjectOfType<SettingsUI>().GetComponent<Canvas>().enabled = true;
    SettingsUI.IsSettingsOpen = true;
  }

  public void OpenCredits()
  {
    SceneManager.LoadScene("Credits");
  }

  public void ChangeRoomCode(string newCode)
  {
    code = newCode;
  }

  public async void UpdateServerBrowser()
  {
    refreshBtn.interactable = false;
    refreshBtn.GetComponentInChildren<TMP_Text>().text = "Refreshing...";
    refreshBtn.GetComponentInChildren<TMP_Text>().fontStyle = FontStyles.Italic;
    serverBrowser.SetActive(true);


    FirebaseManager.Match[] matches = await FirebaseManager.GetLobbies();

    foreach (Transform child in serversContainer.transform)
    {
      Destroy(child.gameObject);
    }

    foreach (FirebaseManager.Match match in matches)
    {
      GameObject card = Instantiate(gameCard, Vector3.zero, Quaternion.Euler(0, 0, 0), serversContainer.transform);

      GameCard gameCardUi = card.GetComponent<GameCard>();

      gameCardUi.playerCount.text = match.playerCount + "/" + match.lobbySize;
      gameCardUi.code.text = match.code;
      gameCardUi.gameMode.text = match.gameMode;

      card.GetComponent<Button>().onClick.AddListener(delegate
      {
        code = match.code;
        Connect();
        PlaySelectSound();
      });
    }

    refreshBtn.GetComponentInChildren<TMP_Text>().text = "Refresh";
    refreshBtn.GetComponentInChildren<TMP_Text>().fontStyle = FontStyles.Normal;
    refreshBtn.interactable = true;
  }

  public void PlaySelectSound()
  {
    AudioSystem.Instance.PlaySound("Select");
  }
}