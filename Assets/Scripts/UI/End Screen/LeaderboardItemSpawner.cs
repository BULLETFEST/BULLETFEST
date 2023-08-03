using System.Linq;
using Mirror;
using UnityEngine;

public class LeaderboardItemSpawner : NetworkBehaviour
{
  private GameObject leaderboard;
  private GameObject leaderboardItem;

  private void Awake()
  {
    leaderboard = GameObject.FindGameObjectWithTag("Leaderboard");
    leaderboardItem = (GameObject)Resources.Load("Spawnable/LeaderboardItem");
  }

  [Server]
  public override void OnStartServer()
  {
    base.OnStartServer();
    for (int i = 0; i < GameManager.Instance.players.Count; i++)
    {
      GameObject lbItem = Instantiate(leaderboardItem, Vector3.zero, Quaternion.Euler(0, 0, 0), leaderboard.transform);
      NetworkServer.Spawn(lbItem, NetworkServer.connections[GameManager.Instance.players.ElementAt(i).Key]);
      NetworkServer.ReplacePlayerForConnection(NetworkServer.connections[GameManager.Instance.players.ElementAt(i).Key], lbItem);
    }
  }
}
