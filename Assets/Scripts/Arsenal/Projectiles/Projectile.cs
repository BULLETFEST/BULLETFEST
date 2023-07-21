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

  [SyncVar]
  [SerializeField]
  protected bool passThrough = false;

  [SyncVar]
  [SerializeField]
  protected int passThroughAmount = 0;

  int passedThrough = 0;

  [SerializeField]
  bool destroySelf = true;

  [DrawIf("destroySelf", true)]
  [SerializeField]
  float destroySelfTime = 5f;

  [SerializeField]
  protected bool modifyStartingRotation;

  [DrawIf("modifyStartingRotation", true)]
  [SerializeField]
  protected Vector3 startingRotation;

  [ServerCallback]
  protected virtual void Start()
  {
    Server_DisableCollisionWith(owner);
    if (destroySelf)
    {
      StartCoroutine(DestroySelf());
    }

    if (modifyStartingRotation)
    {
      transform.rotation = Quaternion.Euler(startingRotation);
    }
  }

  protected virtual IEnumerator DestroySelf()
  {
    yield return new WaitForSeconds(destroySelfTime);
    NetworkServer.Destroy(gameObject);
  }

  [ServerCallback]
  protected virtual void OnCollisionEnter2D(Collision2D other)
  {
    if (!other.gameObject.GetComponent<DamageController>())
    {
      NetworkServer.Destroy(gameObject);
      return;
    }

    DealDamage(other.gameObject);

    if (!passThrough)
    {
      NetworkServer.Destroy(gameObject);
    }
    else
    {
      if (passedThrough >= passThroughAmount)
      {
        NetworkServer.Destroy(gameObject);
      }

      Server_DisableCollisionWith(other.gameObject);
      passedThrough++;
    }
  }

  [Command(requiresAuthority = false)]
  protected void Server_DisableCollisionWith(GameObject other)
  {
    DisableCollisionWith(other);
  }

  [ClientRpc]
  protected void DisableCollisionWith(GameObject other)
  {
    Physics2D.IgnoreCollision(GetComponent<Collider2D>(), other.GetComponent<Collider2D>());
  }

  [Command(requiresAuthority = false)]
  protected void DealDamage(GameObject victim)
  {
    victim.GetComponent<DamageController>().TakeDamage(damage, owner);
  }
}
