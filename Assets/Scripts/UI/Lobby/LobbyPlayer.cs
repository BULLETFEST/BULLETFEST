using Mirror;
using TMPro;
using UnityEngine;
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
        Join = (isServer ? EpicTransport.EOSSDKComponent.LocalUserProductIdString : MyNetworkManager.Instance.networkAddress) + "|||" + MyNetworkManager.Instance.roomCode + "|||" + DiscordController.partyId,
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
  private void UpdateDisplayName(string dName)
  {
    if (dName.Length > 16)
    {
      dName = dName[..16];
    }

    if (string.IsNullOrEmpty(dName)) dName = "Guest";

    if (GameManager.Instance.players.ContainsKey(connectionToClient.connectionId))
    {
      GameManager.Instance.players.Remove(connectionToClient.connectionId);
    }

    GameManager.Instance.players.Add(connectionToClient.connectionId, new PlayerData(dName, connectionToClient.connectionId));

    MyNetworkManager.Instance.PlayerUpdate?.Invoke();

    displayName = dName;
  }

  private void HandleUpdateName(string oldName, string newName)
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