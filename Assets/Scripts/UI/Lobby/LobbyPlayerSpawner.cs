using UnityEngine;
using Mirror;
using System.Linq;
using UnityEngine.UI;

public class LobbyPlayerSpawner : NetworkBehaviour
{
  [SerializeField] private GameObject lobbyPlayer;

  Color[] colors = new Color[] {
    new Color(0.5882353f, 0.1137255f, 0.04313726f), // 961D0B
    new Color(0.0993236f, 0.4487756f, 0.6792453f), // 1972AD
    new Color(0.1027946f, 0.6226415f, 0.1877513f), // 1A9F30
    new Color(0.6235294f, 0.6018561f, 0.1019608f), // 9F991A
  };

  public override void OnStartServer()
  {
    base.OnStartServer();

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

    if (conn != NetworkServer.localConnection) player.GetComponent<LobbyPlayer>().kickBtn.gameObject.SetActive(true);

    NetworkServer.Spawn(player, conn);
    NetworkServer.AddPlayerForConnection(conn, player);
    NetworkServer.SetClientReady(conn);

    // CallPlayerJoined();
    OnPlayerJoin(player, NetworkServer.connections.Count - 1);

    NetworkConnectionToClient[] conns = NetworkServer.connections.Values.ToArray();

    for (int i = 0; i < conns.Length; i++)
    {
      SyncPreviousPlayers(conn, conns[i].identity.gameObject, i);
    }

  }

  [TargetRpc]
  public void SyncPreviousPlayers(NetworkConnection conn, GameObject objToSync, int idx)
  {
    GameObject lobbyPlayersContainer = GameObject.FindGameObjectWithTag("LobbyPlayerContainer");

    objToSync.transform.SetParent(lobbyPlayersContainer.transform);
    objToSync.transform.localScale = Vector3.one;
    objToSync.GetComponent<Image>().color = colors[idx];
    objToSync.transform.SetAsLastSibling();
  }

  [ClientRpc]
  public void OnPlayerJoin(GameObject playerObj, int idx)
  {
    GameObject lobbyPlayersContainer = GameObject.FindGameObjectWithTag("LobbyPlayerContainer");

    playerObj.transform.SetParent(lobbyPlayersContainer.transform);
    playerObj.transform.localScale = Vector3.one;
    playerObj.GetComponent<Image>().color = colors[idx];

    // GameObject[] lobbyPlayers = GameObject.FindGameObjectsWithTag("LobbyPlayer");
    // for (int i = 0; i < lobbyPlayers.Length; i++)
    // {
    //   lobbyPlayers[i].transform.SetParent(lobbyPlayersContainer.transform);
    //   lobbyPlayers[i].transform.localScale = Vector3.one;
    //   lobbyPlayers[i].GetComponent<Image>().color = colors[i];

    //   if (lobbyPlayers[i].GetComponent<NetworkIdentity>().netId == 0)
    //   {
    //     RectTransform rt = lobbyPlayers[i].GetComponent<LobbyPlayer>().DisplayNameUI.GetComponent<RectTransform>();
    //     rt.anchoredPosition = new Vector3(0, 90, rt.localPosition.z);

    //     lobbyPlayers[i].GetComponent<LobbyPlayer>().crown.gameObject.SetActive(true);
    //   }
    // }

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

  public void PlayerDisconnect(NetworkConnectionToClient conn) => CallPlayerDisconnect();

  [ClientRpc]
  public void CallPlayerDisconnect()
  {
    foreach (GameObject card in GameObject.FindGameObjectsWithTag("LobbyPlayer"))
    {
      card.GetComponent<LobbyPlayer>().OnPlayerDisconnect();
    }
  }

  // [ClientRpc]
  // void CallPlayerJoined()
  // {
  //   foreach (GameObject card in GameObject.FindGameObjectsWithTag("LobbyPlayer"))
  //   {
  //     card.GetComponent<LobbyPlayer>().OnPlayerJoin();
  //   }
  // }
}