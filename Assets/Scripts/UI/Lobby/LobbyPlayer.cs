using UnityEngine;
using TMPro;
using Mirror;
using UnityEngine.UI;

public class LobbyPlayer : NetworkBehaviour
{
  public TMP_Text DisplayNameUI;

  [SyncVar(hook = nameof(HandleUpdateName))]
  public string displayName;

  public Button kickBtn;

  public Image crown;

  public override void OnStartAuthority()
  {
    base.OnStartAuthority();
    // OnPlayerJoin();

    UpdateDisplayName(PlayerPrefs.GetString("PlayerName", "Guest"));
    Message.HideMessage();
  }

  public void OnPlayerDisconnect()
  {
    DiscordController.UpdateActivity(new Discord.Activity
    {
      State = "In a lobby",
      Secrets = {
        Join = (isServer ? EpicTransport.EOSSDKComponent.LocalUserProductIdString : MyNetworkManager.instance.networkAddress) + "|||" + MyNetworkManager.instance.roomCode + "|||" + DiscordController.partyId,
      },
      Party = {
        Size = {
          MaxSize = 4,
          CurrentSize = GameObject.FindGameObjectsWithTag("LobbyPlayer").Length,
        },
        Id = DiscordController.partyId//DiscordController.now.ToUnixTimeMilliseconds().ToString(),
      }
    });
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