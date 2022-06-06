using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;

public class Server : NetworkBehaviour
{

  void Start()
  {
    if (!isServer) Destroy(gameObject.GetComponent<Server>());
  }

  public override void OnStartClient()
  {
    base.OnStartClient();

    Server_InitializePlayer(connectionToClient);
  }

  [Command(requiresAuthority = false)]
  void Server_InitializePlayer(NetworkConnectionToClient conn)
  {
    // Sync Player names
    List<GameObject> players = GameObject.FindGameObjectsWithTag("Player").ToList();
    players.Add(this.gameObject);
    string[] names = new string[players.Count];

    foreach (GameObject player in players)
    {
      names[players.IndexOf(player)] = player.GetComponent<PlayerObjects>().uiName.text;
    }

    InitializePlayer(conn, players.ToArray(), names);
  }

  [TargetRpc]
  void InitializePlayer(NetworkConnection conn, GameObject[] players, string[] names)
  {
    foreach (GameObject player in players)
    {
      player.GetComponent<PlayerObjects>().uiName.text = names[System.Array.IndexOf(players, player)];
    }
  }
}
