using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

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

  void Start()
  {
    // if (FindObjectsOfType<PlayerSpawnSystem>().Length > 1) NetworkServer.Destroy(gameObject);
  }
  public override void OnStartServer()
  {
    base.OnStartServer();
    foreach (KeyValuePair<NetworkConnectionToClient, string> data in Room.players)
    {
      SpawnPlayer(data.Key, data.Value);
    }
  }
  [Server]
  public void SpawnPlayer(NetworkConnectionToClient conn, string displayName)
  {
    GameObject playerInstance = Instantiate(NetworkManager.singleton.playerPrefab, Vector3.zero, Quaternion.Euler(0, 0, 0));
    // playerInstance.GetComponent<PlayerVars>().uiName.text = displayName;
    NetworkServer.Spawn(playerInstance, conn);
    NetworkServer.ReplacePlayerForConnection(conn, playerInstance);
  }
}
