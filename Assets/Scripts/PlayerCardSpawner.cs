using UnityEngine;
using Mirror;
using System.Linq;

public class PlayerCardSpawner : NetworkBehaviour
{
  private GameObject playerCards;

  // Start is called before the first frame update
  public override void OnStartServer()
  {
    base.OnStartServer();
    playerCards = GameObject.FindGameObjectWithTag("PlayerCards");

    for (int i = 0; i < NetworkServer.connections.Count; i++)
    {
      SpawnCard(NetworkServer.connections.ElementAt(i).Value);
    }

    MyNetworkManager.instance.PlayerConnect += SpawnCard;

  }

  [Server]
  void SpawnCard(NetworkConnectionToClient conn)
  {
    GameObject player = Instantiate(MyNetworkManager.instance.playerCard, Vector3.zero, Quaternion.Euler(0, 0, 0), playerCards.transform);
    player.GetComponent<PlayerCard>().DisplayNameUI.text = "Loading...";
    if (conn != NetworkServer.localConnection) player.GetComponent<PlayerCard>().kickBtn.gameObject.SetActive(true);
    NetworkServer.Spawn(player, conn);
    NetworkServer.AddPlayerForConnection(conn, player);
  }
}
