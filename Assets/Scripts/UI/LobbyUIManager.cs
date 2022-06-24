using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using System;

public class LobbyUIManager : NetworkBehaviour
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

  public void Start()
  {
    // base.OnStartLocalPlayer();

    if (Room.isHost)
    {
      // startButton.interactable = true;
      Room.PlayerUpdate += PlayerUpdate;
      PlayerUpdate();
    }

    startButton.onClick.AddListener(delegate { StartGame(); });

  }

  [Server]
  void StartGame()
  {
    Room.StartGame();
  }

  public void PlayerUpdate()
  {
    // print("a");
    // if (Room.players.Count < 2) startButton.interactable = false;
    // else 
    startButton.interactable = true;
  }

  private void OnDestroy()
  {
    Room.PlayerUpdate -= PlayerUpdate;
  }


}
