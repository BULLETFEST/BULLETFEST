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
  }

  public override void OnStartServer()
  {
    base.OnStartServer();
    for (int i = 0; i < Room.players.Count; i++)
    {
      GameObject playerInstance = Instantiate(NetworkManager.singleton.playerPrefab, spawnPoints[i].transform.position, Quaternion.Euler(0, 0, 0));
      // playerInstance.GetComponent<PlayerVars>().uiName.text = displayName;
      // NetworkServer.Spawn(playerInstance, Room.players.ElementAt(i).Key);
      NetworkServer.Spawn(playerInstance);
      NetworkServer.ReplacePlayerForConnection(Room.players.ElementAt(i).Key, playerInstance);
      NetworkServer.SetClientReady(Room.players.ElementAt(i).Key);
    }
  }
}
