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
      SpawnCard(NetworkServer.connections.ElementAt(i).Value);
    }

    MyNetworkManager.instance.PlayerConnect += SpawnCard;
  }

  private void OnDestroy()
  {
    MyNetworkManager.instance.PlayerConnect -= SpawnCard;
  }

  public void SpawnCard(NetworkConnectionToClient conn)
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

  [ClientRpc]
  void CallPlayerJoined()
  {
    foreach (GameObject card in GameObject.FindGameObjectsWithTag("LobbyPlayer"))
    {
      card.GetComponent<LobbyPlayer>().OnPlayerJoin();
    }
  }
}
