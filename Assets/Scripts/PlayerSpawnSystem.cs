using System.Collections;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawnSystem : NetworkBehaviour
{
  private GameObject[] spawnPoints;

  [SyncVar]
  public System.DateTime timeStamp;
  private MyNetworkManager nm = MyNetworkManager.instance;
  private Color[] colors = new Color[] {
    new Color(0.5882353f, 0.1137255f, 0.04313726f), // 961D0B
    new Color(0.0993236f, 0.4487756f, 0.6792453f), // 1972AD
    new Color(0.1027946f, 0.6226415f, 0.1877513f), // 1A9F30
    new Color(0.6235294f, 0.6018561f, 0.1019608f), // 9F991A
  };

  private void Awake()
  {
    Time.timeScale = 1;
    spawnPoints = GameObject.FindGameObjectsWithTag("Spawnpoint");
  }

  public override void OnStartServer()
  {
    base.OnStartServer();

    GameObject winningPlayer = null;
    for (int i = 0; i < nm.players.Count; i++)
    {
      NetworkConnectionToClient conn = nm.players.ElementAt(i).Key;
      GameObject playerInstance = Instantiate(nm.PlayerPrefab, spawnPoints[i].transform.position, Quaternion.Euler(0, 0, 0));
      // playerInstance.GetComponent<PlayerVars>().uiName.text = displayName;
      // NetworkServer.Spawn(playerInstance, Room.players.ElementAt(i).Key);
      // playerInstance.GetComponent<PlayerVars>().timeleft = timeStamp;
      NetworkServer.Spawn(playerInstance);
      NetworkServer.ReplacePlayerForConnection(conn, playerInstance);
      NetworkServer.SetClientReady(conn);
      if (conn == nm.winner)
      {
        winningPlayer = playerInstance; //playerInstance.GetComponent<PlayerVars>().crown.SetActive(true);
      }
    }

    if (nm.settings.enableBots && nm._BotSupport.Contains(SceneManager.GetActiveScene().path))
    {
      for (int i = 0; i < nm.settings.lobbySize - nm.players.Count; i++)
      {
        GameObject bot = Instantiate(nm.BotPrefab, spawnPoints[spawnPoints.Length - 1 - i].transform.position, Quaternion.identity);
        bot.name = "BOT" + i;
        NetworkServer.Spawn(bot);
        Rpc_SetPlayerColor(bot, i + nm.players.Count);
      }
    }

    for (int i = 0; i < nm.players.Count; i++)
    {
      NetworkConnectionToClient conn = nm.players.ElementAt(i).Key;
      Rpc_SetPlayerColor(conn.identity.gameObject, i);
    }

    if (winningPlayer != null)
    {
      EnableCrown(winningPlayer);
    }
  }

  [ClientRpc]
  private void EnableCrown(GameObject player)
  {
    player.GetComponent<PlayerRefs>().crown.SetActive(true);
    player.GetComponent<PlayerRefs>().uiName.transform.localPosition = new Vector3(0, 2, 0);
  }

  [Server]
  public IEnumerator Cmd_RespawnPlayer(NetworkConnection conn)
  {
    Rpc_SetPlayerPosition(conn.identity.gameObject);
    yield return new WaitForSecondsRealtime(5);

    PlayerBehavior pb = conn.identity.gameObject.GetComponent<PlayerBehavior>();

    pb.playerRefs.damageController.dead = false;
    pb.playerRefs.lockMovement = false;
    pb.playerRefs.lockShooting = false;
    pb.playerRefs.lockWeapon = false;

    Rpc_RespawnPlayer(conn.identity.gameObject);
    conn.identity.gameObject.GetComponent<DamageController>().health = conn.identity.gameObject.GetComponent<DamageController>().maxHealth;
  }

  [Server]
  public IEnumerator Cmd_RespawnBot(GameObject bot)
  {
    Rpc_SetPlayerPosition(bot);
    yield return new WaitForSecondsRealtime(5);

    BotRefs botRefs = bot.GetComponent<BotRefs>();

    botRefs.damageController.dead = false;
    botRefs.lockMovement = false;
    botRefs.lockShooting = false;
    botRefs.lockWeapon = false;

    Rpc_RespawnPlayer(bot);
    bot.GetComponent<DamageController>().health = bot.GetComponent<DamageController>().maxHealth;
  }

  [ClientRpc]
  private void Rpc_SetPlayerColor(GameObject player, int idx)
  {
    if (player.tag == "Player")
    {
      player.GetComponent<PlayerRefs>().graphics.sprites[0].color = colors[idx % 4];
    }
    else if (player.tag == "Bot")
    {
      player.GetComponent<BotRefs>().graphics.sprites[0].color = colors[idx % 4];
    }
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
    if (player.tag == "Bot")
    {
      BotRefs botRefs = player.GetComponent<BotRefs>();

      botRefs.rb.velocity = Vector2.zero;

      botRefs.uiName.gameObject.SetActive(true);

      // pb.health = pb.maxHealth;

      botRefs.graphics.EnableAll();
      botRefs.uiName.gameObject.SetActive(true);
      player.GetComponent<BoxCollider2D>().enabled = true;
      player.GetComponent<Rigidbody2D>().simulated = true;
    }
    else
    {
      PlayerBehavior pb = player.GetComponent<PlayerBehavior>();

      pb.playerRefs.rb.velocity = Vector2.zero;

      pb.playerRefs.uiName.gameObject.SetActive(true);

      // pb.health = pb.maxHealth;

      pb.playerRefs.graphics.EnableAll();
      pb.playerRefs.uiName.gameObject.SetActive(true);
      player.GetComponent<BoxCollider2D>().enabled = true;
      player.GetComponent<Rigidbody2D>().simulated = true;
    }
  }
}
