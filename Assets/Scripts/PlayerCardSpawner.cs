using UnityEngine;
using Mirror;
using System.Linq;

public class PlayerCardSpawner : NetworkBehaviour
{
  private GameObject playerCards;

  public override void OnStartServer()
  {
    base.OnStartServer();
    playerCards = GameObject.FindGameObjectWithTag("PlayerCards");

    print(NetworkServer.connections.Count);

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
    print("spawned card");
    GameObject player = Instantiate(MyNetworkManager.instance.playerCard, Vector3.zero, Quaternion.Euler(0, 0, 0), playerCards.transform);
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
