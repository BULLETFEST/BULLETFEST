using System.Linq;
using Mirror;
using UnityEngine;

public class LeaderboardItemSpawner : NetworkBehaviour
{
  GameObject leaderboard;
  GameObject leaderboardItem;

  MyNetworkManager Room;

  void Awake()
  {
    leaderboard = GameObject.FindGameObjectWithTag("Leaderboard");
    leaderboardItem = (GameObject)Resources.Load("Spawnable/LeaderboardItem");

    Room = MyNetworkManager.instance;
  }

  [Server]
  public override void OnStartServer()
  {
    base.OnStartServer();
    for (int i = 0; i < Room.players.Count; i++)
    {
      GameObject lbItem = Instantiate(leaderboardItem, Vector3.zero, Quaternion.Euler(0, 0, 0), leaderboard.transform);
      NetworkServer.Spawn(lbItem, Room.players.ElementAt(i).Key);
      NetworkServer.ReplacePlayerForConnection(Room.players.ElementAt(i).Key, lbItem);
    }
  }
}
