using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.Video;

public class PlayerBehavior : NetworkBehaviour
{
  private PlayerRefs playerRefs;
  private bool shootKeyUp = true;
  private GameObject weaponToPickup;

  private System.Action<GameObject> PlayHitSoundAction;

  // Start is called before the first frame update
  private void Awake()
  {
    playerRefs = GetComponent<PlayerRefs>();
  }

  private void Start()
  {
    PlayHitSoundAction = delegate (GameObject g) { PlayHitSound(connectionToClient); };

    playerRefs.damageController.onTakeDamage += PlayHitSoundAction;

    FetchTime();
  }

  private void OnDestroy()
  {
    // playerVars.damageController.onDeath -= Server_Die;
    playerRefs.damageController.onTakeDamage -= PlayHitSoundAction;
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
    playerRefs.timeleft = FindObjectOfType<PlayerSpawnSystem>().timeStamp;
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

    if (Utilities.GetKeybind("fire") && !playerRefs.lockShooting)
    {
      Fire();
    }

    if (Utilities.GetKeybindDown("altFire") && !playerRefs.lockShooting)
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

    if (Utilities.GetKeybindDown("scoreboard"))
    {
      ScoreboardManager.Instance.GetComponent<CanvasGroup>().alpha = 1;
    }
    if (Utilities.GetKeybindUp("scoreboard"))
    {
      ScoreboardManager.Instance.GetComponent<CanvasGroup>().alpha = 0;
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
    if (weapon != null && !playerRefs.lockMovement)
    {
      Rpc_SwitchWeapon(weapon.GetComponent<WeaponItem>().WeaponID);
      playerRefs.weaponBehavior.SwitchWeapon(weapon.GetComponent<WeaponItem>().WeaponID);
      NetworkServer.Destroy(weapon);

      Target_UpdateUI(playerRefs.weaponBehavior.arsenal.Where(x => x.uniqueID == weapon.GetComponent<WeaponItem>().WeaponID).ToArray()[0].magazineSize);
    }
  }

  [ClientRpc]
  private void Rpc_SwitchWeapon(string WeaponID)
  {
    playerRefs.weaponBehavior.SwitchWeapon(WeaponID);
  }

  [Command]
  private void ShootKeyUp()
  {
    shootKeyUp = true;
  }

  [Command]
  private void Fire()
  {
    WeaponClass weapon = playerRefs.weaponBehavior.weapon;

    if (playerRefs.lockShooting)
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

    if (weapon.fireTimeout > Time.time)
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

    weapon.fireTimeout = (float)Time.time + (1f / weapon.fireRate);

    Rpc_AddForce(gameObject);
    playerRefs.weaponBehavior.Fire(weapon.uniqueID, connectionToClient.identity.gameObject);

    Target_UpdateUI(weapon.bulletsInMag);
    Target_ShakeScreen();
    shootKeyUp = false;

    if (weapon.bulletsInMag <= 0 && weapon.deleteOnEmpty)
    {
      Rpc_SwitchWeapon(null);
      playerRefs.weaponBehavior.SwitchWeapon(null);
    }
  }

  [Command]
  private void AltFire()
  {
    if (playerRefs.lockShooting)
    {
      return;
    }

    WeaponBehavior weapon = playerRefs.weaponBehavior;

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
  private void Rpc_AddForce(GameObject target)
  {
    if (playerRefs.weaponBehavior.weapon.animateOnShot)
    {
      playerRefs.weaponAnimator.animator.Play("Fire");
    }

    playerRefs.weaponBehavior.AddForce(target);
    if (playerRefs.weaponBehavior.weapon.soundOnShoot)
    {
      playerRefs.audioSystem.PlaySound(playerRefs.weaponBehavior.weapon.shootSound);
    }
  }

  [TargetRpc]
  private void Target_UpdateUI(int bulletsInMag)
  {

    playerRefs.weaponBehavior.weapon.bulletsInMag = bulletsInMag;
    playerRefs.uiController.UpdateAmmoText(bulletsInMag);
  }

  [TargetRpc]
  private void Target_ShakeScreen()
  {
    StartCoroutine(Camera.main.GetComponent<CameraShake>().Shake(playerRefs.weaponBehavior.weapon.cameraShakeDuration,
                                                                 playerRefs.weaponBehavior.weapon.cameraShakeIntensity));
  }

  [TargetRpc]
  private void PlayHitSound(NetworkConnection conn)
  {
    playerRefs.audioSystem.PlaySound("Hit");
  }
}
