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

  public override void OnStartAuthority()
  {
    base.OnStartAuthority();

    GameObject playerCards = GameObject.FindGameObjectWithTag("PlayerCards");
    GameObject[] _playerCards = GameObject.FindGameObjectsWithTag("PlayerCard");
    foreach (GameObject _playerCard in _playerCards)
      _playerCard.transform.parent = playerCards.transform;

    UpdateDisplayName(PlayerPrefs.GetString("PlayerName", "Guest"));
  }

  [Command]
  void UpdateDisplayName(string dName) => displayName = dName;

  void HandleUpdateName(string oldName, string newName)
  {
    if (newName.Length > 16) newName = newName.Substring(0, 16);
    DisplayNameUI.text = newName;

    if (MyNetworkManager.instance.players.ContainsKey(connectionToClient))
      MyNetworkManager.instance.players.Remove(connectionToClient);
    MyNetworkManager.instance.players.Add(connectionToClient, new PlayerData(newName));

    MyNetworkManager.instance.PlayerUpdate?.Invoke();
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
