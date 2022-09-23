using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class MyNetworkManager : NetworkManager
{
  public static MyNetworkManager instance;

  string[] queuedScenes;

  [SerializeField]
  public GameObject PlayerSpawnSystemPrefab,
                     WinnerPanelPrefab,
                     PlayerPrefab,
                     BotPrefab;

  public Dictionary<NetworkConnectionToClient, PlayerData> players { get; } = new Dictionary<NetworkConnectionToClient, PlayerData>();

  public NetworkConnectionToClient[] sortedPlayerList = new NetworkConnectionToClient[4];

  public NetworkConnectionToClient winner;
  public GameObject botWinner;

  public System.Action PlayerUpdate;
  public System.Action<NetworkConnectionToClient> PlayerConnect, PlayerDisconnect;

  [HideInInspector]
  public bool gameStarted = false,
              isHost = false;

  [HideInInspector]
  public string roomCode;

  [HideInInspector]
  public PrivacyType privacyType = PrivacyType.Public;

  [HideInInspector]
  public GameMode gameMode = 0;

  [HideInInspector]
  public float deathmatchLength = 1;

  [HideInInspector]
  public int rounds,
             chosenMap = 0;

  public static int playableScenesCount = 0, menuScenesCount = 0;

  [Scene] public string[] _4Players, _6Players, _8Players, _BotSupport;

  public int maxPlayers = 4;

  public bool enableBots = false;

  public override void Awake()
  {
    base.Awake();

    for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
    {
      string s = SceneUtility.GetScenePathByBuildIndex(i);

      if (s.Contains("GameScenes")) playableScenesCount++;
      else menuScenesCount++;
    }

    rounds = playableScenesCount;
  }

  public override void Start()
  {
    base.Start();

    if (instance == null)
      instance = this;
    else if (instance != this)
      Destroy(gameObject);

    foreach (GameObject prefab in Resources.LoadAll<GameObject>("Spawnable"))
    {
      NetworkClient.RegisterPrefab(prefab);
    }

    NetworkClient.RegisterHandler<Message.ServerMessge>(OnServerMessage);
  }

  void OnServerMessage(Message.ServerMessge msg)
  {
    Message.DisplayMessage(msg.titleText, msg.contentText, msg.alignment);
    if (msg.disconnect) Utilities.Disconnect();
  }

  public override void OnStartServer()
  {
    base.OnStartServer();
    isHost = true;
    StartCoroutine(KeepAlive());
  }

  IEnumerator KeepAlive()
  {
    try
    {
      FirebaseManager.KeepAlive();
    }
    catch { }

    yield return new WaitForSecondsRealtime(120f);

    StartCoroutine(KeepAlive());
  }

  [Server]
  public override void OnServerConnect(NetworkConnectionToClient conn)
  {
    base.OnServerConnect(conn);

    if (NetworkServer.connections.Count > maxPlayers)
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

    FirebaseManager.UpdateLobby(NetworkServer.connections.Count, gameMode.ToString(), privacyType.ToString().ToLower(), gameStarted);
  }

  public override void OnServerSceneChanged(string sceneName)
  {
    base.OnServerSceneChanged(sceneName);

    if (SceneManager.GetActiveScene().buildIndex > menuScenesCount - 1)
    {
      if (!FindObjectOfType<PlayerSpawnSystem>())
      {
        GameObject go = Instantiate(PlayerSpawnSystemPrefab);
        if (gameMode == GameMode.Deathmatch)
        {
          go.GetComponent<PlayerSpawnSystem>().timeStamp = System.DateTime.UtcNow.AddMinutes(deathmatchLength);
        }
        NetworkServer.Spawn(go);
      }
    }
    if (sceneName == "Lobby")
    {
      FirebaseManager.UpdateLobby(NetworkServer.connections.Count, gameMode.ToString(), privacyType.ToString().ToLower(), false);
    }
  }

  public override void OnClientSceneChanged()
  {
    base.OnClientSceneChanged();

    if (SceneManager.GetActiveScene().name == "End") return;

    VideoPlayer v = Camera.main.gameObject.AddComponent<VideoPlayer>();
    v.clip = Resources.Load<VideoClip>("glitch");
    v.isLooping = true;
    v.playOnAwake = true;
    v.waitForFirstFrame = true;
    v.playbackSpeed = 1.75f;
    v.targetCameraAlpha = 0.222f;
    v.aspectRatio = VideoAspectRatio.FitInside;
    v.audioOutputMode = VideoAudioOutputMode.None;
    v.renderMode = VideoRenderMode.CameraFarPlane;

    v.Play();
  }

  public override void OnServerDisconnect(NetworkConnectionToClient conn)
  {
    if (SceneManager.GetActiveScene().name != "End") base.OnServerDisconnect(conn);

    if (players.ContainsKey(conn))
    {
      players.Remove(conn);

      FirebaseManager.UpdateLobby(NetworkServer.connections.Count, gameMode.ToString(), privacyType.ToString().ToLower(), gameStarted);

      PlayerUpdate?.Invoke();

      if (gameStarted)
      {
        if (players.Count == 1) queuedScenes = new string[0];
        deadPlayers--;
        OnPlayerDie(conn);
      }
    }

    PlayerDisconnect?.Invoke(conn);
  }


  int deadPlayers = 0;

  [Server]
  public void OnPlayerDie(NetworkConnectionToClient conn)
  {
    BotPathfinding[] bots = FindObjectsOfType<BotPathfinding>();

    if (NetworkServer.connections.Count == 1 && bots.Length <= 0) AnnounceWinner(bots);
    else if (gameMode == GameMode.Elimination)
    {
      deadPlayers++;
      if (deadPlayers == (NetworkServer.connections.Count + bots.Length) - 1)
      {
        AnnounceWinner(bots);
      }
    }
    else
    {
      StartCoroutine(FindObjectOfType<PlayerSpawnSystem>().Cmd_RespawnPlayer(conn));
    }
  }

  [Server]
  public void AnnounceWinner(BotPathfinding[] bots)
  {
    if (Utilities.FindWithType(out PlayerSpawnSystem go))
    {
      NetworkServer.Destroy(go.gameObject);
    }

    // If all players have left, choose host to be the winner
    if (players.Count == 1 && bots.Length <= 0)
      winner = players.ElementAt(0).Key;
    // If gamemode is elimination, choose last player alive
    else if (gameMode == 0)
    {
      GameObject[] alivePlayers = GameObject.FindGameObjectsWithTag("Player").Where(x => !x.GetComponent<DamageController>().dead).ToArray();

      if (alivePlayers.Length <= 0) winner = null;
      else winner = alivePlayers[0].GetComponent<NetworkIdentity>().connectionToClient;

      if (winner == null)
        botWinner = GameObject.FindGameObjectsWithTag("Bot").Where(x => !x.GetComponent<BotPathfinding>().dead).ToArray()[0];
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

      // PlayerVars winnerVars = winner.identity.GetComponent<PlayerVars>();

      // winnerVars.lockWeapon = true;
      // winnerVars.lockMovement = true;
      // winnerVars.lockShooting = true;
    }

    GameObject winnerUi = Instantiate(WinnerPanelPrefab);
    NetworkServer.Spawn(winnerUi);

    string winnerName = botWinner != null ? "BOT" : players[winner].displayName;
    int winnerIdx = botWinner != null ? -1 : System.Array.IndexOf(NetworkServer.connections.Values.ToArray(), winner);

    WinnerPanelPrefab.GetComponent<WinnerUI>().winnerText.text = $"{winnerName} won the round";

    if (botWinner != null)
      WinnerPanelPrefab.GetComponent<WinnerUI>().playerImage.color = new Color(0.3936009f, 0.5186465f, 0.5754717f);

    if (players.Count <= 1) return;

    for (int i = 0; i < NetworkServer.connections.Count; i++)
    {
      FindObjectOfType<Server>().SetWinnerText(NetworkServer.connections.ElementAt(i).Value, $"{winnerName} won the round!", winnerIdx);
    }
  }

  [Server]
  public void StartGame()
  {
    winner = null;
    botWinner = null;

    int sceneCount = SceneManager.sceneCountInBuildSettings;
    List<string> _scenes = new();
    if (gameMode == GameMode.Elimination)
    {
      for (int i = 0; i < sceneCount; i++)
      {
        string path = SceneUtility.GetScenePathByBuildIndex(i);
        string dir = Path.GetDirectoryName(path);
        string sName = Path.GetFileNameWithoutExtension(path);

        if (enableBots && !_BotSupport.Contains(path)) continue;

        if (dir.EndsWith("4Players") && players.Count <= 4) _scenes.Add(sName);
        else if (dir.EndsWith("16Players") && players.Count <= 16) _scenes.Add(sName);
      }

      rounds = Mathf.Clamp(rounds, 1, playableScenesCount);
      while (_scenes.Count > rounds)
      {
        _scenes.RemoveAt(Random.Range(0, _scenes.Count));
      }
    }
    else
    {
      if (chosenMap == 0)
      {
        if (!enableBots)
        {
          int chosenMapIdx = Random.Range(menuScenesCount, sceneCount);
          _scenes.Add(Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(chosenMapIdx)));
        }
        else
        {
          _scenes.Add(Path.GetFileNameWithoutExtension(_BotSupport[Random.Range(0, _BotSupport.Length)]));
        }
      }
      else _scenes.Add(Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(chosenMap - 1 + menuScenesCount)));
    }

    queuedScenes = _scenes.ToArray();

    gameStarted = true;

    FirebaseManager.UpdateLobby(NetworkServer.connections.Count, gameMode.ToString(), privacyType.ToString().ToLower(), gameStarted);

    CycleMap();
  }

  [Server]
  public void CycleMap()
  {
    deadPlayers = 0;
    winner = null;
    botWinner = null;

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

  public override void OnStopServer()
  {
    players.Clear();
    base.OnStopServer();
  }

  public override void OnStopClient()
  {
    players.Clear();
    base.OnStopClient();
  }

  public enum GameMode
  {
    Elimination = 0,
    Deathmatch = 1,
  }

  public enum PrivacyType
  {
    Public = 0,
    Private = 1,
  }
}