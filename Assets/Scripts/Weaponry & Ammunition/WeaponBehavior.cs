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

  public GameObject shootingPoint;

  Coroutine reloadRoutine;

  void Start()
  {
    weapon.bulletsInMag = weapon.magazineSize;

    uiController.UpdateAmmoText(weapon.bulletsInMag, weapon.magazineSize);
    uiController.UpdateWeaponUI(weapon);
  }

  public void Fire_Regular()
  {

    GameObject spawnedBullet = Instantiate(weapon.bulletPrefab, shootingPoint.transform.position, Quaternion.Euler(0, playerVars.graphics.transform.rotation.y != 0 ? 180 : 0, Random.Range(weapon.inaccuracyRange[0], weapon.inaccuracyRange[1] + 1)));

    spawnedBullet.GetComponent<Rigidbody2D>().velocity = weapon.bulletVelocity * spawnedBullet.transform.right;

    spawnedBullet.GetComponent<Bullet>().owner = owner;
    spawnedBullet.GetComponent<Bullet>().damage = weapon.damage;
  }

  public void Shoot(string weaponId)
  {
    WeaponClass equippedWeapon = arsenal.Where(w => w.ID == weaponId).ToArray()[0];

    // Get the weapon prefix ID (stg, hdg, etc)
    string weaponType = equippedWeapon.ID.Substring(0, 3).ToLower();

    switch (weaponType)
    {
      case "hdg":
      case "stg":
      case "smg":
        Fire_Regular();
        break;
      case "rpg":
        throw new System.NotImplementedException();
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

      uiController.UpdateAmmoText(weapon.bulletsInMag, weapon.magazineSize);
    }
    else
    {
      while (weapon.bulletsInMag < weapon.magazineSize)
      {
        yield return new WaitForSeconds(weapon.reloadTime);
        weapon.bulletsInMag++;

        uiController.UpdateAmmoText(weapon.bulletsInMag, weapon.magazineSize);
      }
    }

    uiController.uiReloadCircle.enabled = false;
    playerVars.isReloading = false;
  }

  public void SwitchWeapon(string weaponID)
  {
    weapon = arsenal.Where(w => w.ID == weaponID).ToArray()[0];
    weapon.bulletsInMag = weapon.magazineSize;
    weapon.fireTimeout = 0;

    uiController.UpdateWeaponUI(weapon);
    uiController.UpdateAmmoText(weapon.bulletsInMag, weapon.magazineSize);

    shootingPoint.transform.localPosition = weapon.bulletSpawnPoint.localPosition;
  }
}
