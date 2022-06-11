using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Gravestone : NetworkBehaviour
{
  public GameObject dirt;
  public ParticleSystem particleSystem;

  private void OnCollisionEnter2D(Collision2D other)
  {
    dirt.SetActive(true);
    particleSystem.Play();
  }
}
