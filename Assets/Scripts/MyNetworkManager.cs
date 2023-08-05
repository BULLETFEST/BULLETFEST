using System.Collections;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MyNetworkManager : NetworkManager
{
  public static MyNetworkManager Instance { get; private set; }

  [HideInInspector]
  public ChatManager Chat;

  public System.Action PlayerUpdate;
  public System.Action<NetworkConnectionToClient> PlayerConnect, PlayerDisconnect, PlayerSpawn;

  [HideInInspector]
  public bool isHost { get; private set; } = false;

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

  public override void Awake()
  {
    base.Awake();

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
  }

  public override void OnServerAddPlayer(NetworkConnectionToClient conn)
  {
    PlayerSpawn?.Invoke(conn);
  }

  public override void Start()
  {
    base.Start();

    if (Instance == null)
    {
      Instance = this;
    }
    else if (Instance != this)
    {
      Destroy(gameObject);
    }

    foreach (GameObject prefab in Resources.LoadAll<GameObject>("Spawnable"))
    {
      NetworkClient.RegisterPrefab(prefab);
    }

    NetworkClient.RegisterHandler<Message.ServerMessge>(OnServerMessage);
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

    isHost = true;
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

    if (NetworkServer.connections.Count > GameManager.settings.lobbySize)
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

      if (GameManager.Instance.state == GameManager.GameState.Started)
      {
        if (GameManager.Instance.players.Count == 1)
        {
          GameManager.Instance.EndGame();
        }
        GameManager.Instance.OnPlayerDie(conn.identity.gameObject.GetComponent<DamageController>());
      }
    }

    PlayerDisconnect?.Invoke(conn);
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