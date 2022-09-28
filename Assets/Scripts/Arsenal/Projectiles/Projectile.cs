using System.Collections;
using Mirror;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
  [HideInInspector]
  [SyncVar]
  public float damage;

  [HideInInspector]
  public GameObject owner;

  [HideInInspector]
  [SyncVar]
  public bool passThrough = false;

  [HideInInspector]
  [SyncVar]
  public int passThroughAmount = 0;

  internal int passedThrough = 0;

  [ServerCallback]
  internal virtual void Start()
  {
    Server_DisableCollisionWith(owner);
    StartCoroutine(DestroySelf());
  }

  internal virtual IEnumerator DestroySelf()
  {
    yield return new WaitForSeconds(5f);
    NetworkServer.Destroy(gameObject);
  }

  [ServerCallback]
  internal virtual void OnCollisionEnter2D(Collision2D other)
  {
    if (!other.gameObject.GetComponent<DamageController>())
    {
      NetworkServer.Destroy(gameObject);
      return;
    }

    DealDamage(other.gameObject);

    if (!passThrough) NetworkServer.Destroy(gameObject);
    else
    {
      if (passedThrough >= passThroughAmount) NetworkServer.Destroy(gameObject);
      Server_DisableCollisionWith(other.gameObject);
      passedThrough++;
    }
  }

  [Command(requiresAuthority = false)]
  internal void Server_DisableCollisionWith(GameObject other) => DisableCollisionWith(other);

  [ClientRpc]
  internal void DisableCollisionWith(GameObject other) =>
    Physics2D.IgnoreCollision(GetComponent<Collider2D>(), other.GetComponent<Collider2D>());

  [Command(requiresAuthority = false)]
  internal void DealDamage(GameObject victim)
  {
    victim.GetComponent<DamageController>().TakeDamage(damage, owner);
  }
}
