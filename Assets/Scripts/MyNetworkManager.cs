using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using System.Collections;
using EpicTransport;


public class MyNetworkManager : NetworkManager
{
  public static MyNetworkManager instance;

  string[] queuedScenes;

  [SerializeField]
  private GameObject PlayerSpawnSystemPrefab,
                     WinnerPanelPrefab;

  public Dictionary<NetworkConnectionToClient, PlayerData> players { get; } = new Dictionary<NetworkConnectionToClient, PlayerData>();

  public NetworkConnectionToClient[] sortedPlayerList = new NetworkConnectionToClient[4];

  public NetworkConnectionToClient winner;

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
    StartCoroutine(keepAlive());
  }

  IEnumerator keepAlive()
  {
    try
    {
      Firebase.KeepAlive();
    }
    catch { }

    yield return new WaitForSecondsRealtime(60 * 2.5f);

    StartCoroutine(keepAlive());
  }

  [Server]
  public override void OnServerConnect(NetworkConnectionToClient conn)
  {
    base.OnServerConnect(conn);

    if (NetworkServer.connections.Count >= 5)
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

    Firebase.UpdateLobby(NetworkServer.connections.Count, gameMode.ToString(), privacyType.ToString().ToLower());
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
  }

  public override void OnServerDisconnect(NetworkConnectionToClient conn)
  {
    if (SceneManager.GetActiveScene().name != "End") base.OnServerDisconnect(conn);

    if (players.ContainsKey(conn))
    {
      players.Remove(conn);

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
    if (NetworkServer.connections.Count == 1) AnnounceWinner();
    else if (gameMode == GameMode.Rounds)
    {
      deadPlayers++;
      if (deadPlayers == NetworkServer.connections.Count - 1)
      {
        AnnounceWinner();
      }
    }
    else
    {
      StartCoroutine(FindObjectOfType<PlayerSpawnSystem>().Cmd_RespawnPlayer(conn));
    }
  }

  [Server]
  public void AnnounceWinner()
  {
    if (Utilities.FindWithType<PlayerSpawnSystem>(out PlayerSpawnSystem go))
    {
      NetworkServer.Destroy(go.gameObject);
    }

    // If all players have left, choose host to be the winner
    if (players.Count == 1)
      winner = players.ElementAt(0).Key;
    // If gamemode is elimination, choose last player alive
    else if (gameMode == 0)
      winner = GameObject.FindGameObjectsWithTag("Player").Where(x => !x.GetComponent<PlayerBehavior>().dead).ToArray()[0].GetComponent<NetworkIdentity>().connectionToClient;
    // If gamemode is deathmatch, choose player with most kills
    else
    {
      // https://stackoverflow.com/a/1332/11420492
      //winner = (from entry in players orderby entry.Value descending select entry).First().Key;

      // https://stackoverflow.com/a/4157151/11420492
      winner = players.OrderBy(x => x.Value.kills).ToList().Last().Key;
    }

    players[winner].wins++;

    PlayerVars winnerVars = winner.identity.GetComponent<PlayerVars>();

    winnerVars.lockWeapon = true;
    winnerVars.lockMovement = true;
    winnerVars.lockShooting = true;

    GameObject winnerUi = Instantiate(WinnerPanelPrefab);
    NetworkServer.Spawn(winnerUi);

    WinnerPanelPrefab.GetComponent<WinnerUI>().winnerText.text = $"{players[winner].displayName} won the round";

    if (players.Count <= 1) return;

    for (int i = 0; i < NetworkServer.connections.Count; i++)
    {
      FindObjectOfType<Server>().SetWinnerText(NetworkServer.connections.ElementAt(i).Value, $"{players[winner].displayName} won the round!", System.Array.IndexOf(NetworkServer.connections.Values.ToArray(), winner));
    }
  }

  [Server]
  public void StartGame()
  {
    winner = null;

    int sceneCount = SceneManager.sceneCountInBuildSettings;
    List<string> _scenes = new();
    if (gameMode == GameMode.Rounds)
    {
      for (int i = 0; i < sceneCount; i++)
      {
        string path = SceneUtility.GetScenePathByBuildIndex(i);
        string dir = Path.GetDirectoryName(path);
        string sName = Path.GetFileNameWithoutExtension(path);

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
        int chosenMapIdx = Random.Range(menuScenesCount, sceneCount);
        _scenes.Add(Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(chosenMapIdx)));
      }
      else _scenes.Add(Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(chosenMap - 1 + menuScenesCount)));
    }

    queuedScenes = _scenes.ToArray();

    gameStarted = true;

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
    Rounds = 0,
    Deathmatch = 1,
  }

  public enum PrivacyType
  {
    Public = 0,
    Private = 1,
  }
}