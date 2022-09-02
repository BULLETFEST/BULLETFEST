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
    OnPlayerJoin();

    UpdateDisplayName(PlayerPrefs.GetString("PlayerName", "Guest"));
  }

  Color[] colors = new Color[] {
    new Color(0.5882353f, 0.1137255f, 0.04313726f), // 961D0B
    new Color(0.0993236f, 0.4487756f, 0.6792453f), // 1972AD
    new Color(0.1027946f, 0.6226415f, 0.1877513f), // 1A9F30
    new Color(0.6235294f, 0.6018561f, 0.1019608f), // 9F991A
  };

  public void OnPlayerJoin()
  {
    GameObject lobbyPlayersContainer = GameObject.FindGameObjectWithTag("LobbyPlayerContainer");
    GameObject[] lobbyPlayers = GameObject.FindGameObjectsWithTag("LobbyPlayer");
    for (int i = 0; i < lobbyPlayers.Length; i++)
    {
      lobbyPlayers[i].transform.SetParent(lobbyPlayersContainer.transform);
      lobbyPlayers[i].transform.localScale = Vector3.one;
      lobbyPlayers[i].GetComponent<Image>().color = colors[i];

      if (lobbyPlayers[i].GetComponent<NetworkIdentity>().netId == 0)
      {
        RectTransform rt = lobbyPlayers[i].GetComponent<LobbyPlayer>().DisplayNameUI.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector3(0, 90, rt.localPosition.z);

        lobbyPlayers[i].GetComponent<LobbyPlayer>().crown.gameObject.SetActive(true);
      }
    }

    DiscordController.UpdateActivity(new Discord.Activity
    {
      State = "In a lobby",
      Secrets = {
        Join = (isServer ? EpicTransport.EOSSDKComponent.LocalUserProductIdString : MyNetworkManager.instance.networkAddress) + "|||" + MyNetworkManager.instance.RoomCode + "|||" + DiscordController.partyId,
      },
      Party = {
        Size = {
          MaxSize = 4,
          CurrentSize = lobbyPlayers.Length,
        },
        Id = DiscordController.partyId//DiscordController.now.ToUnixTimeMilliseconds().ToString(),
      }
    });
  }

  public void OnPlayerDisconnect()
  {
    DiscordController.UpdateActivity(new Discord.Activity
    {
      State = "In a lobby",
      Secrets = {
        Join = (isServer ? EpicTransport.EOSSDKComponent.LocalUserProductIdString : MyNetworkManager.instance.networkAddress) + "|||" + MyNetworkManager.instance.RoomCode + "|||" + DiscordController.partyId,
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
