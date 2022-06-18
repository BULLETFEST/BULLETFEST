using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Gravestone : NetworkBehaviour
{
  public GameObject dirt;
  public ParticleSystem particleSystem;

  [SyncVar]
  bool played = false;

  [Command(requiresAuthority = false)]
  public void Collided()
  {
    if (played) return;

    played = true;
    Rpc_Collided();
  }

  [ClientRpc]
  void Rpc_Collided()
  {
    dirt.SetActive(true);
    particleSystem.Play();
  }
}
