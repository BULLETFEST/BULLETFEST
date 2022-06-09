using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public class WeaponBehavior : MonoBehaviour
{
  public WeaponClass weapon;
  public NetworkConnection owner;
  public PlayerUI uiController;
  public PlayerVars playerVars;

  public WeaponClass[] arsenal;

  Coroutine reloadRoutine;

  void Start()
  {
    weapon.fireRate /= 10;
    weapon.bulletsInMag = weapon.magazineSize;

    uiController.UpdateAmmoText(weapon);
    uiController.UpdateWeaponUI(weapon);
  }

  public void Fire_Regular()
  {
    GameObject spawnedBullet = Instantiate(weapon.bulletPrefab, weapon.bulletSpawnPoint.transform.position, Quaternion.Euler(0, 0, Random.Range(weapon.inaccuracyRange[0], weapon.inaccuracyRange[1])));

    spawnedBullet.GetComponent<Rigidbody2D>().velocity = new Vector2(weapon.bulletVelocity, 0) * transform.right;

    spawnedBullet.GetComponent<Bullet>().owner = owner;
    spawnedBullet.GetComponent<Bullet>().damage = weapon.damage;
  }

  public void Shoot(string weaponId)
  {
    WeaponClass equippedWeapon = arsenal.Where(w => w.ID == weaponId).ToArray()[0];

    // Get the weapon prefix ID (stg, hdg, etc)
    string weaponType = equippedWeapon.ID.Substring(0, 3).ToLower();

    // Single weapon firing
    if (equippedWeapon.firingMode == WeaponClass.FireMode.Single)
    {
      switch (weaponType)
      {
        case "hdg":
        case "stg":
          Fire_Regular();
          break;
        case "rpg":
          throw new System.NotImplementedException();
      }
    }
    // Auto/Burst firing
    else if (equippedWeapon.firingMode != WeaponClass.FireMode.Single)
    {
      switch (weaponType)
      {
        case "smg":
          Fire_Regular();
          break;
      }
    }

    // if (!isReloading && currWeapon.bulletsInMag < currWeapon.magSize && (Input.GetKeyDown(Utilities.StringToKeyCode(keybinds["reload"])) || Input.GetKeyDown(Utilities.StringToKeyCode(keybinds["reload2"]))))
    // {
    //   reloadRoutine = StartCoroutine(Reload(currWeapon));
    // }
  }

  public IEnumerator Reload()
  {
    playerVars.isReloading = true;

    uiController.uiReloadCircle.enabled = true;

    if (weapon.reloadType == WeaponClass.ReloadType.Magazine)
    {
      yield return new WaitForSeconds(weapon.reloadTime);
      weapon.bulletsInMag = weapon.magazineSize;

      uiController.UpdateAmmoText(weapon);
    }
    else
    {
      while (weapon.bulletsInMag < weapon.magazineSize)
      {
        yield return new WaitForSeconds(weapon.reloadTime);
        weapon.bulletsInMag++;

        uiController.UpdateAmmoText(weapon);
      }
    }

    uiController.uiReloadCircle.enabled = false;
    playerVars.isReloading = false;
  }
}
