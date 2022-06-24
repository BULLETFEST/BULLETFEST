using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public class PlayerSpawnSystem : NetworkBehaviour
{
  MyNetworkManager room;
  MyNetworkManager Room
  {
    get
    {
      if (room != null) { return room; }
      return room = NetworkManager.singleton as MyNetworkManager;
    }
  }

  GameObject[] spawnPoints;

  void Awake()
  {
    spawnPoints = GameObject.FindGameObjectsWithTag("Spawnpoint");
    print("h");
  }

  public override void OnStartServer()
  {
    base.OnStartServer();
    // foreach (KeyValuePair<NetworkConnectionToClient, string> data in Room.players)
    for (int i = 0; i < Room.players.Count; i++)
    {
      // SpawnPlayer(data.Key, data.Value);
      GameObject playerInstance = Instantiate(NetworkManager.singleton.playerPrefab, spawnPoints[i].transform.position, Quaternion.Euler(0, 0, 0));
      // playerInstance.GetComponent<PlayerVars>().uiName.text = displayName;
      NetworkServer.Spawn(playerInstance, Room.players.ElementAt(i).Key);
      NetworkServer.ReplacePlayerForConnection(Room.players.ElementAt(i).Key, playerInstance);
    }
  }
  // [Server]
  // public void SpawnPlayer(NetworkConnectionToClient conn, string displayName, GameObject point)
  // {
  //   GameObject playerInstance = Instantiate(NetworkManager.singleton.playerPrefab, Vector3.zero, Quaternion.Euler(0, 0, 0));
  //   // playerInstance.GetComponent<PlayerVars>().uiName.text = displayName;
  //   NetworkServer.Spawn(playerInstance, conn);
  //   NetworkServer.ReplacePlayerForConnection(conn, playerInstance);
  // }
}
