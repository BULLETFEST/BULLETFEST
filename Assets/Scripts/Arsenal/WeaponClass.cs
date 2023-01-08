using UnityEngine;

[System.Serializable]
public class WeaponClass : MonoBehaviour
{
  public enum FireMode
  {
    Single,
    Burst,
    Auto
  }

  public enum ReloadType
  {
    Magazine,
    Shells
  }

  [Header("Base Weapon Information")]
  public string weaponName;
  [Tooltip(
  @"Valid weapon IDs: 
  HDG - Handgun
  RPG - Rocket Propelled Grenade
  THRW - Throwable
  SMG - Sub-Machine Gun
  LMG - Light Machine Gun
  STG - Shotgun
  SNR - Sniper Rifle
  MEL - Melee
  ")]
  public string ID;

  public bool rotateWithCursor = true;
  public bool deleteOnEmpty = false;

  public bool isShotgun = false;

  public bool isMelee = false;

  [DrawIf("isShotgun", true)]
  public int pelletCount;

  [DrawIf("isMelee", true)]
  public float meleeRange;

  public int magazineSize;

  public float[] inaccuracyRange = new float[] { 0, 0 };

  public float shotPushback;
  public float movementUnlockTime;

  public float damage;
  public float fireRate;

  public float cameraShakeIntensity,
               cameraShakeDuration;

  public FireMode firingMode;

  // public ReloadType reloadType;

  [Header("Projectile")]
  public GameObject projectilePrefab;
  public Transform projectileSpawnPoint;
  public float projectileVelocity;
  public float projectileTorque;

  [Header("")]
  public string shootSound = "Shoot";

  public Sprite weaponIcon;


  // Copied over from DWAG2 Code
  //   public WeaponClass(int magSize, int pelletCount, int[] inAccRange,
  //                      float reloadTime, float shootPushback, float range,
  //                      float damage, float bulletVel, string ID,
  //                      string weaponName, FireType fireType, ReloadType reloadType,
  //                      float fireRate, GameObject bulletPrefab, Vector2 bulletSpawnPoint)
  //   {
  //     this.magSize = magSize;
  //     this.pelletCount = pelletCount;
  //     this.inAccRange = inAccRange;
  //     this.reloadTime = reloadTime;
  //     this.shootPushback = shootPushback;
  //     this.range = range;
  //     this.damage = damage;
  //     this.weaponName = weaponName;
  //     this.fireType = fireType;
  //     this.fireRate = fireRate;
  //     this.bulletPrefab = bulletPrefab;
  //     this.bulletVel = bulletVel;
  //     this.ID = ID;
  //     this.reloadType = reloadType;
  //     this.bulletSpawnPoint = bulletSpawnPoint;
  //   }

  [HideInInspector]
  public float fireTimeout = 0;

  [HideInInspector]
  public int bulletsInMag;

  public bool animateOnShot = false;
  public float animationShotDamageDelay = 0f;
}
