using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class WeaponBehavior : MonoBehaviour
{
  public WeaponClass weapon;
  public PlayerUI uiController;
  public PlayerVars playerVars;

  public WeaponClass[] arsenal;
  private Coroutine reloadRoutine;

  [HideInInspector]
  public List<Explosive> awaitingDetonation = new();

  private void Start()
  {
    // weapon.bulletsInMag = weapon.magazineSize;
    // weapon.fireTimeout = 0;

    // uiController.UpdateAmmoText(weapon.bulletsInMag, weapon.magazineSize);
    // uiController.UpdateWeaponUI(weapon);
  }

  public void Fire(string weaponId, NetworkConnection shooter)
  {
    WeaponClass equippedWeapon = arsenal.Where(w => w.ID == weaponId).ToArray()[0];

    // Get the weapon prefix ID (stg, hdg, etc)
    string weaponType = equippedWeapon.ID.Split("_")[0].ToLower();

    switch (weaponType)
    {
      case "hdg":
      case "smg":
      case "lmg":
      case "snr":
      case "thrw":
        Fire_Regular(shooter);
        break;
      case "stg":
        Fire_Pellets(shooter);
        break;
      case "rpg":
        throw new System.NotImplementedException();
      case "mel":
        StartCoroutine(Fire_Melee(shooter));
        break;
      default:
        break;
    }
  }

  // Add recoil to the user
  public void AddForce(GameObject target)
  {
    if (weapon?.shotPushback == 0)
    {
      return;
    }

    PlayerVars shooterVars = target.GetComponent<PlayerVars>();
    shooterVars.rb.velocity = new Vector2(0, shooterVars.rb.velocity.y);
    shooterVars.lockMovement = true;
    Vector2 vel = shooterVars.weaponBehavior.weapon.shotPushback * -shooterVars.weaponBehavior.transform.right;
    shooterVars.rb.AddForce(new Vector2(vel.x * 1.75f, vel.y / 1.55f), ForceMode2D.Impulse);
    StartCoroutine(UnlockMovement(shooterVars.weaponBehavior.weapon.movementUnlockTime, shooterVars));
  }

  public IEnumerator Fire_Melee(NetworkConnection shooter)
  {
    yield return new WaitForSeconds(weapon.animationShotDamageDelay);
    RaycastHit2D hit = Physics2D.Raycast(weapon.projectileSpawnPoint.transform.position, transform.right, weapon.meleeRange);
    if (hit.collider == null)
    {
      yield break;
    }

    Debug.DrawLine(weapon.projectileSpawnPoint.transform.position, hit.point, Color.white, 2f);

    if (!hit.collider.GetComponent<DamageController>())
    {
      yield break;
    }

    hit.collider.GetComponent<DamageController>().TakeDamage(weapon.damage, shooter.identity.gameObject);
  }

  public void Fire_Regular(NetworkConnection shooter)
  {
    GameObject spawnedBullet = Instantiate(weapon.projectilePrefab, weapon.projectileSpawnPoint.transform.position, Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z + Random.Range(weapon.inaccuracyRange[0], weapon.inaccuracyRange[1])));

    Physics2D.IgnoreCollision(spawnedBullet.GetComponent<Collider2D>(), shooter.identity.gameObject.GetComponent<Collider2D>());

    spawnedBullet.GetComponent<Rigidbody2D>().velocity = weapon.projectileVelocity * spawnedBullet.transform.right;
    spawnedBullet.GetComponent<Rigidbody2D>().AddTorque(weapon.projectileTorque);

    spawnedBullet.GetComponent<Projectile>().owner = shooter.identity.gameObject;
    spawnedBullet.GetComponent<Projectile>().damage = weapon.damage;

    if (spawnedBullet.GetComponent<Explosive>())
    {
      awaitingDetonation.Add(spawnedBullet.GetComponent<Explosive>());
    }

    NetworkServer.Spawn(spawnedBullet, shooter);
  }

  public void Fire_Pellets(NetworkConnection shooter)
  {
    for (int i = 0; i < weapon.pelletCount; i++)
    {
      Fire_Regular(shooter);
    }
  }

  private IEnumerator UnlockMovement(float time, PlayerVars shooterVars)
  {
    yield return new WaitForSecondsRealtime(time);
    shooterVars.lockMovement = false;
  }

  public void SwitchWeapon(string weaponID)
  {
    if (weapon != null)
    {
      Destroy(weapon.gameObject);
    }

    if (playerVars.graphics.sprites.Count > 2)
    {
      playerVars.graphics.sprites.RemoveAt(2);
    }

    if (weaponID != null)
    {
      GameObject chosenWeapon = arsenal.Where(w => w.ID == weaponID).ToArray()[0].gameObject;
      GameObject newWeapon = Instantiate(chosenWeapon, transform.position, transform.rotation, transform);
      transform.localPosition = chosenWeapon.transform.position;
      transform.localRotation = chosenWeapon.transform.rotation;

      weapon = newWeapon.GetComponent<WeaponClass>();
      weapon.bulletsInMag = weapon.magazineSize;
      weapon.fireTimeout = 0;

      uiController.UpdateAmmoText(weapon.magazineSize);
      playerVars.graphics.sprites.Add(newWeapon.GetComponentInChildren<SpriteRenderer>());
      playerVars.graphics.sprites[2].enabled = true;

      if (weapon.animateOnShot)
      {
        playerVars.weaponAnimator.animator = weapon.GetComponent<Animator>();
      }
    }
    else
    {
      // weapon = null;
      if (playerVars.graphics.sprites.Count >= 3)
      {
        playerVars.graphics.sprites[2].enabled = false;
      }

      uiController.UpdateAmmoText(-1);
    }
  }
}
