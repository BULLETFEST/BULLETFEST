using Mirror;
using UnityEngine;

public class BotBehavior : NetworkBehaviour
{
  private BotRefs botRefs;

  private void Start()
  {
    botRefs = GetComponent<BotRefs>();
  }

  private void Update()
  {
    if (!isServer)
    {
      return;
    }

    if (transform.position.y is <= (-15) or >= 50)
    {
      botRefs.damageController.TakeDamage(9999999, null);
    }
  }

  public void Fire(float playerPosX, float angle)
  {
    WeaponClass weapon = botRefs.weaponBehavior.weapon;

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

    botRefs.weaponBehavior.transform.localRotation = Quaternion.Euler(playerPosX < 0 ? 180 : 0, playerPosX < 0 ? 180 : 0, ((playerPosX < 0 ? -1 : 1) * angle) + Random.Range(-25f, 25f));

    weapon.bulletsInMag--;
    weapon.fireTimeout = (float)Time.time + (1f / weapon.fireRate);// * (weapon.firingMode == WeaponClass.FireMode.Single ? 1.65f : 1));

    Rpc_AddForce(gameObject, weapon.shootSound);
    botRefs.weaponBehavior.Fire(weapon.ID, gameObject);

    if (botRefs.weaponBehavior.awaitingDetonation.Count >= 3)
    {
      foreach (Explosive explosive in botRefs.weaponBehavior.awaitingDetonation)
      {
        explosive.Detonate();
      }

      botRefs.weaponBehavior.awaitingDetonation.Clear();
    }
  }

  [ClientRpc]
  private void Rpc_AddForce(GameObject target, string shootSound)
  {
    if (botRefs.weaponBehavior.weapon.animateOnShot)
    {
      botRefs.weaponAnimator.animator.Play("Fire");
    }

    botRefs.weaponBehavior.AddForce(target);
    if (shootSound != "")
    {
      FindObjectOfType<AudioSystem>().PlaySound(shootSound);
    }
  }

  public void SwitchWeapon(GameObject weapon)
  {
    if (weapon != null && !botRefs.lockMovement)
    {
      WeaponItem weaponItem = weapon.GetComponent<WeaponItem>();

      botRefs.weaponBehavior.SwitchWeapon(weaponItem.WeaponID);
      TargetRpc_SwitchWeapon(weaponItem.WeaponID);
      NetworkServer.Destroy(weapon);
    }
  }

  [ClientRpc]
  private void TargetRpc_SwitchWeapon(string WeaponID)
  {
    botRefs.weaponBehavior.SwitchWeapon(WeaponID);
  }
}
