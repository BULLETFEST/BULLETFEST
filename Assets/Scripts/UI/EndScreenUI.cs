using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using UnityEngine.UI;

public class EndScreenUI : NetworkBehaviour
{
  GameObject leaderboard;
  GameObject leaderboardItem;

  private MyNetworkManager room;
  public MyNetworkManager Room
  {
    get
    {
      if (room != null) { return room; }
      return room = NetworkManager.singleton as MyNetworkManager;
    }
  }

  // Start is called before the first frame update
  void Awake()
  {
    leaderboard = GameObject.FindGameObjectWithTag("Leaderboard");
    leaderboardItem = (GameObject)Resources.Load("SpawnableNoNetId/LeaderboardItem");

    Cursor.visible = true;

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



  public Button playAgain, exit;

  public override void OnStartClient()
  {
    base.OnStartClient();

    if (Room.isHost)
    {
      playAgain.gameObject.SetActive(true);

      playAgain.onClick.AddListener(delegate
      {
        // Room.
        Room.ServerChangeScene("Lobby");

      });
    }
    else Destroy(playAgain);

    // exit.onClick.AddListener(delegate { Room.Disconnect(); });

  }

  public void Exit() => Room.Disconnect();
}
