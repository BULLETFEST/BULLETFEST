using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerNetworking : NetworkBehaviour
{
  PlayerObjects playerObjects;

  void Start()
  {
    if (isLocalPlayer)
      gameObject.layer = 30;

    playerObjects = GetComponent<PlayerObjects>();

    Server_InitializePlayer();
  }

  [Command]
  void Server_InitializePlayer()
  {
    string name = PlayerPrefs.GetString("name");
    playerObjects.uiName.text = name;

    ClientRpc_InitializePlayer(name);
  }

  [ClientRpc]
  void ClientRpc_InitializePlayer(string name)
  {
    playerObjects.uiName.text = name;
  }

}
