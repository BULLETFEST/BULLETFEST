using UnityEngine;
using TMPro;
using Mirror;
using UnityEngine.UI;

public class PlayerCard : NetworkBehaviour
{
  public TMP_Text DisplayNameUI;

  [SyncVar(hook = nameof(HandleUpdateName))]
  public string displayName;

  public Button kickBtn;

  public override void OnStartAuthority()
  {
    base.OnStartAuthority();
    OnPlayerJoin();

    UpdateDisplayName(PlayerPrefs.GetString("PlayerName", "Guest"));
  }

  public void OnPlayerJoin()
  {
    GameObject playerCards = GameObject.FindGameObjectWithTag("PlayerCards");
    GameObject[] _playerCards = GameObject.FindGameObjectsWithTag("PlayerCard");
    foreach (GameObject _playerCard in _playerCards)
      _playerCard.transform.SetParent(playerCards.transform);

    GameObject.FindGameObjectWithTag("HostCard").transform.SetParent(playerCards.transform.parent, false);
    GameObject.FindGameObjectWithTag("HostCard").GetComponent<RectTransform>().anchoredPosition = Vector2.zero;


  }

  [Command]
  void UpdateDisplayName(string dName)
  {
    if (dName.Length > 16) dName = dName.Substring(0, 16);


    if (MyNetworkManager.instance.players.ContainsKey(connectionToClient))
      MyNetworkManager.instance.players.Remove(connectionToClient);
    MyNetworkManager.instance.players.Add(connectionToClient, new PlayerData(dName));

    MyNetworkManager.instance.PlayerUpdate?.Invoke();

    displayName = dName;
  }


  void HandleUpdateName(string oldName, string newName)
  {
    DisplayNameUI.text = newName;
  }

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
