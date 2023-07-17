using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayerSpawner : NetworkBehaviour
{
  [SerializeField] private GameObject lobbyPlayer, chatPrefab;

  private Color[] colors = new Color[] {
    new Color(0.5882353f, 0.1137255f, 0.04313726f), // 961D0B
    new Color(0.0993236f, 0.4487756f, 0.6792453f), // 1972AD
    new Color(0.1027946f, 0.6226415f, 0.1877513f), // 1A9F30
    new Color(0.6235294f, 0.6018561f, 0.1019608f), // 9F991A
  };

  public override void OnStartServer()
  {
    base.OnStartServer();

    if (!FindObjectOfType<ChatManager>())
    {
      NetworkServer.Spawn(Instantiate(chatPrefab, Vector3.zero, Quaternion.identity));
    }

    for (int i = 0; i < NetworkServer.connections.Count; i++)
    {
      SpawnLobbyPlayer(NetworkServer.connections.ElementAt(i).Value);
    }

    MyNetworkManager.instance.PlayerConnect += SpawnLobbyPlayer;
    MyNetworkManager.instance.PlayerDisconnect += PlayerDisconnect;
  }

  private void OnDestroy()
  {
    MyNetworkManager.instance.PlayerConnect -= SpawnLobbyPlayer;
    MyNetworkManager.instance.PlayerDisconnect -= PlayerDisconnect;
  }

  public void SpawnLobbyPlayer(NetworkConnectionToClient conn)
  {
    GameObject player = Instantiate(lobbyPlayer, Vector3.zero, Quaternion.Euler(0, 0, 0));

    LobbyPlayer card = player.GetComponent<LobbyPlayer>();
    card.DisplayNameUI.text = "Loading...";

    if (conn != NetworkServer.localConnection)
    {
      player.GetComponent<LobbyPlayer>().kickBtn.gameObject.SetActive(true);
    }

    NetworkServer.Spawn(player, conn);
    NetworkServer.AddPlayerForConnection(conn, player);
    NetworkServer.SetClientReady(conn);

    // CallPlayerJoined();
    OnPlayerJoin(player, NetworkServer.connections.Count - 1);

    NetworkConnectionToClient[] conns = NetworkServer.connections.Values.ToArray();

    for (int i = 0; i < conns.Length; i++)
    {
      if (conns[i].identity != null)
      {
        SyncPreviousPlayers(conn, conns[i].identity.gameObject, i, i == 0);
      }
    }

  }

  [TargetRpc]
  public void SyncPreviousPlayers(NetworkConnection conn, GameObject objToSync, int idx, bool isHost)
  {
    GameObject lobbyPlayersContainer = GameObject.FindGameObjectWithTag("LobbyPlayerContainer");

    objToSync.transform.SetParent(lobbyPlayersContainer.transform);
    objToSync.transform.localScale = Vector3.one;
    objToSync.GetComponent<Image>().color = colors[idx % 4];
    objToSync.transform.SetAsLastSibling();
    objToSync.transform.position = Vector3.zero;

    if (isHost)
    {
      RectTransform rt = objToSync.GetComponent<LobbyPlayer>().DisplayNameUI.GetComponent<RectTransform>();
      rt.anchoredPosition = new Vector3(0, 90, rt.localPosition.z);

      objToSync.GetComponent<LobbyPlayer>().crown.gameObject.SetActive(true);
    }
  }

  [ClientRpc]
  public void OnPlayerJoin(GameObject playerObj, int idx)
  {
    GameObject lobbyPlayersContainer = GameObject.FindGameObjectWithTag("LobbyPlayerContainer");

    playerObj.transform.SetParent(lobbyPlayersContainer.transform);
    playerObj.transform.localScale = Vector3.one;
    playerObj.GetComponent<Image>().color = colors[idx % 4];

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

  public void PlayerDisconnect(NetworkConnectionToClient conn)
  {
    CallPlayerDisconnect();
  }

  [ClientRpc]
  public void CallPlayerDisconnect()
  {
    foreach (GameObject player in GameObject.FindGameObjectsWithTag("LobbyPlayer"))
    {
      player.GetComponent<LobbyPlayer>().OnPlayerDisconnect();
    }
  }
}