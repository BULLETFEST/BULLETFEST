using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayerSpawner : NetworkBehaviour
{
  [SerializeField] private GameObject lobbyPlayer, chatPrefab;

  private Color[] colors => Globals.colors;

  public override void OnStartServer()
  {
    base.OnStartServer();
    // SceneManager.LoadScene(4, LoadSceneMode.Additive);

    // if (!FindObjectOfType<ChatManager>())
    // {
    //   NetworkServer.Spawn(Instantiate(chatPrefab, Vector3.zero, Quaternion.identity));
    // }

    for (int i = 0; i < NetworkServer.connections.Count; i++)
    {
      SpawnLobbyPlayer(NetworkServer.connections.ElementAt(i).Value);
    }

    MyNetworkManager.Instance.PlayerConnect += SpawnLobbyPlayer;
    MyNetworkManager.Instance.PlayerDisconnect += PlayerDisconnect;
  }

  private void OnDestroy()
  {
    MyNetworkManager.Instance.PlayerConnect -= SpawnLobbyPlayer;
    MyNetworkManager.Instance.PlayerDisconnect -= PlayerDisconnect;
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