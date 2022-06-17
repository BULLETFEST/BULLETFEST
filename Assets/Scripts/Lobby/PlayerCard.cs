using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;

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

  [ServerCallback]
  public override void OnStartClient()
  {
    Room.players.Add(new PlayerData()
    {
      displayName = displayName,
      netId = connectionToClient.connectionId
    });
  }

  [Command]
  void UpdateDisplayName() => displayName = PlayerPrefs.GetString("PlayerName", "Guest");

  void HandleUpdateName(string oldName, string newName)
  {
    DisplayNameUI.text = newName;
  }

  [ClientRpc]
  void Rpc_UpdateDisplayName() => DisplayNameUI.text = displayName;
}
