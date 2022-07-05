using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;
using UnityEngine.UI;

public class PlayerCard : NetworkBehaviour
{
  public TextMeshProUGUI DisplayNameUI;

  [SyncVar(hook = nameof(HandleUpdateName))]
  public string displayName;

  public Button kickBtn;

  private MyNetworkManager room;
  private MyNetworkManager Room
  {
    get
    {
      if (room != null) { return room; }
      return room = NetworkManager.singleton as MyNetworkManager;
    }
  }

  public override void OnStartAuthority()
  {
    base.OnStartAuthority();


    GameObject playerCards = GameObject.FindGameObjectWithTag("PlayerCards");
    GameObject[] _playerCards = GameObject.FindGameObjectsWithTag("PlayerCard");
    foreach (GameObject _playerCard in _playerCards)
      _playerCard.transform.parent = playerCards.transform;

    UpdateDisplayName();
    // if (!room.isHost) OnPointerEnter();
    // NetworkServer.SetClientReady(connectionToClient);
  }

  // [ServerCallback]
  // public override void OnStartClient()
  // {

  // }

  [Command]
  void UpdateDisplayName() => displayName = PlayerPrefs.GetString("PlayerName", "Guest");

  void HandleUpdateName(string oldName, string newName)
  {
    DisplayNameUI.text = newName;

    // print(connectionToClient);

    if (Room.players.ContainsKey(connectionToClient))
      Room.players.Remove(connectionToClient);
    Room.players.Add(connectionToClient, new PlayerData(newName));

    Room.PlayerUpdate?.Invoke();
  }

  [ClientRpc]
  void Rpc_UpdateDisplayName() => DisplayNameUI.text = displayName;

  // [Command]
  // public void OnPointerEnter()
  // {
  //   kickBtn.gameObject.SetActive(true);
  // }

  [Server]
  public void KickPlayer()
  {
    connectionToClient.Send(new Message.ServerMessge
    {
      titleText = "Disconnected",
      contentText = "You've been kicked out of the game",
      _alignment = 2,
      disconnect = true
    });
  }
}
