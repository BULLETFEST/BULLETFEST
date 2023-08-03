using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class GameManager : NetworkBehaviour
{

  public readonly SyncDictionary<int, PlayerData> players = new();

  public static GameManager Instance;

  public static readonly Color[] colors = new Color[] {
    new Color(0.5882353f, 0.1137255f, 0.04313726f), // 961D0B
    new Color(0.0993236f, 0.4487756f, 0.6792453f), // 1972AD
    new Color(0.1027946f, 0.6226415f, 0.1877513f), // 1A9F30
    new Color(0.6235294f, 0.6018561f, 0.1019608f), // 9F991A
  };

  private MyNetworkManager nm => MyNetworkManager.instance;

  [SerializeField]
  private GameObject PlayerSpawnSystemPrefab,
           GunSpawnerPrefab,
           WinnerPanelPrefab,
           ScoreboardManagerPrefab;


  public NetworkConnectionToClient _winner { get; private set; }

  private void Awake()
  {
    if (Instance == null)
    {
      Instance = this;
    }
    else
    {
      Destroy(gameObject);
    }
  }

  private void Start()
  {
    DontDestroyOnLoad(gameObject);
  }

  [ServerCallback]
  public void InitializeScene()
  {
    // Instantiate PlayerSpawner
    GameObject go = Instantiate(PlayerSpawnSystemPrefab);
    if (nm.settings.gameMode == GameSettings.GameMode.Deathmatch)
    {
      go.GetComponent<PlayerSpawnSystem>().timeStamp = System.DateTime.UtcNow.AddMinutes(nm.settings.deathmatchLength);
    }
    NetworkServer.Spawn(go);


    // Instantiate GunSpawner
    go = Instantiate(GunSpawnerPrefab);
    go.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height + 50, 10));
    go.GetComponent<BoxCollider2D>().size = new Vector2(Camera.main.orthographicSize * Camera.main.aspect * 1.75f, 1);
    NetworkServer.Spawn(go);


    // Instantiate ScoreboardManager;
    go = Instantiate(ScoreboardManagerPrefab);
    NetworkServer.Spawn(go);
  }

  public readonly List<DamageController> _damageControllers = new();

  [Server]
  public void OnPlayerDie(DamageController player)
  {
    int aliveCount = 0;
    DamageController lastAlive = null;

    if (nm.settings.gameMode == GameSettings.GameMode.Deathmatch)
    {
      StartCoroutine(FindObjectOfType<PlayerSpawnSystem>().Cmd_RespawnPlayer(player.gameObject));
      return;
    }


    foreach (DamageController controller in _damageControllers)
    {
      if (!controller.dead)
      {
        aliveCount++;
        lastAlive = controller;
      }
    }

    if (lastAlive == null)
    {
      lastAlive = _damageControllers[0];
    }

    if (aliveCount <= 1)
    {
      AnnounceWinner(lastAlive, lastAlive.gameObject.GetComponent<BotRefs>() != null);
    }
  }

  public void AnnounceWinner(DamageController winner, bool isBot)
  {
    if (Utilities.FindWithType(out PlayerSpawnSystem go))
    {
      NetworkServer.Destroy(go.gameObject);
    }

    int connId = 0;

    if (nm.settings.gameMode == GameSettings.GameMode.Deathmatch)
    {
      connId = players.OrderBy(x => x.Value.kills).ToList().Last().Key;
    }
    else if (!isBot)
    {
      connId = winner.gameObject.GetComponent<NetworkIdentity>().connectionToClient.connectionId;
      players[connId].wins++;

      // As mentioned in DamageController.cs -> ServerDie()
      players[connId] = players[connId];
    }

    if (!isBot)
    {
      _winner = NetworkServer.connections[connId];
    }

    GameObject winnerCanvas = Instantiate(WinnerPanelPrefab);
    NetworkServer.Spawn(winnerCanvas);


    UpdateWinnerCanvas(winnerCanvas.GetComponent<WinnerUI>(),
                       (isBot ? "BOT" : players[connId].displayName) + " won the round!",
                       isBot ? -1 : System.Array.IndexOf(NetworkServer.connections.Keys.ToArray(), connId));
  }

  [ClientRpc]
  private void UpdateWinnerCanvas(WinnerUI winnerCanvas, string playerName, int colorIdx)
  {
    winnerCanvas.winnerText.text = playerName;

    winnerCanvas.playerImage.color = colorIdx == -1 ? new Color(0.3936009f, 0.5186465f, 0.5754717f) : colors[colorIdx % colors.Length];
  }
}
