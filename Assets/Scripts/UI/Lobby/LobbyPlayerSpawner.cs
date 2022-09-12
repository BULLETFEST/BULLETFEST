using UnityEngine;
using Mirror;
using System.Linq;

public class LobbyPlayerSpawner : NetworkBehaviour
{
  [SerializeField] private GameObject lobbyPlayer;

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

    CallPlayerJoined();
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

  [ClientRpc]
  void CallPlayerJoined()
  {
    foreach (GameObject card in GameObject.FindGameObjectsWithTag("LobbyPlayer"))
    {
      card.GetComponent<LobbyPlayer>().OnPlayerJoin();
    }
  }
}
