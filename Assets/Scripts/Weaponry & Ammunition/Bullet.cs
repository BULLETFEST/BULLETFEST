using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Bullet : NetworkBehaviour
{
  [HideInInspector]
  public float damage;

  [HideInInspector]
  public NetworkConnection owner;

  [HideInInspector]
  public bool passThrough = false;
  [HideInInspector]
  public int passThroughAmount = 0;

  int passedThrough = 0;

  private void OnCollisionEnter2D(Collision2D other)
  {
    if (other.gameObject.layer != 31)
    {
      NetworkServer.Destroy(this.gameObject);
      return;
    }

    other.gameObject.GetComponent<PlayerBehavior>().TakeDamage(damage, owner);

    if (!passThrough) NetworkServer.Destroy(this.gameObject);
    else
    {
      if (passedThrough >= passThroughAmount) NetworkServer.Destroy(this.gameObject);
      Physics2D.IgnoreCollision(GetComponent<Collider2D>(), other.collider);
      passedThrough++;
    }
  }
}
