using UnityEngine;
using Mirror;
using System;

public class Behavior : NetworkBehaviour
{
  protected ComponentRefs componentRefs;

  protected Action afterFire;

  private void Awake()
  {
    componentRefs = GetComponent<ComponentRefs>();
  }

  virtual protected void Update()
  {
    if ((transform.position.y is <= (-15) or >= 50) && !componentRefs.damageController.dead)
    {
      componentRefs.damageController.TakeDamage(99999, null);
    }
  }

  [Command]
  protected virtual void Cmd_Fire()
  {
    WeaponClass weapon = componentRefs.weapon;

    if (componentRefs.lockShooting)
    {
      return;
    }

    if (weapon == null)
    {
      return;
    }

    if (weapon.fireTimeout > Time.time)
    {
      return;
    }

    if (weapon.bulletsInMag <= 0 && !weapon.isMelee)
    {
      return;
    }

    if (!weapon.isMelee)
    {
      weapon.bulletsInMag--;
    }

    weapon.fireTimeout = (float)Time.time + (1f / weapon.fireRate);

    Rpc_AddForce(gameObject);
    componentRefs.weaponBehavior.Fire(weapon.uniqueID, connectionToClient.identity.gameObject);

    if (weapon.bulletsInMag <= 0 && weapon.deleteOnEmpty)
    {
      Rpc_SwitchWeapon(null);
      componentRefs.weaponBehavior.SwitchWeapon(null);
    }

    if (componentRefs.weapon.soundOnShoot)
    {
      FindFirstObjectByType<Server>().Rpc_PlaySoundAll(componentRefs.weapon.shootSound);
    }

    afterFire?.Invoke();
  }

  [ClientRpc]
  protected virtual void Rpc_AddForce(GameObject target)
  {
    if (componentRefs.weapon.animateOnShot)
    {
      componentRefs.weaponAnimator.animator.Play("Fire");
    }

    componentRefs.weaponBehavior.AddForce(target);
  }

  [Command]
  protected virtual void Cmd_SwitchWeapon(GameObject weapon)
  {
    if (weapon != null && !componentRefs.lockMovement)
    {
      Rpc_SwitchWeapon(weapon.GetComponent<WeaponItem>().WeaponID);
      componentRefs.weaponBehavior.SwitchWeapon(weapon.GetComponent<WeaponItem>().WeaponID);
      NetworkServer.Destroy(weapon);
    }
  }

  [ClientRpc]
  protected virtual void Rpc_SwitchWeapon(string WeaponID)
  {
    componentRefs.weaponBehavior.SwitchWeapon(WeaponID);
  }
}
