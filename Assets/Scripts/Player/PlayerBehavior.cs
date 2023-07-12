using Mirror;
using UnityEngine;
using UnityEngine.Video;

public class PlayerBehavior : NetworkBehaviour
{
  // public float maxHealth = 10f;

  // [SyncVar(hook = nameof(OnDamageTaken))]
  // public float health = 1f;

  [HideInInspector]
  public PlayerVars playerVars;
  [HideInInspector]
  public PlayerUI uiController;
  [HideInInspector]
  public WeaponBehavior weaponBehavior;
  private bool shootKeyUp = true;
  private GameObject weaponToPickup;

  public GameObject gravestone, killfeed, killfeedItem, playerDeathParticles;
  private System.Action<GameObject> PlayHitSoundAction;

  // Start is called before the first frame update
  private void Start()
  {
    playerVars = GetComponent<PlayerVars>();
    uiController = GetComponent<PlayerUI>();
    weaponBehavior = GetComponentInChildren<WeaponBehavior>();

    // health = maxHealth;

    PlayHitSoundAction = delegate (GameObject g) { PlayHitSound(connectionToClient); };

    // playerVars.damageController.onDeath += Server_Die;
    playerVars.damageController.onTakeDamage += PlayHitSoundAction;

    VideoPlayer v = Camera.main.gameObject.AddComponent<VideoPlayer>();
    v.clip = Resources.Load<VideoClip>("glitch");
    v.isLooping = true;
    v.playOnAwake = true;
    v.waitForFirstFrame = true;
    v.playbackSpeed = 1.75f;
    v.targetCameraAlpha = 0.222f;
    v.aspectRatio = VideoAspectRatio.FitInside;
    v.audioOutputMode = VideoAudioOutputMode.None;
    v.renderMode = VideoRenderMode.CameraFarPlane;

    v.Play();

    FetchTime();
  }

  private void OnDestroy()
  {
    // playerVars.damageController.onDeath -= Server_Die;
    playerVars.damageController.onTakeDamage -= PlayHitSoundAction;
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
  private void FetchTime()
  {
    playerVars.timeleft = FindObjectOfType<PlayerSpawnSystem>().timeStamp;
  }

  // Update is called once per frame
  private void Update()
  {
    if (!isLocalPlayer)
    {
      return;
    }

    if (SaveSystem.IsSettingsOpen)
    {
      return;
    }

    weaponToPickup = FindClosestGun();

    if (Utilities.GetKeybind("fire") && !playerVars.lockShooting)
    {
      Fire();
    }

    if (Utilities.GetKeybindDown("altFire") && !playerVars.lockShooting)
    {
      AltFire();
    }

    if (Utilities.GetKeybindUp("fire"))
    {
      ShootKeyUp();
    }

    if (Utilities.GetKeybindDown("weaponPickup") && weaponToPickup != null)
    {
      SwitchWeapon(weaponToPickup);
      weaponToPickup = null;
    }
  }

  private GameObject FindClosestGun()
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
  private void SwitchWeapon(GameObject weapon)
  {
    if (weapon != null && !playerVars.lockMovement)
    {
      TargetRpc_SwitchWeapon(weapon.GetComponent<WeaponItem>().WeaponID);
      weaponBehavior.SwitchWeapon(weapon.GetComponent<WeaponItem>().WeaponID);
      NetworkServer.Destroy(weapon);
    }
  }

  [ClientRpc]
  private void TargetRpc_SwitchWeapon(string WeaponID)
  {
    weaponBehavior.SwitchWeapon(WeaponID);

    // if (playerVars.reloadRoutine != null)
  }

  [Command]
  private void ShootKeyUp()
  {
    shootKeyUp = true;
  }

  [Command]
  private void Fire()
  {
    WeaponClass weapon = playerVars.weaponBehavior.weapon;

    if (playerVars.lockShooting)
    {
      return;
    }

    if (weapon == null)
    {
      return;
    }

    if (weapon.firingMode == WeaponClass.FireMode.Single && !shootKeyUp)
    {
      return;
    }

    if (weapon.fireTimeout > NetworkTime.time)
    {
      return;
    }

    if (weapon.bulletsInMag <= 0 && !weapon.isMelee)
    {
      return;
    }

    if (!weapon.isMelee)
    {
      weapon.bulletsInMag--;
    }

    weapon.fireTimeout = (float)NetworkTime.time + (1f / weapon.fireRate);

    Rpc_AddForce(gameObject, weapon.shootSound);
    playerVars.weaponBehavior.Fire(weapon.ID, connectionToClient);

    Target_UpdateUI(weapon.bulletsInMag);
    shootKeyUp = false;

    if (weapon.bulletsInMag <= 0 && weapon.deleteOnEmpty)
    {
      TargetRpc_SwitchWeapon(null);
      weaponBehavior.SwitchWeapon(null);
    }
  }

  [Command]
  private void AltFire()
  {
    if (playerVars.lockShooting)
    {
      return;
    }

    print("Called explosion");

    WeaponBehavior weapon = playerVars.weaponBehavior;

    if (weapon.awaitingDetonation.Count > 0)
    {
      foreach (Explosive explosive in weapon.awaitingDetonation)
      {
        explosive.Detonate();
      }

      weapon.awaitingDetonation.Clear();
    }
  }

  [ClientRpc]
  private void Rpc_AddForce(GameObject target, string shootSound)
  {
    if (playerVars.weaponBehavior.weapon.animateOnShot)
    {
      playerVars.weaponAnimator.animator.Play("Fire");
    }

    playerVars.weaponBehavior.AddForce(target);
    if (shootSound != "")
    {
      playerVars.audioSystem.PlaySound(shootSound);
    }
  }

  [TargetRpc]
  private void Target_UpdateUI(int bulletsInMag)
  {
    StartCoroutine(Camera.main.GetComponent<CameraShake>().Shake(playerVars.weaponBehavior.weapon.cameraShakeDuration,
                                                                 playerVars.weaponBehavior.weapon.cameraShakeIntensity));
    playerVars.weaponBehavior.weapon.bulletsInMag = bulletsInMag;
    uiController.UpdateAmmoText(bulletsInMag);
  }

  // [Command(requiresAuthority = false)]
  // public void TakeDamage(float damage, GameObject owner)
  // {
  //   if (dead) return;

  //   damageDealer = owner;

  //   health -= damage;

  //   PlayHitSound(connectionToClient);
  // }

  [TargetRpc]
  private void PlayHitSound(NetworkConnection conn)
  {
    playerVars.audioSystem.PlaySound("Hit");
  }

  // GameObject damageDealer = null;
  // public void OnDamageTaken(float oldHealth, float newHealth)
  // {
  //   if (health > 0) return;
  //   Server_Die(damageDealer ?? gameObject,
  //              gameObject);
  // }
}
