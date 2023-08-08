using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
  public enum GameState
  {
    Lobby,
    Started,
    Ended
  }

  public readonly SyncDictionary<int, PlayerData> players = new();

  public static GameManager Instance;

  [SyncVar]
  private readonly GameSettings _settings = new();

  public static GameSettings settings { get; private set; }

  Color[] colors => Globals.colors;

  private MyNetworkManager nm => MyNetworkManager.Instance;

  [SerializeField]
  private GameObject PlayerSpawnSystemPrefab,
           GunSpawnerPrefab,
           WinnerPanelPrefab,
           ScoreboardManagerPrefab;

  public NetworkConnectionToClient _winner { get; private set; }

  public GameState state = GameState.Lobby;

  private string[] queuedScenes;

  private void Awake()
  {
    if (Instance == null)
    {
      Instance = this;
    }
    else
    {
      NetworkServer.Destroy(gameObject);
    }

    settings = _settings;
    MyNetworkManager.Instance.PlayerConnect += OnPlayerJoin;
  }

  private void Start()
  {
    DontDestroyOnLoad(gameObject);

    settings.rounds = MyNetworkManager.playableScenesCount;
  }

  [ServerCallback]
  public void InitializeScene()
  {
    // Instantiate PlayerSpawner
    GameObject go = Instantiate(PlayerSpawnSystemPrefab);
    if (settings.gameMode == GameSettings.GameMode.Deathmatch)
    {
      go.GetComponent<PlayerSpawnSystem>().timeStamp = System.DateTime.UtcNow.AddMinutes(settings.deathmatchLength);
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
  public void OnPlayerJoin(NetworkConnectionToClient conn)
  {
    if (state == GameState.Started)
    {
      LateJoin(conn);
    }
  }

  void LateJoin(NetworkConnectionToClient conn)
  {
    if (!settings.allowLateJoin)
    {
      conn.Send(new Message.ServerMessge
      {
        titleText = "Disconnected",
        contentText = "Game has started",
        _alignment = 2,
        disconnect = true
      });
      conn.Disconnect();
      return;
    }

    FindObjectOfType<PlayerSpawnSystem>().SpawnPlayer(conn);


  }

  [Server]
  public void OnPlayerDie(DamageController player)
  {
    int aliveCount = 0;
    DamageController lastAlive = null;

    if (settings.gameMode == GameSettings.GameMode.Deathmatch)
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

    if (settings.gameMode == GameSettings.GameMode.Deathmatch)
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


  #region SCENE LOGIC

  [Server]
  public void StartGame()
  {
    int sceneCount = SceneManager.sceneCountInBuildSettings;
    List<string> _scenes = new();

    switch (settings.lobbySize)
    {
      case 4:
      default:
        _scenes = nm._4Players.ToList();
        break;
      case 6:
        _scenes = nm._6Players.ToList();
        break;
    }

    if (settings.enableBots)
    {
      _scenes = nm._BotSupport.Where(x => _scenes.Contains(x)).ToList();
    }

    if (settings.gameMode == GameSettings.GameMode.Elimination)
    {
      settings.rounds = Mathf.Clamp(settings.rounds, 1, MyNetworkManager.playableScenesCount);
      while (_scenes.Count > settings.rounds)
      {
        _scenes.RemoveAt(Random.Range(0, _scenes.Count));
      }
    }
    else
    {
      List<string> temp = new();
      if (settings.chosenMap == 0)
      {
        int chosenMapIdx = Random.Range(0, _scenes.Count);

        temp.Add(_scenes[chosenMapIdx]);

        _scenes = temp;
      }
      else
      {
        temp.Add(_scenes[settings.chosenMap - 1]);

        _scenes = temp;
      }
    }

    if (Globals._testMap)
    {
      _scenes = new()
      {
        nm.TESTING_SCENE
      };
    }

    queuedScenes = _scenes.ToArray();

    state = GameState.Started;

    FirebaseManager.UpdateLobby(NetworkServer.connections.Count);

    CycleMap();
  }

  [Server]
  public void CycleMap()
  {
    if (queuedScenes.Length == 0) { EndGame(); return; }

    int chosenScene = Random.Range(0, queuedScenes.Length);

    string chosenSceneId = queuedScenes[chosenScene];

    List<string> _scenes = queuedScenes.ToList();

    _scenes.Remove(chosenSceneId);

    queuedScenes = _scenes.ToArray();

    nm.ServerChangeScene(chosenSceneId);
  }

  [Server]
  public void EndGame()
  {
    state = GameState.Ended;
    nm.ServerChangeScene("End");

    queuedScenes = new string[0];
  }

  #endregion
}
