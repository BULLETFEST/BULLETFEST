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

  private void OnCollisionEnter2D(Collision2D other)
  {
    if (played) return;

    dirt.SetActive(true);
    particleSystem.Play();
    played = true;
  }
}
