using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class LobbyUIManager : MonoBehaviour
{
  private MyNetworkManager room;
  private MyNetworkManager Room
  {
    get
    {
      if (room != null) { return room; }
      return room = NetworkManager.singleton as MyNetworkManager;
    }
  }

  public Button startButton;

  void Start()
  {
    if (!Room.isHost) startButton.interactable = false;

    startButton.onClick.AddListener(delegate { StartGame(); });
  }

  [Server]
  void StartGame()
  {
    Room.ServerChangeScene("Game");

  }


}
