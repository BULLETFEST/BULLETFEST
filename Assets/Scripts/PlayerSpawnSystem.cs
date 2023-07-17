using System.Collections;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawnSystem : NetworkBehaviour
{
  private GameObject[] spawnPoints;

  [SerializeField]
  GameObject botPrefab;

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
      GameObject playerInstance = Instantiate(nm.playerPrefab, spawnPoints[i].transform.position, Quaternion.Euler(0, 0, 0));
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
        GameObject bot = Instantiate(botPrefab, spawnPoints[spawnPoints.Length - 1 - i].transform.position, Quaternion.identity);
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
  public IEnumerator Cmd_RespawnPlayer(GameObject player)
  {
    Rpc_SetPlayerPosition(player);
    yield return new WaitForSecondsRealtime(5);

    ComponentRefs refs = player.GetComponent<ComponentRefs>();

    refs.damageController.dead = false;
    refs.lockMovement = false;
    refs.lockShooting = false;
    refs.lockWeapon = false;

    Rpc_RespawnPlayer(player);
    player.GetComponent<DamageController>().health = player.GetComponent<DamageController>().maxHealth;
  }

  [ClientRpc]
  private void Rpc_SetPlayerColor(GameObject player, int idx)
  {
    player.GetComponent<ComponentRefs>().graphics.sprites[0].color = colors[idx % 4];
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
    ComponentRefs refs = player.GetComponent<ComponentRefs>();

    refs.rb.velocity = Vector2.zero;
    refs.uiName.gameObject.SetActive(true);
    refs.graphics.EnableAll();
    refs.bc.enabled = true;
    refs.rb.simulated = true;
  }
}
