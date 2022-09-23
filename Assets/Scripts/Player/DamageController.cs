using System;
using Mirror;
using UnityEngine;

public class DamageController : NetworkBehaviour
{
  [SyncVar(hook = nameof(OnDamageTaken))]
  public float health, maxHealth;

  public Action<GameObject> onTakeDamage;
  public Action<GameObject> onDeath;

  [SyncVar]
  public bool dead = false;

  GameObject damageDealer;

  void Start()
  {
    health = maxHealth;
  }

  [Command(requiresAuthority = false)]
  public void TakeDamage(float damage, GameObject owner)
  {
    if (dead) return;

    damageDealer = owner;

    health -= damage;

    onTakeDamage?.Invoke(damageDealer);
  }

  void OnDamageTaken(float oldHealth, float newHealth)
  {
    if (health <= 0) Die();
  }

  [Command(requiresAuthority = false)]
  public void Die()
  {
    if (health > 0 || dead) return;

    dead = true;

    onDeath?.Invoke(damageDealer ?? gameObject);
  }

}
