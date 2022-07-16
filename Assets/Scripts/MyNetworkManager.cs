using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using UnityEngine.SceneManagement;
using System.IO;

public class MyNetworkManager : NetworkManager
{
  [Scene][SerializeField] private string menu = string.Empty;

  string[] playableScenes;

  private GameObject playerCard;
  private GameObject playerCards;
  private GameObject LeaderboardItem;

  [SerializeField]
  private GameObject playerSpawnSystem;

  public GameObject winnerUI;

  public Dictionary<NetworkConnectionToClient, PlayerData> players { get; } = new Dictionary<NetworkConnectionToClient, PlayerData>();

  public bool gameStarted = false;

  public NetworkConnectionToClient winner;

  public System.Action PlayerUpdate;

  public bool isHost = false;

  public string RoomCode;

  public GameMode gameMode = 0;

  public override void Start()
  {
    base.Start();

    foreach (GameObject prefab in Resources.LoadAll<GameObject>("Spawnable"))
    {
      NetworkClient.RegisterPrefab(prefab);
      if (prefab.name == "PlayerCard") playerCard = prefab;
    }

    LeaderboardItem = (GameObject)Resources.Load("SpawnableNoNetId/LeaderboardItem");
    NetworkClient.RegisterPrefab(LeaderboardItem);

    NetworkClient.RegisterHandler<Message.ServerMessge>(OnServerMessage);
  }

  void OnServerMessage(Message.ServerMessge msg)
  {
    Message.DisplayMessage(msg.titleText, msg.contentText, msg.alignment);
    if (msg.disconnect) Disconnect();
  }

  public override void OnStartServer()
  {
    base.OnStartServer();
    isHost = true;
  }

  [Server]
  public override void OnServerConnect(NetworkConnectionToClient conn)
  {
    base.OnServerConnect(conn);

    if (NetworkServer.connections.Count >= 4)
    {
      conn.Send(new Message.ServerMessge
      {
        titleText = "Disconnected",
        contentText = "Game is full",
        _alignment = 2,
        disconnect = true
      });
      // conn.Disconnect();
      return;
    }

    if (SceneManager.GetActiveScene().path != menu)
    {
      conn.Send(new Message.ServerMessge
      {
        titleText = "Disconnected",
        contentText = "Game has started",
        _alignment = 2,
        disconnect = true
      });
      // conn.Disconnect();
    }
  }

  public override void OnServerSceneChanged(string sceneName)
  {
    base.OnServerSceneChanged(sceneName);

    if (SceneManager.GetActiveScene().path == menu)
      playerCards = GameObject.FindGameObjectWithTag("PlayerCards");
    else if (SceneManager.GetActiveScene().name != "End")
    {
      if (!FindObjectOfType<PlayerSpawnSystem>())
      {
        GameObject go = Instantiate(playerSpawnSystem);
        if (gameMode == GameMode.Deathmatch)
        {
          go.GetComponent<PlayerSpawnSystem>().timeStamp = System.DateTime.UtcNow.AddMinutes(1);
        }
        NetworkServer.Spawn(go);


      }
    }
    else
    {
      if (!FindObjectOfType<EndScreenUI>())
      {
        GameObject go = new GameObject("LeaderboardItemSpawner", typeof(EndScreenUI));
        GameObject LeaderboardItemSpawner = Instantiate(go);
        Destroy(go);
        NetworkServer.Spawn(LeaderboardItemSpawner);
      }
    }
  }

  public override void OnServerAddPlayer(NetworkConnectionToClient conn)
  {
    if (SceneManager.GetActiveScene().path == menu)
    {
      GameObject player = Instantiate(playerCard, Vector3.zero, Quaternion.Euler(0, 0, 0), playerCards.transform);
      player.GetComponent<PlayerCard>().DisplayNameUI.text = "Loading...";
      if (conn != NetworkServer.localConnection) player.GetComponent<PlayerCard>().kickBtn.gameObject.SetActive(true);
      NetworkServer.AddPlayerForConnection(conn, player);
    }
  }

  public override void OnServerDisconnect(NetworkConnectionToClient conn)
  {
    base.OnServerDisconnect(conn);
    if (players.ContainsKey(conn))
    {
      players.Remove(conn);

      PlayerUpdate?.Invoke();

      if (gameStarted)
      {
        if (players.Count == 1) playableScenes = new string[0];
        deadPlayers--;
        OnPlayerDie(conn);
      }
    }
  }


  int deadPlayers = 0;

  [Server]
  public void OnPlayerDie(NetworkConnectionToClient conn)
  {
    if (gameMode == GameMode.Rounds)
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
    if (gameMode == 0)
      winner = GameObject.FindGameObjectsWithTag("Player").Where(x => x.activeInHierarchy).ToArray()[0].GetComponent<NetworkIdentity>().connectionToClient;
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

    GameObject winnerUi = Instantiate(winnerUI);
    NetworkServer.Spawn(winnerUi);

    foreach (NetworkConnectionToClient conn in NetworkServer.connections.Values)
    {
      FindObjectOfType<Server>().SetWinnerText(conn, $"{players[winner].displayName} won the round!");
    }
  }

  [Server]
  public void StartGame()
  {
    int sceneCount = SceneManager.sceneCountInBuildSettings;
    List<string> _scenes = new();
    for (int i = 0; i < sceneCount; i++)
    {
      string path = SceneUtility.GetScenePathByBuildIndex(i);
      string dir = Path.GetDirectoryName(path);
      string sName = Path.GetFileNameWithoutExtension(path);

      if (dir.EndsWith("4Players") && players.Count <= 4) _scenes.Add(sName);
      else if (dir.EndsWith("16Players") && players.Count <= 16) _scenes.Add(sName);
    }

    playableScenes = _scenes.ToArray();

    gameStarted = true;

    CycleMap();
  }

  [Server]
  public void CycleMap()
  {

    deadPlayers = 0;

    if (playableScenes.Length == 0 || gameMode == GameMode.Deathmatch)
    {
      gameStarted = false;
      ServerChangeScene("End");
      return;
    }

    int chosenScene = Random.Range(0, playableScenes.Length);

    string chosenSceneId = playableScenes[chosenScene];

    List<string> _scenes = playableScenes.ToList();

    _scenes.Remove(chosenSceneId);

    playableScenes = _scenes.ToArray();

    ServerChangeScene(chosenSceneId);
  }

  public override void OnStopServer()
  {
    players.Clear();
    base.OnStopServer();
  }

  public void Disconnect()
  {
    if (NetworkServer.active && NetworkClient.isConnected)
    {
      StopHost();
    }
    else if (NetworkClient.isConnected)
    {
      StopClient();
    }
  }



  public enum GameMode
  {
    Rounds = 0,
    Deathmatch = 1,
  }
}