using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using UnityEngine.SceneManagement;

public class MyNetworkManager : NetworkManager
{
  [Scene][SerializeField] private string menu = string.Empty;

  private GameObject playerCard;
  private GameObject playerCards;

  public Dictionary<NetworkConnectionToClient, string> players { get; } = new Dictionary<NetworkConnectionToClient, string>();

  public override void Start()
  {
    base.Start();

    foreach (GameObject prefab in Resources.LoadAll<GameObject>("Spawnable"))
    {
      NetworkClient.RegisterPrefab(prefab);
      if (prefab.name == "PlayerCard") playerCard = prefab;
    }
  }

  public bool isHost = false;

  public override void OnStartServer()
  {
    base.OnStartServer();
    isHost = true;

    // NetworkServer.RegisterHandler<Dictionary<NetworkConnectionToClient, string>>(AddPlayer);
  }

  [Server]
  public override void OnServerConnect(NetworkConnectionToClient conn)
  {
    base.OnServerConnect(conn);

    if (SceneManager.GetActiveScene().path != menu) conn.Disconnect();
  }

  public override void OnServerSceneChanged(string sceneName)
  {

    if (SceneManager.GetActiveScene().path == menu)
      playerCards = GameObject.FindGameObjectWithTag("PlayerCards");
    else
    {
      if (!FindObjectOfType<PlayerSpawnSystem>())
      {
        GameObject go = new GameObject("PlayerSpawner", typeof(PlayerSpawnSystem));
        GameObject playerSpawnSystem = Instantiate(go);
        Destroy(go);
        NetworkServer.Spawn(playerSpawnSystem);
      }
    }

    base.OnServerSceneChanged(sceneName);
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
    base.OnServerDisconnect(conn);
  }

  public override void ServerChangeScene(string newSceneName)
  {
    for (int i = players.Count - 1; i >= 0; i--)
    {
      var conn = players.Keys.ToArray()[i];
      var gameplayerInstance = Instantiate(base.playerPrefab);
      // gameplayerInstance.SetDisplayName(RoomPlayers[i].DisplayName);

      NetworkServer.Destroy(conn.identity.gameObject);

      NetworkServer.ReplacePlayerForConnection(conn, gameplayerInstance.gameObject);
      // NetworkClient.Ready();
      NetworkServer.SetClientReady(conn);
    }

    base.ServerChangeScene(newSceneName);
  }

  public override void OnStopServer()
  {
    players.Clear();
    base.OnStopServer();
  }
}
