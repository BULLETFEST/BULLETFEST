using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class WeaponBehavior : MonoBehaviour
{
  public WeaponClass WeaponStats;
  public NetworkConnection owner;

  void Start()
  {
    WeaponStats.fireRate /= 10;
  }

  public void ShootBullet()
  {
    GameObject spawnedBullet = Instantiate(WeaponStats.bulletPrefab, WeaponStats.bulletSpawnPoint.transform.position, Quaternion.Euler(0, 0, Random.Range(WeaponStats.inaccuracyRange[0], WeaponStats.inaccuracyRange[1])));

    spawnedBullet.GetComponent<Rigidbody2D>().velocity = new Vector2(WeaponStats.bulletVelocity, 0) * transform.right;

    spawnedBullet.GetComponent<Bullet>().owner = owner;
    spawnedBullet.GetComponent<Bullet>().damage = WeaponStats.damage;
  }
}
