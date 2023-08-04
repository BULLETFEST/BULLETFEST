using System.Collections.Generic;
using System.Linq;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUIManager : NetworkBehaviour
{
  public Button startButton, settingsButton;
  public TMP_Dropdown deathmatchTime, maps, gameModeDrowpdown;
  public TMP_InputField rounds;
  public TMP_Text roomCode, roundsDefault;
  public Toggle privacy, enableBots;
  private MyNetworkManager nm = MyNetworkManager.Instance;
  private GameSettings settings => GameManager.settings;

  public override void OnStartClient()
  {
    base.OnStartClient();

    roomCode.text = $"Room code: {nm.roomCode}";
  }

  private void Start()
  {
    if (!nm.isHost)
    {
      startButton.gameObject.SetActive(false);
      settingsButton.gameObject.SetActive(false);
    }


    gameModeDrowpdown.value = (int)settings.gameMode;
    rounds.text = settings.rounds.ToString();
    deathmatchTime.value = DeathmatchTimeToOption(settings.deathmatchLength);
    maps.value = settings.chosenMap;
    privacy.isOn = settings.privacyType == GameSettings.PrivacyType.Private;
    enableBots.isOn = settings.enableBots;


    roomCode.text = $"Room code: {nm.roomCode}";

    roundsDefault.text = $"Default: {MyNetworkManager.playableScenesCount}";

    startButton.onClick.AddListener(delegate { StartGame(); });

    // List<string> mapNames = new();

    // for (int i = MyNetworkManager.menuScenesCount; i < SceneManager.sceneCountInBuildSettings; i++)
    // {
    //   mapNames.Add(System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i)).Replace("_", " "));
    // }
    // maps.AddOptions(nm._4Players.ToList());

    UpdateMapsList();

    maps.onValueChanged.AddListener(delegate { SelectMap(maps.value); });

    SetupModifiers();
  }

  private void Update()
  {
    if (nm.isHost)
    {
#if !UNITY_EDITOR
    if (GameManager.Instance.players.Count < 2 && !settings.enableBots) startButton.interactable = false;
    else
#endif
      startButton.interactable = true;
    }
  }

  [Server]
  private void StartGame()
  {
    GameManager.Instance.StartGame();
    FindObjectOfType<AudioSystem>().PlaySound("Select");
  }

  public void Quit()
  {
    nm.Disconnect();
    FindObjectOfType<AudioSystem>().PlaySound("Select");
  }

  public void ChangeGameMode(int option)
  {
    settings.gameMode = (GameSettings.GameMode)option;

    if (option == 1)
    {
      rounds.transform.parent.gameObject.SetActive(false);
      deathmatchTime.transform.parent.gameObject.SetActive(true);
      maps.transform.parent.gameObject.SetActive(true);
    }
    else
    {
      rounds.transform.parent.gameObject.SetActive(true);
      deathmatchTime.transform.parent.gameObject.SetActive(false);
      maps.transform.parent.gameObject.SetActive(false);
    }

    FirebaseManager.UpdateLobby(NetworkServer.connections.Count);

    nm.Chat.messages.Add($"W|Gamemode changed to {settings.gameMode}");
  }

  public void TogglePrivate(bool option)
  {
    settings.privacyType = option ? GameSettings.PrivacyType.Private : GameSettings.PrivacyType.Public;

    FirebaseManager.UpdateLobby(NetworkServer.connections.Count);

    nm.Chat.messages.Add($"W|Lobby visibility changed to {settings.privacyType}");
  }

  public void ToggleBots(bool option)
  {
    settings.enableBots = option;

    UpdateMapsList();

    nm.Chat.messages.Add($"W|Bots changed to {(option ? "Enabled" : "Disabled")}");
  }

  public void ChangeRoundCount(string count)
  {
    settings.rounds = int.Parse(count);
    nm.Chat.messages.Add($"W|Rounds changed to {count}");
  }

  public void ChangeDeathmatchTime(int option)
  {
    switch (option)
    {
      case 0:
      default:
        settings.deathmatchLength = 1;
        break;
      case 1:
        settings.deathmatchLength = 1.5f;
        break;
      case 2:
        settings.deathmatchLength = 2;
        break;
      case 3:
        settings.deathmatchLength = 2.5f;
        break;
      case 4:
        settings.deathmatchLength = 3;
        break;
      case 5:
        settings.deathmatchLength = 3.5f;
        break;
      case 6:
        settings.deathmatchLength = 4;
        break;
      case 7:
        settings.deathmatchLength = 4.5f;
        break;
      case 8:
        settings.deathmatchLength = 5;
        break;
    }

    nm.Chat.messages.Add($"W|Deathmatch time changed to {settings.deathmatchLength} minutes");
  }

  public int DeathmatchTimeToOption(float t)
  {
    switch (t)
    {
      case 1:
      default:
        return 0;
      case 1.5f:
        return 1;
      case 2:
        return 2;
      case 2.5f:
        return 3;
      case 3:
        return 4;
      case 3.5f:
        return 5;
      case 4:
        return 6;
      case 4.5f:
        return 7;
      case 5:
        return 8;
    }
  }

  public void SelectMap(int map)
  {
    settings.chosenMap = map;
  }

  public void ChangeLobbySize(int option)
  {
    switch (option)
    {
      case 0:
      default:
        settings.lobbySize = 4;
        break;
      case 1:
        settings.lobbySize = 6;
        break;
    }

    UpdateMapsList();
    FirebaseManager.UpdateLobby(NetworkServer.connections.Count);
    nm.Chat.messages.Add($"W|Lobby size changed to {settings.lobbySize}");
  }

  public void UpdateMapsList()
  {
    List<string> n;

    List<string> other;

    switch (settings.lobbySize)
    {
      case 4:
      default:
        other = nm._4Players.ToList();
        break;
      case 6:
        other = nm._6Players.ToList();
        break;
    }

    n = settings.enableBots ? nm._BotSupport.ToList().Where(x => other.Contains(x)).ToList() : other;

    n = n.Select(map => { return map = System.IO.Path.GetFileNameWithoutExtension(map).Replace("_", " "); }).ToList();

    n.Insert(0, "Random");

    maps.ClearOptions();

    maps.AddOptions(n);
  }

  /*
  *
  * Additional Modifiers
  *
  */

  [Header("Additional Modifiers")]
  public Toggle goldenGun;

  private void SetupModifiers()
  {
    goldenGun.isOn = settings.goldenGun;
  }

  public void ToggleGoldenGun(bool option)
  {
    settings.goldenGun = option;
    nm.Chat.messages.Add($"W|Golden gun changed to {(option ? "Enabled" : "Disabled")}");

  }
}