using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Bullet : NetworkBehaviour
{
  [HideInInspector]
  [SyncVar]
  public float damage;

  [HideInInspector]
  public NetworkConnection owner;

  [HideInInspector]
  [SyncVar]
  public bool passThrough = false;

  [HideInInspector]
  [SyncVar]
  public int passThroughAmount = 0;

  int passedThrough = 0;

  [ServerCallback]
  void Start() => Server_DisableCollisionWith(owner.identity.gameObject);

  private void OnCollisionEnter2D(Collision2D other)
  {
    if (other.gameObject.tag != "Player")
    {
      NetworkServer.Destroy(this.gameObject);
      return;
    }

    if (other.gameObject == owner.identity.gameObject) return;

    DealDamage(other.gameObject);

    if (!passThrough) NetworkServer.Destroy(this.gameObject);
    else
    {
      if (passedThrough >= passThroughAmount) NetworkServer.Destroy(this.gameObject);
      Server_DisableCollisionWith(other.gameObject);
      passedThrough++;
    }
  }

  [Command(requiresAuthority = false)]
  void Server_DisableCollisionWith(GameObject other) => DisableCollisionWith(other);

  [ClientRpc]
  void DisableCollisionWith(GameObject other) =>
    Physics2D.IgnoreCollision(GetComponent<Collider2D>(), other.GetComponent<Collider2D>());

  [Command(requiresAuthority = false)]
  void DealDamage(GameObject victim)
  {
    victim.GetComponent<PlayerBehavior>().TakeDamage(damage, owner.identity.gameObject);
  }
}
