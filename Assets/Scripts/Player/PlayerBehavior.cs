using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public class PlayerBehavior : NetworkBehaviour
{
  public float maxHealth = 10f;

  [SyncVar(hook = nameof(OnDamageTaken))]
  public float health = 1f;

  [HideInInspector]
  public PlayerVars playerVars;
  [HideInInspector]
  public PlayerUI uiController;
  [HideInInspector]
  public WeaponBehavior weaponBehavior;

  bool shootKeyUp = true;

  GameObject weaponToPickup;

  public GameObject gravestone, killfeed, killfeedItem;

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
  void Awake()
  {
    playerVars = GetComponent<PlayerVars>();
    uiController = GetComponent<PlayerUI>();
    weaponBehavior = GetComponentInChildren<WeaponBehavior>();

    health = maxHealth;

    uiController.uiHealthSlider.maxValue = maxHealth;
    uiController.uiHealthSlider.value = maxHealth;
  }

  // Update is called once per frame
  void Update()
  {
    if (!isLocalPlayer) return;

    if (Input.GetKeyDown(KeyCode.X)) TakeDamage(1, null);

    if (Input.GetKey(KeyCode.Mouse0) && !playerVars.lockShooting) Shoot(isServer);

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
    TargetRpc_SwitchWeapon(weaponToPickup.GetComponent<WeaponItem>().WeaponID);
    weaponBehavior.SwitchWeapon(weaponToPickup.GetComponent<WeaponItem>().WeaponID);
    NetworkServer.Destroy(weaponToPickup);
  }

  [TargetRpc]
  void TargetRpc_SwitchWeapon(string WeaponID)
  {
    weaponBehavior.SwitchWeapon(WeaponID);
  }

  [Command] void ShootKeyUp() => shootKeyUp = true;

  [Command]
  void Shoot(bool _isServer)
  {
    if (playerVars.lockShooting) return;

    // bool _isServer = conn.identity.isServer;
    // Debug.Log(_isServer);

    WeaponClass weapon = playerVars.weaponBehavior.weapon;

    if (playerVars.isReloading)
    {
      if (weapon.reloadType == WeaponClass.ReloadType.Shells &&
          weapon.bulletsInMag > 0)
      {
        StopCoroutine(playerVars.reloadRoutine);
        playerVars.isReloading = false;

        Target_CancelReload();
      }
    }
    else
    {
      if (weapon.firingMode == WeaponClass.FireMode.Single && !shootKeyUp) return;
      if (weapon.fireTimeout > NetworkTime.time) return;

      weapon.bulletsInMag--;
      weapon.fireTimeout = (float)NetworkTime.time + weapon.fireRate;

      playerVars.weaponBehavior.Shoot(weapon.ID, connectionToClient);
      Rpc_AddForce(gameObject);
      playerVars.weaponBehavior.AddForce(gameObject);
      if (weapon.bulletsInMag <= 0)
      {
        playerVars.reloadRoutine = StartCoroutine(playerVars.weaponBehavior.Reload());
        if (!_isServer) Target_Reload();
      }
      Target_UpdateUI(weapon.bulletsInMag);
      shootKeyUp = false;
    }
  }

  [ClientRpc]
  void Rpc_AddForce(GameObject target)
  {
    playerVars.weaponBehavior.AddForce(target);
  }

  [TargetRpc]
  void Target_UpdateUI(int bulletsInMag)
  {
    playerVars.weaponBehavior.weapon.bulletsInMag = bulletsInMag;
    uiController.UpdateAmmoText(bulletsInMag, playerVars.weaponBehavior.weapon.magazineSize);
  }

  [TargetRpc]
  void Target_Reload()
  {
    playerVars.reloadRoutine = StartCoroutine(playerVars.weaponBehavior.Reload());
  }

  [TargetRpc]
  void Target_CancelReload()
  {
    if (playerVars.reloadRoutine != null) StopCoroutine(playerVars.reloadRoutine);

    uiController.uiReloadCircle.enabled = false;
    playerVars.isReloading = false;
  }

  [Command(requiresAuthority = false)]
  public void TakeDamage(float damage, GameObject owner)
  {
    health -= damage;

    damageDealer = owner;
  }


  // [TargetRpc]
  public IEnumerator UpdateHealthBar()
  {
    while (uiController.uiHealthSlider.value + 0.1 != health)
    {
      uiController.uiHealthSlider.value = Mathf.Lerp(uiController.uiHealthSlider.value, health, 3 * Time.deltaTime);
      yield return null;
    }
  }

  GameObject damageDealer = null;
  public void OnDamageTaken(float oldHealth, float newHealth)
  {
    StartCoroutine(UpdateHealthBar());

    if (health > 0) return;
    // uiController.mainCanvas.enabled = false;
    uiController.infoGroup.alpha = 0;

    Server_Die(damageDealer != null ? damageDealer.GetComponent<PlayerVars>().name : playerVars.uiName.text, playerVars.uiName.text);
  }

  bool dead = false;
  [Command(requiresAuthority = false)]
  public void Server_Die(string killer, string killed)
  {
    if (dead) return;

    dead = true;
    playerVars.lockMovement = true;
    playerVars.lockShooting = true;
    playerVars.lockWeapon = true;



    ClientRpc_Die(killer, killed);
    GameObject spawnedGravestone = Instantiate(gravestone, new Vector3(transform.position.x, transform.position.y + 50), Quaternion.Euler(0, 0, 0));
    NetworkServer.Spawn(spawnedGravestone);
    // print(playerVars.gameObject.name);
    // Utilities.PrintArr(NetworkServer.spawned.Keys.ToArray());
    // print();
    // GameObject spawnedKillfeedItem = Instantiate(killfeedItem, Vector2.zero, Quaternion.Euler(0, 0, 0), NetworkServer.connections[0].identity.GetComponent<PlayerVars>().killfeed.transform);
    // spawnedKillfeedItem.GetComponent<KillFeedItem>().killer.text = killer;
    // spawnedKillfeedItem.GetComponent<KillFeedItem>().killed.text = killed;
    // NetworkServer.Spawn(spawnedKillfeedItem);
  }

  [ClientRpc]
  public void ClientRpc_Die(string killer, string killed)
  {
    playerVars.graphics.DisableAll();
    this.gameObject.GetComponent<BoxCollider2D>().enabled = false;
    this.gameObject.GetComponent<Rigidbody2D>().simulated = false;

    foreach (var player in NetworkServer.connections)
    {
      // PlayerVars localVars = player.Value.identity.gameObject.GetComponent<PlayerVars>();
      // GameObject spawnedKillfeedItem = Instantiate(killfeedItem, Vector3.zero, Quaternion.Euler(0, 0, 0), localVars.killfeed.transform);
      // spawnedKillfeedItem.GetComponent<KillFeedItem>().killer.text = killer;
      // spawnedKillfeedItem.GetComponent<KillFeedItem>().killed.text = killed;
      UpdateKillfeed(player.Value, killer, killed);
    }

  }

  [TargetRpc]
  public void UpdateKillfeed(NetworkConnection conn, string killer, string killed)
  {
    PlayerVars localVars = conn.identity.gameObject.GetComponent<PlayerVars>();
    GameObject spawnedKillfeedItem = Instantiate(killfeedItem, Vector3.zero, Quaternion.Euler(0, 0, 0), localVars.killfeed.transform);
    spawnedKillfeedItem.GetComponent<KillFeedItem>().killer.text = killer;
    spawnedKillfeedItem.GetComponent<KillFeedItem>().killed.text = killed;
  }
}
