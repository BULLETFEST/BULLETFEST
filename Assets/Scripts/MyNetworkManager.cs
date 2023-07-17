using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MyNetworkManager : NetworkManager
{
  public static MyNetworkManager instance;

  public GameSettings settings = new();
  private string[] queuedScenes;

  [SerializeField]
  GameObject PlayerSpawnSystemPrefab,
             GunSpawnerPrefab,
             WinnerPanelPrefab;

  public ChatManager Chat;

  [HideInInspector]
  public GameObject botWinner;

  public Dictionary<NetworkConnectionToClient, PlayerData> players { get; } = new Dictionary<NetworkConnectionToClient, PlayerData>();

  public NetworkConnectionToClient[] sortedPlayerList = new NetworkConnectionToClient[4];

  public NetworkConnectionToClient winner;

  public System.Action PlayerUpdate, AllClientsReady;
  public System.Action<NetworkConnectionToClient> PlayerConnect, PlayerDisconnect, PlayerSpawn;

  [HideInInspector]
  public bool gameStarted = false,
              isHost = false;

  [HideInInspector]
  public string roomCode;

  public static int playableScenesCount = 0, menuScenesCount = 0;

  [Scene] public string[] _4Players, _6Players, _8Players, _BotSupport;

  [Scene] public string TESTING_SCENE;
  private bool hasFiredReadyEvent;
  private static bool firstInit = true;

  public bool enableTestMode;

#if UNITY_EDITOR
  public static bool testMode = false;
#else
public static readonly bool testMode = false;
#endif

  public override void Awake()
  {
    base.Awake();

    if (!firstInit)
    {
      settings.rounds = playableScenesCount;
      return;
    }

#if UNITY_EDITOR
    if (testMode)
    {
      testMode = enableTestMode;
    }
#endif

    for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
    {
      string s = SceneUtility.GetScenePathByBuildIndex(i);

      if (s.Contains("GameScenes"))
      {
        playableScenesCount++;
      }
      else
      {
        menuScenesCount++;
      }
    }

    settings.rounds = playableScenesCount;

    firstInit = false;
  }

  public override void OnServerAddPlayer(NetworkConnectionToClient conn)
  {
    // base.OnServerAddPlayer(conn);

    PlayerSpawn?.Invoke(conn);
  }

  public override void Start()
  {
    base.Start();

    if (instance == null)
    {
      instance = this;
    }
    else if (instance != this)
    {
      Destroy(gameObject);
    }

    // transport = gameObject.AddComponent<EpicTransport.EosTransport>();
    // FindObjectOfType<NetworkManager>().transport = transport;

    foreach (GameObject prefab in Resources.LoadAll<GameObject>("Spawnable"))
    {
      NetworkClient.RegisterPrefab(prefab);
    }

    NetworkClient.RegisterHandler<Message.ServerMessge>(OnServerMessage);
  }

  public override void LateUpdate()
  {
    base.LateUpdate();

    if (isHost && !hasFiredReadyEvent)
    {
      bool allReady = true;

      for (int i = 0; i < NetworkServer.connections.Count; i++)
      {
        if (NetworkServer.connections.ElementAt(i).Value.identity == null)
        {
          allReady = false;
          break;
        }
      }

      if (allReady)
      {
        AllClientsReady?.Invoke();
        hasFiredReadyEvent = true;
      }
    }
  }

  private void OnServerMessage(Message.ServerMessge msg)
  {
    Message.DisplayMessage(msg.titleText, msg.contentText, msg.alignment);
    if (msg.disconnect)
    {
      Disconnect();
    }
  }

  public override void OnStartServer()
  {
    base.OnStartServer();
    StartCoroutine(KeepAlive());
  }

  private IEnumerator KeepAlive()
  {
    FirebaseManager.KeepAlive();

    yield return new WaitForSecondsRealtime(120f);

    StartCoroutine(KeepAlive());
  }

  [Server]
  public override void OnServerConnect(NetworkConnectionToClient conn)
  {
    base.OnServerConnect(conn);

    if (NetworkServer.connections.Count > settings.lobbySize)
    {
      conn.Send(new Message.ServerMessge
      {
        titleText = "Disconnected",
        contentText = "Game is full",
        _alignment = 2,
        disconnect = true
      });
      conn.Disconnect();
      return;
    }

    if (SceneManager.GetActiveScene().name != "Lobby")
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



    PlayerConnect?.Invoke(conn);

    FirebaseManager.UpdateLobby(NetworkServer.connections.Count);
  }

  public override void OnServerSceneChanged(string sceneName)
  {
    hasFiredReadyEvent = false;

    base.OnServerSceneChanged(sceneName);

    if (SceneManager.GetActiveScene().buildIndex > menuScenesCount - 1)
    {
      if (!FindObjectOfType<PlayerSpawnSystem>())
      {
        GameObject go = Instantiate(PlayerSpawnSystemPrefab);
        if (settings.gameMode == GameSettings.GameMode.Deathmatch)
        {
          go.GetComponent<PlayerSpawnSystem>().timeStamp = System.DateTime.UtcNow.AddMinutes(settings.deathmatchLength);
        }
        NetworkServer.Spawn(go);
      }

      if (!FindObjectOfType<GunSpawner>())
      {
        GameObject go = Instantiate(GunSpawnerPrefab);

        go.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height + 50, 10));
        go.GetComponent<BoxCollider2D>().size = new Vector2(Camera.main.orthographicSize * Camera.main.aspect * 1.75f, 1);
        NetworkServer.Spawn(go);
      }

    }
    if (sceneName == "Lobby")
    {
      FirebaseManager.UpdateLobby(NetworkServer.connections.Count);
    }
  }

  public override void OnServerDisconnect(NetworkConnectionToClient conn)
  {
    if (SceneManager.GetActiveScene().name != "End")
    {
      base.OnServerDisconnect(conn);
    }

    if (players.ContainsKey(conn))
    {

      if (Utilities.FindWithType(out ChatManager chatManager))
      {
        chatManager.messages.Add($"R|{players[conn].displayName} has left the game");
      }

      players.Remove(conn);

      FirebaseManager.UpdateLobby(NetworkServer.connections.Count);

      // PlayerUpdate?.Invoke();

      if (gameStarted)
      {
        if (players.Count == 1)
        {
          queuedScenes = new string[0];
        }

        deadPlayers--;
        OnPlayerDie(conn.identity.gameObject);
      }
    }

    PlayerDisconnect?.Invoke(conn);
  }

  private int deadPlayers = 0;

  [Server]
  public void OnPlayerDie(GameObject player)
  {
    BotPathfinding[] bots = FindObjectsOfType<BotPathfinding>();

    if (NetworkServer.connections.Count == 1 && bots.Length <= 0)
    {
      AnnounceWinner(bots);
    }
    else if (settings.gameMode == GameSettings.GameMode.Elimination)
    {
      deadPlayers++;
      if (deadPlayers == NetworkServer.connections.Count + bots.Length - 1)
      {
        AnnounceWinner(bots);
      }
    }
    else
    {
      StartCoroutine(FindObjectOfType<PlayerSpawnSystem>().Cmd_RespawnPlayer(player));
    }
  }

  [Server]
  public void AnnounceWinner(BotPathfinding[] bots)
  {
    winner = null;
    botWinner = null;

    if (Utilities.FindWithType(out PlayerSpawnSystem go))
    {
      NetworkServer.Destroy(go.gameObject);
    }

    // If all players have left, choose host to be the winner
    if (players.Count == 1 && bots.Length <= 0)
    {
      winner = players.ElementAt(0).Key;
    }
    // If gamemode is elimination, choose last player alive
    else if (settings.gameMode == 0)
    {
      GameObject[] alivePlayers = FindObjectsOfType<PlayerBehavior>().Where(x => !x.GetComponent<DamageController>().dead).Select(x => x.gameObject).ToArray();

      winner = alivePlayers.Length <= 0 ? null : alivePlayers[0].GetComponent<NetworkIdentity>().connectionToClient;

      if (winner == null)
      {
        botWinner = GameObject.FindGameObjectsWithTag("Bot").Where(x => !x.GetComponent<DamageController>().dead).ToArray()[0];
      }
    }
    // If gamemode is deathmatch, choose player with most kills
    else
    {
      // https://stackoverflow.com/a/1332/11420492
      //winner = (from entry in players orderby entry.Value descending select entry).First().Key;

      // https://stackoverflow.com/a/4157151/11420492
      winner = players.OrderBy(x => x.Value.kills).ToList().Last().Key;
    }

    if (botWinner == null)
    {
      players[winner].wins++;
    }

    GameObject winnerUi = Instantiate(WinnerPanelPrefab);
    NetworkServer.Spawn(winnerUi);

    string winnerName = botWinner != null ? "BOT" : players[winner].displayName;
    int winnerIdx = botWinner != null ? -1 : System.Array.IndexOf(NetworkServer.connections.Values.ToArray(), winner);

    FindObjectOfType<Server>().SetWinnerText($"{winnerName} won the round!", winnerIdx);
  }

  [Server]
  public void StartGame()
  {
    winner = null;
    botWinner = null;

    int sceneCount = SceneManager.sceneCountInBuildSettings;
    List<string> _scenes = new();

    switch (settings.lobbySize)
    {
      case 4:
      default:
        _scenes = _4Players.ToList();
        break;
      case 6:
        _scenes = _6Players.ToList();
        break;
    }

    print(testMode);

    if (!testMode)
    {
      if (settings.enableBots)
      {
        _scenes = _BotSupport.Where(x => _scenes.Contains(x)).ToList();
      }

      if (settings.gameMode == GameSettings.GameMode.Elimination)
      {
        settings.rounds = Mathf.Clamp(settings.rounds, 1, playableScenesCount);
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
    }
    else
    {
      _scenes = new()
      {
        TESTING_SCENE
      };
    }

    queuedScenes = _scenes.ToArray();

    gameStarted = true;

    FirebaseManager.UpdateLobby(NetworkServer.connections.Count);

    CycleMap();
  }

  [Server]
  public void CycleMap()
  {
    deadPlayers = 0;

    if (queuedScenes.Length == 0)// || gameMode == GameMode.Deathmatch)
    {
      gameStarted = false;
      List<KeyValuePair<NetworkConnectionToClient, PlayerData>> temp = players.ToList();

      temp.Sort(delegate (KeyValuePair<NetworkConnectionToClient, PlayerData> a, KeyValuePair<NetworkConnectionToClient, PlayerData> b)
      {
        return -a.Value.kills.CompareTo(b.Value.kills);
      });

      sortedPlayerList = temp.ToDictionary(x => x.Key, x => x.Value).Keys.ToArray();

      ServerChangeScene("End");
      return;
    }

    int chosenScene = Random.Range(0, queuedScenes.Length);

    string chosenSceneId = queuedScenes[chosenScene];

    List<string> _scenes = queuedScenes.ToList();

    _scenes.Remove(chosenSceneId);

    queuedScenes = _scenes.ToArray();

    ServerChangeScene(chosenSceneId);
  }

  public override void OnStopHost()
  {
    base.OnStopHost();
    isHost = false;
    players.Clear();
    if (SceneManager.GetActiveScene().name != "MainMenu")
    {
      SceneManager.LoadScene(1);
    }
  }

  public override void OnStopClient()
  {
    base.OnStopClient();
    players.Clear();
    // if (SceneManager.GetActiveScene().name != "MainMenu") SceneManager.LoadScene(1);
  }

  public void Disconnect()
  {
    if (Utilities.FindWithType(out ChatManager cm))
    {
      Destroy(cm.gameObject);
    }

    if (mode == NetworkManagerMode.ServerOnly)
    {
      FirebaseManager.CloseLobby();
      StopServer();
    }
    else if (mode == NetworkManagerMode.Host)
    {
      FirebaseManager.CloseLobby();
      StopHost();
    }
    else if (mode == NetworkManagerMode.ClientOnly)
    {
      StopClient();
    }

    // SceneManager.LoadSceneAsync("MainMenu");
  }
}