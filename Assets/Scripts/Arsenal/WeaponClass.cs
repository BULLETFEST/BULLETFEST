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

  public enum WeaponType
  {
    HDG,
    RPG,
    THRW,
    SMG,
    LMG,
    STG,
    SNR,
    MLE,
  }

  [Header("Basic Weapon Information")]
  public string weaponName;
  public string uniqueID;
  public WeaponType weaponType;


  [Header("Weapon Stats")]

  [DrawIf("weaponType", WeaponType.STG)]
  public int pelletCount;

  [DrawIf("weaponType", WeaponType.MLE)]
  public float meleeRange;

  [DrawIf("weaponType", WeaponType.MLE, true)]
  public int magazineSize;

  public float[] inaccuracyRange = new float[] { 0, 0 };

  public float shotPushback;
  public float movementUnlockTime;

  public float damage;

  public float fireRate;
  public FireMode firingMode;

  [Header("Camera")]
  public float cameraShakeIntensity;
  public float cameraShakeDuration;

  [Header("Projectile")]

  [DrawIf("weaponType", WeaponType.MLE, true)]
  public GameObject projectilePrefab;
  public Transform projectileSpawnPoint;
  [DrawIf("weaponType", WeaponType.MLE, true)]
  public float projectileVelocity;
  [DrawIf("weaponType", WeaponType.MLE, true)]
  public float projectileTorque;

  [Header("Sound")]
  public bool soundOnShoot = false;

  [DrawIf("soundOnShoot", true)]
  public string shootSound = "Shoot";

  [Header("Animation")]
  public bool animateOnShot = false;
  [DrawIf("animateOnShot", true)]
  public float animationShotDamageDelay = 0f;

  [Header("Misc")]
  public bool rotateWithCursor = true;
  public bool deleteOnEmpty = false;

  [HideInInspector]
  public float fireTimeout = 0;

  [HideInInspector]
  public int bulletsInMag;

  [HideInInspector]
  public bool isMelee { get => weaponType == WeaponType.MLE; }
}
