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

  public List<PlayerData> players { get; } = new List<PlayerData>();

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
    base.OnServerSceneChanged(sceneName);

    playerCards = GameObject.FindGameObjectWithTag("PlayerCards");
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
    players.Remove(players.Where(x => x.netId == conn.connectionId).ToArray()[0]);
    base.OnServerDisconnect(conn);
  }

  public override void ServerChangeScene(string newSceneName)
  {
    for (int i = players.Count - 1; i >= 0; i--)
    {
      var conn = players[i].netId;
      var gameplayerInstance = Instantiate(base.playerPrefab);
      // gameplayerInstance.SetDisplayName(RoomPlayers[i].DisplayName);

      NetworkServer.Destroy(NetworkServer.connections[conn].identity.gameObject);

      NetworkServer.ReplacePlayerForConnection(NetworkServer.connections[conn], gameplayerInstance.gameObject);
      // NetworkClient.Ready();
      NetworkServer.SetClientReady(NetworkServer.connections[conn]);
    }

    base.ServerChangeScene(newSceneName);
  }

  public override void OnStopServer()
  {
    players.Clear();
    base.OnStopServer();
  }
}
