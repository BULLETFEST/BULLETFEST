using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;

public class Server : NetworkBehaviour
{

  public override void OnStartAuthority()
  {
    base.OnStartAuthority();
    if (!isServer)
    {
      // Destroy(GetComponent<PlayerVars>().publicCanvas.gameObject);
      Destroy(GetComponent<Server>());
    }
  }

  public override void OnStartClient()
  {
    base.OnStartClient();

    // Server_InitializePlayer(connectionToClient);
  }

  // [Command(requiresAuthority = false)]
  // void Server_InitializePlayer(NetworkConnectionToClient conn)
  // {
  //   // Sync Player names
  //   List<GameObject> players = GameObject.FindGameObjectsWithTag("Player").ToList();
  //   players.Add(this.gameObject);
  //   string[] names = new string[players.Count];

  //   foreach (GameObject player in players)
  //   {
  //     names[players.IndexOf(player)] = player.GetComponent<PlayerVars>().uiName.text;
  //   }

  //   InitializePlayer(conn, players.ToArray(), names);

  //   // conn.identity.gameObject.GetComponent<NewPB>().playerVars = conn.identity.gameObject.GetComponent<PlayerVars>();
  // }

  // [TargetRpc]
  // void InitializePlayer(NetworkConnection conn, GameObject[] players, string[] names)
  // {
  //   foreach (GameObject player in players)
  //   {
  //     player.GetComponent<PlayerVars>().uiName.text = names[System.Array.IndexOf(players, player)];
  //   }
  // }

  [TargetRpc]
  public void SetWinnerText(NetworkConnection conn, string text)
  {
    GameObject.FindObjectOfType<WinnerUI>().winnerText.text = text;
  }

  // [Command(requiresAuthority = false)]
  // public void Cmd_SpawnWinnerCanvas()
  // {
  //   MyNetworkManager nm = GameObject.FindObjectOfType<MyNetworkManager>();

  //   Rpc_SpawnWinnerCanvas($"{nm.players[nm.winner].displayName} won the round!", nm.winnerUI);
  // }

  // [ClientRpc]
  // void Rpc_SpawnWinnerCanvas(string winnerText, GameObject winnerPrefab)
  // {

  //   GameObject winnerUI = Instantiate(winnerPrefab);

  //   winnerUI.GetComponent<WinnerUI>().winnerText.text = winnerText;
  // }
}
