using Mirror;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class LobbyUIManager : NetworkBehaviour
{
  public Button startButton, settingsButton;
  public TMP_Dropdown deathmatchTime, maps;
  public TMP_InputField rounds;
  public TMP_Text roomCode, roundsDefault;

  MyNetworkManager Room;

  void Awake()
  {
    Room = MyNetworkManager.instance;

    if (Room.isHost)
    {
      // startButton.interactable = true;
      Room.PlayerUpdate += PlayerUpdate;
      PlayerUpdate();
    }
    else
    {
      startButton.gameObject.SetActive(false);
      settingsButton.gameObject.SetActive(false);
    }


    roomCode.text = $"Room code: {Room.roomCode}";

    roundsDefault.text = $"Default: {MyNetworkManager.playableScenesCount}";

    startButton.onClick.AddListener(delegate { StartGame(); });

    List<string> mapNames = new();

    for (int i = MyNetworkManager.menuScenesCount; i < SceneManager.sceneCountInBuildSettings; i++)
    {
      mapNames.Add(System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i)).Replace("_", " "));
    }
    maps.AddOptions(mapNames);

    maps.onValueChanged.AddListener(delegate { SelectMap(maps.value); });
  }

  [Server]
  void StartGame()
  {
    Room.StartGame();
    FindObjectOfType<AudioSystem>().PlaySound("Select");
  }

  public void PlayerUpdate()
  {
#if !UNITY_EDITOR
    if (Room.players.Count < 2) startButton.interactable = false;
    else
#endif
    startButton.interactable = true;
  }

  private void OnDestroy()
  {
    Room.PlayerUpdate -= PlayerUpdate;
  }

  public void Quit()
  {
    Utilities.Disconnect();
    FindObjectOfType<AudioSystem>().PlaySound("Select");
  }

  MyNetworkManager nm = MyNetworkManager.instance;

  public void ChangeGameMode(int option)
  {
    nm.gameMode = (MyNetworkManager.GameMode)option;

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

    Firebase.UpdateLobby(NetworkServer.connections.Count, nm.gameMode.ToString(), nm.privacyType.ToString().ToLower());
  }

  public void TogglePrivate(bool option)
  {
    if (option) nm.privacyType = MyNetworkManager.PrivacyType.Private;
    else nm.privacyType = MyNetworkManager.PrivacyType.Public;

    Firebase.UpdateLobby(NetworkServer.connections.Count, nm.gameMode.ToString(), nm.privacyType.ToString().ToLower());
  }

  public void ChangeRoundCount(string count)
  {
    nm.rounds = int.Parse(count);
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

  public void SelectMap(int map)
  {
    nm.chosenMap = map;
  }
}