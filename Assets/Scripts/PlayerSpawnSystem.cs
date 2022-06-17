using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerSpawnSystem : NetworkBehaviour
{

  [Server]
  public void SpawnPlayer(NetworkConnection conn)
  {
    GameObject playerInstance = Instantiate(NetworkManager.singleton.playerPrefab, Vector3.zero, Quaternion.Euler(0, 0, 0));
    NetworkServer.Spawn(playerInstance, conn);
  }
}
