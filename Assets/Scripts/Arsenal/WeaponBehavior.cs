using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class WeaponBehavior : MonoBehaviour
{
  public WeaponClass weapon { get; private set; }
  public ComponentRefs refs;

  public WeaponClass[] arsenal;

  [HideInInspector]
  public List<Explosive> awaitingDetonation = new();

  public void Fire(string weaponId, GameObject shooter)
  {
    WeaponClass equippedWeapon = arsenal.Where(w => w.uniqueID == weaponId).ToArray()[0];

    switch (equippedWeapon.weaponType)
    {
      case WeaponClass.WeaponType.HDG:
      case WeaponClass.WeaponType.SMG:
      case WeaponClass.WeaponType.LMG:
      case WeaponClass.WeaponType.SNR:
      case WeaponClass.WeaponType.THRW:
        Fire_Regular(shooter);
        break;
      case WeaponClass.WeaponType.STG:
        Fire_Pellets(shooter);
        break;
      case WeaponClass.WeaponType.RPG:
        throw new System.NotImplementedException();
      case WeaponClass.WeaponType.MLE:
        StartCoroutine(Fire_Melee(shooter));
        break;
      default:
        break;
    }
  }

  // Add recoil to the user
  public void AddForce(GameObject target)
  {
    if (weapon && weapon.shotPushback == 0)
    {
      return;
    }

    Rigidbody2D rb = target.GetComponent<Rigidbody2D>();


    rb.velocity = new Vector2(0, rb.velocity.y * 0.15f);
    target.GetComponent<ComponentRefs>().lockMovement = true;
    Vector2 vel = weapon.shotPushback * -gameObject.transform.right;
    rb.AddForce(new Vector2(vel.x * 1.85f, vel.y * 1.15f), ForceMode2D.Impulse);
    StartCoroutine(UnlockMovement(weapon.movementUnlockTime, target.GetComponent<ComponentRefs>()));
  }

  public IEnumerator Fire_Melee(GameObject shooter)
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

    hit.collider.GetComponent<DamageController>().TakeDamage(weapon.damage, shooter);
  }

  public void Fire_Regular(GameObject shooter)
  {
    GameObject spawnedBullet = Instantiate(weapon.projectilePrefab, weapon.projectileSpawnPoint.transform.position, Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z + Random.Range(weapon.inaccuracyRange[0], weapon.inaccuracyRange[1])));

    Physics2D.IgnoreCollision(spawnedBullet.GetComponent<Collider2D>(), shooter.GetComponent<Collider2D>());

    spawnedBullet.GetComponent<Rigidbody2D>().velocity = weapon.projectileVelocity * spawnedBullet.transform.right;
    spawnedBullet.GetComponent<Rigidbody2D>().AddTorque(weapon.projectileTorque);

    spawnedBullet.GetComponent<Projectile>().owner = shooter;
    spawnedBullet.GetComponent<Projectile>().damage = weapon.damage;

    if (spawnedBullet.GetComponent<Explosive>())
    {
      awaitingDetonation.Add(spawnedBullet.GetComponent<Explosive>());
    }

    if (GetComponent<PlayerBehavior>())
    {
      NetworkServer.Spawn(spawnedBullet, shooter);
    }
    else
    {
      NetworkServer.Spawn(spawnedBullet);
    }
  }

  public void Fire_Pellets(GameObject shooter)
  {
    for (int i = 0; i < weapon.pelletCount; i++)
    {
      Fire_Regular(shooter);
    }
  }

  public IEnumerator UnlockMovement(float time, ComponentRefs shooterVars)
  {
    yield return new WaitForSecondsRealtime(time);
    shooterVars.lockMovement = false;
  }

  public virtual void SwitchWeapon(string weaponID)
  {
    if (weapon != null)
    {
      Destroy(weapon.gameObject);
    }

    if (refs.graphics.sprites.Count > 3)
    {
      refs.graphics.sprites.RemoveAt(3);
    }

    if (weaponID != null)
    {
      GameObject chosenWeapon = arsenal.Where(w => w.uniqueID == weaponID).ToArray()[0].gameObject;
      GameObject newWeapon = Instantiate(chosenWeapon, transform.position, transform.rotation, transform);
      transform.SetLocalPositionAndRotation(chosenWeapon.transform.position, chosenWeapon.transform.rotation);

      weapon = newWeapon.GetComponent<WeaponClass>();
      weapon.bulletsInMag = weapon.magazineSize;
      weapon.fireTimeout = 0;

      refs.graphics.sprites.Add(newWeapon.GetComponentInChildren<SpriteRenderer>());
      refs.graphics.sprites.Last().enabled = true;

      if (weapon.animateOnShot)
      {
        refs.weaponAnimator.animator = weapon.GetComponent<Animator>();
      }
    }
    else
    {
      if (refs.graphics.sprites.Count >= 4)
      {
        refs.graphics.sprites.Last().enabled = false;
      }
    }
  }
}
