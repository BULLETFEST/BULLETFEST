using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class WeaponItem : NetworkBehaviour
{
  public string WeaponID;

  private void OnCollisionEnter2D(Collision2D other)
  {
    if (other.collider.tag == "Spike") NetworkServer.Destroy(gameObject);
  }

  private void FixedUpdate()
  {
    if (transform.position.y < -15) NetworkServer.Destroy(gameObject);
  }
}
