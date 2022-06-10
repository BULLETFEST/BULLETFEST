using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public class PlayerBehavior : NetworkBehaviour
{
  public float maxHealth = 10f;

  [SyncVar]
  public float health = 1f;

  PlayerVars playerVars;
  PlayerUI uiController;

  WeaponBehavior weaponBehavior;

  bool shootKeyUp = true;

  GameObject weaponToPickup;

  private void OnTriggerEnter2D(Collider2D other)
  {
    if (other.gameObject.tag != "WeaponItem") return;

    weaponToPickup = other.gameObject;
  }

  private void OnTriggerExit2D(Collider2D other)
  {
    if (other.gameObject.tag != "WeaponItem") return;

    weaponToPickup = null;
  }

  // Start is called before the first frame update
  void Start()
  {
    health = maxHealth;

    playerVars = GetComponent<PlayerVars>();
    uiController = GetComponent<PlayerUI>();

    weaponBehavior = GetComponentInChildren<WeaponBehavior>();
    weaponBehavior.owner = NetworkClient.connection;
  }

  // Update is called once per frame
  void Update()
  {
    if (!isLocalPlayer) return;

    if (Input.GetKeyDown(KeyCode.X)) TakeDamage(10, null);

    if (Input.GetKey(KeyCode.Mouse0) && !playerVars.lockMovement) Server_Shoot();

    if (Input.GetKeyUp(KeyCode.Mouse0)) ShootKeyUp();

    if (Input.GetKeyDown(KeyCode.E) && weaponToPickup != null)
    {
      SwitchWeapon();
      weaponToPickup = null;
    }
  }

  [Command]
  void SwitchWeapon()
  {
    weaponBehavior.SwitchWeapon(weaponToPickup.GetComponent<WeaponItem>().WeaponID);
    NetworkServer.Destroy(weaponToPickup);
  }

  [Command] void ShootKeyUp() => shootKeyUp = true;

  [Command]
  void Server_Shoot()
  {
    if (playerVars.isReloading)
    {
      if (weaponBehavior.weapon.reloadType == WeaponClass.ReloadType.Shells &&
          weaponBehavior.weapon.bulletsInMag > 0)
      {
        StopCoroutine(playerVars.reloadRoutine);
        CancelReload(connectionToClient);
      }
    }
    else
    {
      if (weaponBehavior.weapon.firingMode == WeaponClass.FireMode.Single && !shootKeyUp) return;
      if (weaponBehavior.weapon.fireTimeout > NetworkTime.time) return;
      Rpc_Shoot(weaponBehavior.weapon.ID);

      weaponBehavior.weapon.bulletsInMag--;
      weaponBehavior.weapon.fireTimeout = (float)NetworkTime.time + weaponBehavior.weapon.fireRate;
      if (weaponBehavior.weapon.bulletsInMag <= 0)
      {
        playerVars.reloadRoutine = StartCoroutine(weaponBehavior.Reload());
      }

      PostFire(connectionToClient, weaponBehavior.weapon.bulletsInMag, weaponBehavior.weapon.magazineSize);
      shootKeyUp = false;
    }
  }

  [TargetRpc]
  void CancelReload(NetworkConnection conn)
  {
    StopCoroutine(playerVars.reloadRoutine);
    uiController.uiReloadCircle.enabled = false;
    playerVars.isReloading = false;
  }

  [TargetRpc]
  void PostFire(NetworkConnection conn, int bulletsInMag, int magazineSize)
  {
    if (bulletsInMag <= 0) playerVars.reloadRoutine = StartCoroutine(weaponBehavior.Reload());
    uiController.UpdateAmmoText(bulletsInMag, magazineSize);
  }

  // Spawn Bullet on ALL clients
  [ClientRpc]
  void Rpc_Shoot(string weaponID) => weaponBehavior.Shoot(weaponID);

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
