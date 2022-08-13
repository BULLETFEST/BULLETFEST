using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;
using UnityEngine.UI;
using System.Linq;

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

    UpdateDisplayName(PlayerPrefs.GetString("PlayerName", "Guest"));
  }

  // [TargetRpc]
  // void SetName(NetworkConnection conn, GameObject card, string dName)
  // {
  //   card.GetComponent<PlayerCard>().DisplayNameUI.text = dName;
  // }

  [Command]
  void UpdateDisplayName(string dName) => displayName = dName;

  void HandleUpdateName(string oldName, string newName)
  {
    DisplayNameUI.text = newName;

    // print(connectionToClient);

    if (Room.players.ContainsKey(connectionToClient))
      Room.players.Remove(connectionToClient);
    Room.players.Add(connectionToClient, new PlayerData(newName));

    Room.PlayerUpdate?.Invoke();
  }

  // [ClientRpc]
  // void Rpc_UpdateDisplayName(string dName, GameObject card)
  // {
  //   card.GetComponent<PlayerCard>().DisplayNameUI.text = dName;
  // }

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

  // [Server]
  // public void ReceiveName(NetworkConnectionToClient conn, JoinNetworkMessage joinNetworkMessage)
  // {
  //   Rpc_UpdateDisplayName(joinNetworkMessage.name, joinNetworkMessage.card);

  //   if (Room.players.ContainsKey(connectionToClient))
  //     Room.players.Remove(connectionToClient);
  //   Room.players.Add(connectionToClient, new PlayerData(joinNetworkMessage.name));

  //   Room.PlayerUpdate?.Invoke();
  // }

  public struct JoinNetworkMessage : NetworkMessage
  {
    public string name;
    public GameObject card;
  }
}
