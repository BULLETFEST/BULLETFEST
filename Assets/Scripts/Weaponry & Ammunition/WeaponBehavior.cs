using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public class WeaponBehavior : MonoBehaviour
{
  public WeaponClass weapon;
  public PlayerUI uiController;
  public PlayerVars playerVars;

  public WeaponClass[] arsenal;

  Coroutine reloadRoutine;

  void Start()
  {
    // weapon.bulletsInMag = weapon.magazineSize;
    // weapon.fireTimeout = 0;

    // uiController.UpdateAmmoText(weapon.bulletsInMag, weapon.magazineSize);
    // uiController.UpdateWeaponUI(weapon);
  }

  public void Shoot(string weaponId, NetworkConnection shooter)
  {
    WeaponClass equippedWeapon = arsenal.Where(w => w.ID == weaponId).ToArray()[0];

    // Get the weapon prefix ID (stg, hdg, etc)
    string weaponType = equippedWeapon.ID.Substring(0, 3).ToLower();

    switch (weaponType)
    {
      case "hdg":
      case "smg":
      case "lmg":
        Fire_Regular(shooter);
        break;
      case "stg":
        Fire_Pellets(shooter);
        break;
      case "rpg":
        throw new System.NotImplementedException();
    }


  }

  // Add recoil to the user
  public void AddForce(GameObject target)
  {
    if (weapon.shotPushback == 0) return;
    PlayerVars shooterVars = target.GetComponent<PlayerVars>();
    shooterVars.rb.velocity = new Vector2(0, shooterVars.rb.velocity.y);
    shooterVars.lockMovement = true;
    Vector2 vel = shooterVars.weaponBehavior.weapon.shotPushback * -shooterVars.weaponBehavior.transform.right;
    shooterVars.rb.AddForce(new Vector2(vel.x, vel.y / 2.55f), ForceMode2D.Impulse);
    StartCoroutine(UnlockMovement(shooterVars.weaponBehavior.weapon.movementUnlockTime, shooterVars));
  }

  public void Fire_Regular(NetworkConnection shooter)
  {
    GameObject spawnedBullet = Instantiate(weapon.bulletPrefab, weapon.bulletSpawnPoint.transform.position, Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z + Random.Range(weapon.inaccuracyRange[0], weapon.inaccuracyRange[1])));

    Physics2D.IgnoreCollision(spawnedBullet.GetComponent<Collider2D>(), shooter.identity.gameObject.GetComponent<Collider2D>());

    spawnedBullet.GetComponent<Rigidbody2D>().velocity = weapon.bulletVelocity * spawnedBullet.transform.right;

    spawnedBullet.GetComponent<Bullet>().owner = shooter;
    spawnedBullet.GetComponent<Bullet>().damage = weapon.damage;

    // Destroy(spawnedBullet, 0.3f);
    NetworkServer.Spawn(spawnedBullet);
  }

  public void Fire_Pellets(NetworkConnection shooter)
  {
    for (int i = 0; i < weapon.pelletCount; i++)
    {
      Fire_Regular(shooter);
    }
  }

  IEnumerator UnlockMovement(float time, PlayerVars shooterVars)
  {
    yield return new WaitForSecondsRealtime(time);
    shooterVars.lockMovement = false;
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
    if (weapon != null) Destroy(weapon.gameObject);
    GameObject newWeapon = Instantiate(arsenal.Where(w => w.ID == weaponID).ToArray()[0].gameObject, transform.position, transform.rotation, transform);
    weapon = newWeapon.GetComponent<WeaponClass>();
    weapon.bulletsInMag = weapon.magazineSize;
    weapon.fireTimeout = 0;

    uiController.UpdateWeaponUI(weapon);
    uiController.UpdateAmmoText(weapon.bulletsInMag, weapon.magazineSize);

    playerVars.graphics.sprites.RemoveAt(1);
    playerVars.graphics.sprites.Add(newWeapon.GetComponentInChildren<SpriteRenderer>());

    if (playerVars.reloadRoutine != null) StopCoroutine(playerVars.reloadRoutine);

    uiController.uiReloadCircle.enabled = false;
    playerVars.isReloading = false;
  }
}
