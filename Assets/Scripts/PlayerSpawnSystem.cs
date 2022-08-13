using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using UnityEngine.Video;

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

  [SyncVar]
  public System.DateTime timeStamp;

  void Awake()
  {
    Time.timeScale = 1;
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
      // playerInstance.GetComponent<PlayerVars>().timeleft = timeStamp;
      NetworkServer.Spawn(playerInstance);
      NetworkServer.ReplacePlayerForConnection(Room.players.ElementAt(i).Key, playerInstance);
      NetworkServer.SetClientReady(Room.players.ElementAt(i).Key);
    }
  }

  [Server]
  public IEnumerator Cmd_RespawnPlayer(NetworkConnection conn)
  {
    Rpc_SetPlayerPosition(conn.identity.gameObject);
    yield return new WaitForSecondsRealtime(5);

    Rpc_RespawnPlayer(conn.identity.gameObject);
    conn.identity.gameObject.GetComponent<PlayerBehavior>().health = 10;
  }

  [ClientRpc]
  private void Rpc_SetPlayerPosition(GameObject player)
  {
    player.transform.position = spawnPoints[Random.Range(0, spawnPoints.Length)].transform.position;
    player.transform.position = new Vector2(player.transform.position.x, player.transform.position.y + 20);
  }

  [ClientRpc]
  private void Rpc_RespawnPlayer(GameObject player)
  {
    PlayerBehavior pb = player.GetComponent<PlayerBehavior>();

    pb.dead = false;
    pb.playerVars.lockMovement = false;
    pb.playerVars.lockShooting = false;
    pb.playerVars.lockWeapon = false;

    pb.playerVars.uiName.gameObject.SetActive(true);

    // pb.health = pb.maxHealth;

    pb.playerVars.graphics.EnableAll();
    player.GetComponent<BoxCollider2D>().enabled = true;
    player.GetComponent<Rigidbody2D>().simulated = true;
  }
}
