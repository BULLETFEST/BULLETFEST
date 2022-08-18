using UnityEngine;
using Mirror;
using System.Linq;

public class PlayerCardSpawner : NetworkBehaviour
{
  private GameObject playerCards;

  [SerializeField] private GameObject clientLobbyItem, hostLobbyItem;

  public override void OnStartServer()
  {
    base.OnStartServer();
    playerCards = GameObject.FindGameObjectWithTag("PlayerCards");

    // print(NetworkServer.connections.Count);

    // GameObject hostCard = GameObject.FindWithTag("HostCard");

    // PlayerCard card = hostCard.AddComponent<PlayerCard>();

    // NetworkServer.AddPlayerForConnection(NetworkServer.connections.ElementAt(0).Value, hostCard);


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

    GameObject player;

    if (conn.connectionId == 0)
    {
      player = Instantiate(hostLobbyItem);
      player.transform.SetParent(playerCards.transform.parent, false);
    }
    else
    {
      player = Instantiate(clientLobbyItem, Vector3.zero, Quaternion.Euler(0, 0, 0), playerCards.transform);
    }

    player.GetComponent<PlayerCard>().DisplayNameUI.text = "Loading...";
    if (conn != NetworkServer.localConnection) player.GetComponent<PlayerCard>().kickBtn.gameObject.SetActive(true);
    NetworkServer.Spawn(player, conn);
    NetworkServer.AddPlayerForConnection(conn, player);
    NetworkServer.SetClientReady(conn);

    CallPlayerJoined();
  }

  [ClientRpc]
  void CallPlayerJoined()
  {
    foreach (GameObject card in GameObject.FindGameObjectsWithTag("PlayerCard"))
    {
      card.GetComponent<PlayerCard>().OnPlayerJoin();
    }
  }
}
