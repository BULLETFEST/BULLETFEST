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

  public void Collided()
  {
    if (played) return;

    dirt.SetActive(true);
    particleSystem.Play();
    played = true;
  }
}
