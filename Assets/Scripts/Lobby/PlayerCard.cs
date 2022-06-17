using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;
using System.Linq;

public class PlayerCard : NetworkBehaviour
{
  public TextMeshProUGUI DisplayNameUI;

  [SyncVar(hook = nameof(HandleUpdateName))]
  public string displayName;

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
    print(_playerCards.Length);
    foreach (GameObject _playerCard in _playerCards)
      _playerCard.transform.parent = playerCards.transform;

    UpdateDisplayName();
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

    if (Room.players.ContainsKey(connectionToClient))
      Room.players.Remove(connectionToClient);
    Room.players.Add(connectionToClient, newName);
  }

  [ClientRpc]
  void Rpc_UpdateDisplayName() => DisplayNameUI.text = displayName;
}
