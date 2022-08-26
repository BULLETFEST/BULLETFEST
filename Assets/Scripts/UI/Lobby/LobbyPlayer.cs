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

  public void OnPlayerJoin()
  {
    GameObject lobbyPlayersContainer = GameObject.FindGameObjectWithTag("LobbyPlayerContainer");
    GameObject[] lobbyPlayers = GameObject.FindGameObjectsWithTag("LobbyPlayer");
    for (int i = 0; i < lobbyPlayers.Length; i++)
    {
      lobbyPlayers[i].transform.SetParent(lobbyPlayersContainer.transform);
      lobbyPlayers[i].transform.localScale = Vector3.one;

      if (i == 0)
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
        Join = (isServer ? EpicTransport.EOSSDKComponent.LocalUserProductIdString : MyNetworkManager.instance.networkAddress),
      },
      Party = {
        Size = {
          MaxSize = 4,
          CurrentSize = NetworkServer.connections.Count,
        },
        Id = DiscordController.now.ToUnixTimeMilliseconds().ToString(),
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
