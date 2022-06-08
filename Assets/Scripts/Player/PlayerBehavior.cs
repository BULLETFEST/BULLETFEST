using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerBehavior : NetworkBehaviour
{
  public float maxHealth = 10f;

  [SyncVar]
  public float health = 1f;

  PlayerVars playerVars;

  WeaponBehavior weaponBehavior;

  // Start is called before the first frame update
  void Start()
  {
    health = maxHealth;

    playerVars = GetComponent<PlayerVars>();
    weaponBehavior = GetComponentInChildren<WeaponBehavior>();
    weaponBehavior.owner = NetworkClient.connection;
  }

  // Update is called once per frame
  void Update()
  {
    if (!isLocalPlayer) return;

    if (Input.GetKeyDown(KeyCode.X)) TakeDamage(10, null);

    if (Input.GetKeyDown(KeyCode.Mouse0) && !playerVars.lockMovement)
    {
      Server_Shoot();
    }
  }

  [Command]
  void Server_Shoot() => Rpc_Shoot();

  [ClientRpc]
  void Rpc_Shoot()
  {
    // WeaponClass WeaponStats = weaponBehavior.WeaponStats;
    // GameObject spawnedBullet = Instantiate(WeaponStats.bulletPrefab, WeaponStats.bulletSpawnPoint.transform.position, Quaternion.Euler(0, 0, Random.Range(WeaponStats.inaccuracyRange[0], WeaponStats.inaccuracyRange[1])));

    // spawnedBullet.GetComponent<Rigidbody2D>().velocity = new Vector2(WeaponStats.bulletVelocity, 0) * playerObjects.graphics.transform.right;

    // spawnedBullet.GetComponent<Bullet>().owner = null;
    // spawnedBullet.GetComponent<Bullet>().damage = WeaponStats.damage;
    weaponBehavior.Invoke("ShootBullet", 0f);
  }

  public void TakeDamage(float damage, NetworkConnection owner)
  {
    health -= damage;

    OnDamageTaken(health, owner);
  }

  public void OnDamageTaken(float health, NetworkConnection owner = null)
  {
    if (health > 0) return;
    Server_Die(owner != null ? owner.identity.GetComponent<PlayerVars>().name : playerVars.uiName.text, playerVars.uiName.text);
  }

  [Command(requiresAuthority = false)]
  public void Server_Die(string killer, string killed)
  {
    playerVars.lockMovement = true;

    ClientRpc_Die(killer, killed);
  }

  [ClientRpc]
  public void ClientRpc_Die(string killer, string killed)
  {
    playerVars.graphics.SetActive(false);
    this.gameObject.GetComponent<BoxCollider2D>().enabled = false;
    this.gameObject.GetComponent<Rigidbody2D>().simulated = false;

    Debug.Log($"{killer} KILLED {killed}");
  }


}
