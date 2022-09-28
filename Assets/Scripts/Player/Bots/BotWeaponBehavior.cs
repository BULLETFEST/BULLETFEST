using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class BotWeaponBehavior : MonoBehaviour
{
  public WeaponClass weapon;
  public WeaponClass[] arsenal;

  Coroutine reloadRoutine;
  BotVars botVars;

  [HideInInspector]
  public List<Explosive> awaitingDetonation = new();

  void Start()
  {
    arsenal = FindObjectOfType<PlayerBehavior>().gameObject.GetComponentInChildren<WeaponBehavior>().arsenal;
    botVars = GetComponentInParent<BotVars>();
  }

  public void Fire(string weaponId, GameObject shooter)
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
    }


  }

  // Add recoil to the user
  public void AddForce(GameObject target)
  {
    if (weapon.shotPushback == 0) return;

    Rigidbody2D rb = target.GetComponent<Rigidbody2D>();

    rb.velocity = new Vector2(0, rb.velocity.y);
    botVars.lockMovement = true;
    Vector2 vel = weapon.shotPushback * -gameObject.transform.right;
    rb.AddForce(new Vector2(vel.x * 1.75f, vel.y / 1.55f), ForceMode2D.Impulse);
    StartCoroutine(UnlockMovement(weapon.movementUnlockTime/*, shooterVars*/));
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

    NetworkServer.Spawn(spawnedBullet);
  }

  public void Fire_Pellets(GameObject shooter)
  {
    for (int i = 0; i < weapon.pelletCount; i++)
    {
      Fire_Regular(shooter);
    }
  }

  IEnumerator UnlockMovement(float time)
  {
    yield return new WaitForSecondsRealtime(time);
    botVars.lockMovement = false;
  }

  public void SwitchWeapon(string weaponID)
  {
    if (weapon != null) Destroy(weapon.gameObject);
    GameObject newWeapon = Instantiate(arsenal.Where(w => w.ID == weaponID).ToArray()[0].gameObject, transform.position, transform.rotation, transform);
    weapon = newWeapon.GetComponent<WeaponClass>();
    weapon.bulletsInMag = weapon.magazineSize;
    weapon.fireTimeout = 0;

    if (botVars.graphics.sprites.Count > 2) botVars.graphics.sprites.RemoveAt(2);
    botVars.graphics.sprites.Add(newWeapon.GetComponentInChildren<SpriteRenderer>());
  }
}
