using Mirror;
using UnityEngine.UI;
using TMPro;

public class LobbyUIManager : NetworkBehaviour
{
  public Button startButton;
  public TMP_Text roomCode;

  MyNetworkManager Room;

  void Start()
  {
    Room = MyNetworkManager.instance;

    if (Room.isHost)
    {
      // startButton.interactable = true;
      Room.PlayerUpdate += PlayerUpdate;
      PlayerUpdate();
    }

    roomCode.text = $"Room code: {Room.RoomCode}";

    startButton.onClick.AddListener(delegate { StartGame(); });

  }

  [Server]
  void StartGame()
  {
    Room.StartGame();
  }

  public void PlayerUpdate()
  {
#if !UNITY_EDITOR
    print("a");
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
    Room.Disconnect();
  }
}