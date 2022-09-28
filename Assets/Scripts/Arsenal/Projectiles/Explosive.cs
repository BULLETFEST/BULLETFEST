using Mirror;
using UnityEngine;

public class Explosive : Projectile
{
  [SyncVar]
  public bool detonateOnImpact;

  [SyncVar]
  [HideInInspector]
  public bool detonated;

  public float radius;

  internal override void Start()
  {
    Server_DisableCollisionWith(owner);
  }

  internal override void OnCollisionEnter2D(Collision2D other)
  {
    if (detonateOnImpact && !detonated) Detonate();
  }

  [Command]
  public void Detonate()
  {
    if (detonated) return;

    detonated = true;

    DamageController[] damageControllers = FindObjectsOfType<DamageController>();

    foreach (DamageController controller in damageControllers)
    {
      float dist = Utilities.CalculateDistance(transform.position, controller.transform.position);

      if (dist <= radius)
      {
        controller.TakeDamage(damage, owner);
      }
    }

    NetworkServer.Destroy(gameObject);
  }
}
