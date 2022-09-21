using System.Collections;
using UnityEngine;
using Mirror;
using System.Linq;

public class PlayerSpawnSystem : NetworkBehaviour
{
  GameObject[] spawnPoints;

  [SyncVar]
  public System.DateTime timeStamp;

  Color[] colors = new Color[] {
    new Color(0.5882353f, 0.1137255f, 0.04313726f), // 961D0B
    new Color(0.0993236f, 0.4487756f, 0.6792453f), // 1972AD
    new Color(0.1027946f, 0.6226415f, 0.1877513f), // 1A9F30
    new Color(0.6235294f, 0.6018561f, 0.1019608f), // 9F991A
  };

  void Awake()
  {
    Time.timeScale = 1;
    spawnPoints = GameObject.FindGameObjectsWithTag("Spawnpoint");
  }

  public override void OnStartServer()
  {
    base.OnStartServer();

    GameObject winningPlayer = null;
    for (int i = 0; i < MyNetworkManager.instance.players.Count; i++)
    {
      NetworkConnectionToClient conn = MyNetworkManager.instance.players.ElementAt(i).Key;
      GameObject playerInstance = Instantiate(NetworkManager.singleton.playerPrefab, spawnPoints[i].transform.position, Quaternion.Euler(0, 0, 0));
      // playerInstance.GetComponent<PlayerVars>().uiName.text = displayName;
      // NetworkServer.Spawn(playerInstance, Room.players.ElementAt(i).Key);
      // playerInstance.GetComponent<PlayerVars>().timeleft = timeStamp;
      NetworkServer.Spawn(playerInstance);
      NetworkServer.ReplacePlayerForConnection(conn, playerInstance);
      NetworkServer.SetClientReady(conn);
      if (conn == MyNetworkManager.instance.winner) winningPlayer = playerInstance; //playerInstance.GetComponent<PlayerVars>().crown.SetActive(true);
    }

    for (int i = 0; i < MyNetworkManager.instance.players.Count; i++)
    {
      NetworkConnectionToClient conn = MyNetworkManager.instance.players.ElementAt(i).Key;
      Rpc_SetPlayerColor(conn.identity.gameObject, i);
    }

    if (winningPlayer != null) EnableCrown(winningPlayer);
  }

  [ClientRpc]
  void EnableCrown(GameObject player)
  {
    player.GetComponent<PlayerVars>().crown.SetActive(true);
    player.GetComponent<PlayerVars>().uiName.transform.localPosition = new Vector3(0, 2, 0);
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
  private void Rpc_SetPlayerColor(GameObject player, int idx)
  {
    player.GetComponent<PlayerVars>().graphics.sprites[0].color = colors[idx % 4];
  }

  [ClientRpc]
  private void Rpc_SetPlayerPosition(GameObject player)
  {
    player.transform.position = spawnPoints[Random.Range(0, spawnPoints.Length)].transform.position;
    player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
    // player.transform.position = new Vector2(player.transform.position.x, player.transform.position.y + 20);
  }

  [ClientRpc]
  private void Rpc_RespawnPlayer(GameObject player)
  {
    PlayerBehavior pb = player.GetComponent<PlayerBehavior>();

    pb.dead = false;
    pb.playerVars.lockMovement = false;
    pb.playerVars.lockShooting = false;
    pb.playerVars.lockWeapon = false;

    pb.playerVars.rb.velocity = Vector2.zero;

    pb.playerVars.uiName.gameObject.SetActive(true);

    // pb.health = pb.maxHealth;

    pb.playerVars.graphics.EnableAll();
    pb.playerVars.uiName.gameObject.SetActive(true);
    player.GetComponent<BoxCollider2D>().enabled = true;
    player.GetComponent<Rigidbody2D>().simulated = true;
  }
}
