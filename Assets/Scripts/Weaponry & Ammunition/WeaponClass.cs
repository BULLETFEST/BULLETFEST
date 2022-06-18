using System.Collections;
using System.Collections.Generic;
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

  public int magazineSize;

  public bool isShotgun = false;
  [DrawIf("isShotgun", true)]
  public int pelletCount;

  public int[] inaccuracyRange = new int[] { 0, 0 };

  public float reloadTime;
  public float shotPushback;
  public float range;
  public float damage;
  public float fireRate;
  public float bulletVelocity;
  public float movementUnlockTime;

  public float cameraShakeIntensity,
               cameraShakeDuration;

  public string weaponName;
  public string ID;

  public FireMode firingMode;

  public ReloadType reloadType;

  public GameObject bulletPrefab;
  public Transform bulletSpawnPoint;

  public Sprite weaponSprite;



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
}
