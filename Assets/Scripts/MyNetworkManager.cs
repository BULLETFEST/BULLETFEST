using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MyNetworkManager : NetworkManager
{
  public static MyNetworkManager instance { get; private set; }

  public GameSettings settings = new();
  private string[] queuedScenes;

  [HideInInspector]
  public ChatManager Chat;

  // public int[] sortedPlayerList = new int[4];

  public NetworkConnectionToClient winner;

  public System.Action PlayerUpdate, AllClientsReady;
  public System.Action<NetworkConnectionToClient> PlayerConnect, PlayerDisconnect, PlayerSpawn;

  [HideInInspector]
  public bool gameStarted = false,
              isHost = false;

  [HideInInspector]
  public string roomCode;

  public static int playableScenesCount { get; private set; } = 0;

  private int menuScenesCount = 0;

  [Header("Custom Variables")]
  [Scene] public string[] _4Players;
  [Scene] public string[] _6Players;
  [Scene] public string[] _8Players;
  [Scene] public string[] _BotSupport;

  [Scene] public string TESTING_SCENE;
  private bool hasFiredReadyEvent;
  private static bool firstInit = true;

  public override void Awake()
  {
    base.Awake();

    if (!firstInit)
    {
      settings.rounds = playableScenesCount;
      return;
    }

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
      GameManager.Instance.InitializeScene();
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

    if (GameManager.Instance.players.ContainsKey(conn.connectionId))
    {

      if (Utilities.FindWithType(out ChatManager chatManager))
      {
        chatManager.messages.Add($"R|{GameManager.Instance.players[conn.connectionId].displayName} has left the game");
      }

      GameManager.Instance.players.Remove(conn.connectionId);

      FirebaseManager.UpdateLobby(NetworkServer.connections.Count);

      // PlayerUpdate?.Invoke();

      if (gameStarted)
      {
        if (GameManager.Instance.players.Count == 1)
        {
          queuedScenes = new string[0];
        }
        GameManager.Instance.OnPlayerDie(conn.identity.gameObject.GetComponent<DamageController>());
      }
    }

    PlayerDisconnect?.Invoke(conn);
  }

  [Server]
  public void StartGame()
  {
    winner = null;

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

    if (Globals._testMode)
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
    if (queuedScenes.Length == 0)// || gameMode == GameMode.Deathmatch)
    {
      gameStarted = false;
      List<KeyValuePair<int, PlayerData>> temp = GameManager.Instance.players.ToList();

      temp.Sort(delegate (KeyValuePair<int, PlayerData> a, KeyValuePair<int, PlayerData> b)
      {
        return -a.Value.kills.CompareTo(b.Value.kills);
      });

      // sortedPlayerList = temp.ToDictionary(x => x.Key, x => x.Value).Keys.ToArray();

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
    GameManager.Instance.players.Clear();
    if (SceneManager.GetActiveScene().name != "MainMenu")
    {
      SceneManager.LoadScene(1);
    }
  }

  public override void OnStopClient()
  {
    base.OnStopClient();
    GameManager.Instance.players.Clear();
  }

  public void Disconnect()
  {
    if (Utilities.FindWithType(out ChatManager cm))
    {
      Destroy(cm.gameObject);
    }

    if (Utilities.FindWithType(out GameManager gm))
    {
      Destroy(gm.gameObject);
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
  }
}