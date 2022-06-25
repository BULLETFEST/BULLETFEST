using System.Collections;
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
  }

  public System.Action PlayerUpdate;

  public bool isHost = false;

  public override void OnStartServer()
  {
    base.OnStartServer();
    isHost = true;
  }

  [Server]
  public override void OnServerConnect(NetworkConnectionToClient conn)
  {
    base.OnServerConnect(conn);

    if (SceneManager.GetActiveScene().path != menu) conn.Disconnect();
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
        // GameObject go = new GameObject("PlayerSpawner", typeof(PlayerSpawnSystem));
        // Destroy(go);
        GameObject go = Instantiate(playerSpawnSystem);
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
      NetworkServer.AddPlayerForConnection(conn, player);
    }
  }

  public override void OnServerDisconnect(NetworkConnectionToClient conn)
  {
    players.Remove(conn);

    PlayerUpdate?.Invoke();

    if (gameStarted)
    {
      deadPlayers--;
      OnPlayerDie();
    }

    base.OnServerDisconnect(conn);


  }


  int deadPlayers = 0;

  [Server]
  public void OnPlayerDie()
  {
    deadPlayers++;
    if (deadPlayers == NetworkServer.connections.Count - 1)
    {
      winner = GameObject.FindGameObjectsWithTag("Player").Where(x => x.activeInHierarchy).ToArray()[0].GetComponent<NetworkIdentity>().connectionToClient;

      // FindObjectOfType<Server>().Cmd_SpawnWinnerCanvas();

      PlayerVars winnerVars = winner.identity.GetComponent<PlayerVars>();

      winnerVars.lockWeapon = true;
      winnerVars.lockMovement = true;
      winnerVars.lockShooting = true;

      GameObject winnerUi = Instantiate(winnerUI);
      // winnerUi.GetComponent<WinnerUI>().winnerText.text = $"{players[winner].displayName} won the round!";
      NetworkServer.Spawn(winnerUi);

      foreach (NetworkConnectionToClient conn in NetworkServer.connections.Values)
      {
        FindObjectOfType<Server>().SetWinnerText(conn, $"{players[winner].displayName} won the round!");
      }

      // if (playableScenes.Length == 0)
      // {
      //   gameStarted = false;
      //   deadPlayers = 0;
      //   ServerChangeScene("End");
      // }
      // else
      // {
      //   deadPlayers = 0;
      //   CycleMap();
      // }
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

    if (playableScenes.Length == 0)
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



  public override void ServerChangeScene(string newSceneName)
  {
    // for (int i = players.Count - 1; i >= 0; i--)
    // {
    //   var conn = players.Keys.ToArray()[i];
    //   var gameplayerInstance = Instantiate(base.playerPrefab);
    //   // gameplayerInstance.SetDisplayName(RoomPlayers[i].DisplayName);

    //   NetworkServer.Destroy(conn.identity.gameObject);

    //   NetworkServer.ReplacePlayerForConnection(conn, gameplayerInstance.gameObject);
    //   NetworkServer.SetClientReady(conn);
    // }

    base.ServerChangeScene(newSceneName);
    // NetworkServer.SetClientReady();

    // foreach (NetworkConnectionToClient conn in NetworkServer.connections.Values)
    // {
    //   NetworkServer.SetClientReady(conn);
    // }
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
    // stop client if client-only
    else if (NetworkClient.isConnected)
    {
      StopClient();
    }

    // SceneManager.SetActiveScene(SceneManager.GetSceneByName("MainMenu"));
    // stop server if server-only
    // else if (NetworkServer.active)
    // {
    //   if (GUILayout.Button("Stop Server"))
    //   {
    //     manager.StopServer();
    //   }
    // }
  }
}
