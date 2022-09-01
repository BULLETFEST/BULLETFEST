using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

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

  // Start is called before the first frame update
  void Awake()
  {
    playerVars = GetComponent<PlayerVars>();
    uiController = GetComponent<PlayerUI>();
    weaponBehavior = GetComponentInChildren<WeaponBehavior>();

    health = maxHealth;

    FetchTime();
  }

  public override void OnStartAuthority()
  {
    base.OnStartAuthority();

    DiscordController.UpdateActivity(new Discord.Activity
    {
      State = "In a game",
      Party = {
        Size =  {
          MaxSize = 4,
          CurrentSize = NetworkServer.connections.Count
        }
      }
    });
  }

  [Command]
  void FetchTime()
  {
    playerVars.timeleft = FindObjectOfType<PlayerSpawnSystem>().timeStamp;
  }

  // Update is called once per frame
  void Update()
  {
    if (!isLocalPlayer) return;

    if (SaveSystem.IsSettingsOpen) return;

    weaponToPickup = FindClosestGun();

    if (Utilities.GetKeybind("fire") && !playerVars.lockShooting) Shoot();

    if (Utilities.GetKeybindUp("fire")) ShootKeyUp();

    if (Utilities.GetKeybindDown("weaponPickup") && weaponToPickup != null)
    {
      SwitchWeapon(weaponToPickup);
      weaponToPickup = null;
    }
  }

  GameObject FindClosestGun()
  {
    GameObject[] pickableGuns;
    pickableGuns = GameObject.FindGameObjectsWithTag("WeaponItem");
    GameObject closest = null;
    float distance = Mathf.Infinity;
    Vector3 position = transform.position;
    foreach (GameObject go in pickableGuns)
    {
      Vector3 diff = go.transform.position - position;
      float curDistance = diff.sqrMagnitude;
      if (curDistance < distance)
      {
        //if pickable can be inserted here ~Toast
        closest = go;
        distance = curDistance;
      }
    }

    return distance <= 6.5f ? closest : null;
  }

  [Command]
  void SwitchWeapon(GameObject weapon)
  {
    if (weapon != null && !playerVars.lockMovement)
    {
      TargetRpc_SwitchWeapon(weapon.GetComponent<WeaponItem>().WeaponID);
      weaponBehavior.SwitchWeapon(weapon.GetComponent<WeaponItem>().WeaponID);
      NetworkServer.Destroy(weapon);
    }
  }

  [ClientRpc]
  void TargetRpc_SwitchWeapon(string WeaponID)
  {
    weaponBehavior.SwitchWeapon(WeaponID);

    // if (playerVars.reloadRoutine != null) 
  }

  [Command] void ShootKeyUp() => shootKeyUp = true;

  [Command]
  void Shoot()
  {
    WeaponClass weapon = playerVars.weaponBehavior.weapon;

    if (playerVars.lockShooting) return;
    if (playerVars.weaponBehavior.weapon == null) return;
    if (weapon.firingMode == WeaponClass.FireMode.Single && !shootKeyUp) return;
    if (weapon.fireTimeout > NetworkTime.time) return;
    if (weapon.bulletsInMag <= 0) return;



    weapon.bulletsInMag--;
    weapon.fireTimeout = (float)NetworkTime.time + (1f / weapon.fireRate);

    playerVars.weaponBehavior.Shoot(weapon.ID, connectionToClient);
    Rpc_AddForce(gameObject, weapon.shootSound);
    Target_UpdateUI(weapon.bulletsInMag);
    shootKeyUp = false;
  }

  [ClientRpc]
  void Rpc_AddForce(GameObject target, string shootSound)
  {
    playerVars.weaponBehavior.AddForce(target);
    if (shootSound != "")
      playerVars.audioSystem.PlaySound(shootSound);
  }

  [TargetRpc]
  void Target_UpdateUI(int bulletsInMag)
  {
    StartCoroutine(Camera.main.GetComponent<CameraShake>().Shake(playerVars.weaponBehavior.weapon.cameraShakeDuration,
                                                                 playerVars.weaponBehavior.weapon.cameraShakeIntensity));
    playerVars.weaponBehavior.weapon.bulletsInMag = bulletsInMag;
    uiController.UpdateAmmoText(bulletsInMag);
  }

  [Command(requiresAuthority = false)]
  public void TakeDamage(float damage, GameObject owner)
  {
    damageDealer = owner;

    health -= damage;

    PlayHitSound(connectionToClient);
  }

  [TargetRpc]
  void PlayHitSound(NetworkConnection conn) => playerVars.audioSystem.PlaySound("Hit");

  GameObject damageDealer = null;
  public void OnDamageTaken(float oldHealth, float newHealth)
  {
    if (health > 0) return;
    Server_Die(damageDealer != null ? damageDealer : gameObject,
               gameObject);
  }

  public bool dead = false;
  [Command(requiresAuthority = false)]
  public void Server_Die(GameObject killer, GameObject killed)
  {
    if (dead) return;

    dead = true;
    playerVars.lockMovement = true;
    playerVars.lockShooting = true;
    playerVars.lockWeapon = true;

    MyNetworkManager.GameMode gm = FindObjectOfType<MyNetworkManager>().gameMode;

    if (gm == MyNetworkManager.GameMode.Deathmatch) playerVars.uiName.gameObject.SetActive(false);

    NetworkConnectionToClient killerIdentity = killer.GetComponent<NetworkIdentity>().connectionToClient;

    if (killer == killed) MyNetworkManager.instance.players[killerIdentity].kills--;
    else MyNetworkManager.instance.players[killerIdentity].kills++;

    string killerName = killer.GetComponent<PlayerVars>().displayName;
    string killedName = killed.GetComponent<PlayerVars>().displayName;

    ClientRpc_Die(killerName, killedName);

    if (gm != MyNetworkManager.GameMode.Deathmatch)
    {
      // GameObject spawnedGravestone = Instantiate(gravestone, new Vector2(transform.position.x,
      //                                                       playerVars.bc.bounds.min.y + (gravestone.GetComponentInChildren<SpriteRenderer>().bounds.size.y / 2)), Quaternion.Euler(0, 0, 0));
      // NetworkServer.Spawn(spawnedGravestone);
      LayerMask lm = 1 << 6;
      lm |= 1 << 12;

      RaycastHit2D hit = playerVars.playerMovement.Grounded(gameObject, playerVars.bc, lm, 999999f);
      if (hit.collider != null)
      {
        GameObject spawnedGravestone = Instantiate(gravestone, new Vector2(hit.point.x, hit.point.y + (gravestone.GetComponentInChildren<SpriteRenderer>().bounds.size.y / 2)), Quaternion.Euler(0, 0, 0));
        NetworkServer.Spawn(spawnedGravestone);
      }
    }

    foreach (var player in NetworkServer.connections)
    {
      UpdateKillfeed(player.Value, killerName, killedName);
    }

    MyNetworkManager.instance.OnPlayerDie(connectionToClient);
  }

  [ClientRpc]
  public void ClientRpc_Die(string killer, string killed)
  {
    playerVars.graphics.DisableAll();
    playerVars.uiName.gameObject.SetActive(false);
    this.gameObject.GetComponent<BoxCollider2D>().enabled = false;
    this.gameObject.GetComponent<Rigidbody2D>().simulated = false;
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
