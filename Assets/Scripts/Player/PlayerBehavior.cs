using System.Linq;
using Mirror;
using UnityEngine;

public class PlayerBehavior : Behavior
{
  private bool shootKeyUp = true;
  private GameObject weaponToPickup;

  private System.Action<GameObject> PlayHitSoundAction;

  private void Start()
  {
    PlayHitSoundAction = delegate (GameObject g) { PlayHitSound(connectionToClient); };

    componentRefs.damageController.onTakeDamage += PlayHitSoundAction;

    FetchTime();
  }

  private void OnDestroy()
  {
    componentRefs.damageController.onTakeDamage -= PlayHitSoundAction;
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
    ((PlayerRefs)componentRefs).timeleft = FindObjectOfType<PlayerSpawnSystem>().timeStamp;
  }

  // Update is called once per frame
  protected override void Update()
  {
    base.Update();

    if (!isLocalPlayer) return;
    if (SettingsUI.IsSettingsOpen) return;

    if (Utilities.GetKeybind("fire") && !componentRefs.lockShooting)
    {
      Cmd_Fire();
    }

    if (Utilities.GetKeybindDown("altFire") && !componentRefs.lockShooting)
    {
      Cmd_AltFire();
    }

    if (Utilities.GetKeybindUp("fire"))
    {
      Cmd_ShootKeyUp();
    }

    if (Utilities.GetKeybindDown("weaponPickup"))
    {
      weaponToPickup = Utilities.FindNearest(transform, "WeaponItem", 6.5f);

      if (weaponToPickup == null) return;

      Cmd_SwitchWeapon(weaponToPickup);
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


  [Command]
  protected override void Cmd_SwitchWeapon(GameObject weapon)
  {
    base.Cmd_SwitchWeapon(weapon);

    Target_UpdateUI(componentRefs.weaponBehavior.arsenal.Where(x => x.uniqueID == weapon.GetComponent<WeaponItem>().WeaponID).ToArray()[0].magazineSize);
  }

  [Command]
  private void Cmd_ShootKeyUp()
  {
    shootKeyUp = true;
  }

  [Command]
  protected override void Cmd_Fire()
  {
    base.Cmd_Fire();

    if (componentRefs.weaponBehavior.weapon.firingMode == WeaponClass.FireMode.Single && !shootKeyUp)
    {
      return;
    }

    Target_UpdateUI(componentRefs.weaponBehavior.weapon.bulletsInMag);
    Target_ShakeScreen();
    shootKeyUp = false;
  }

  [Command]
  private void Cmd_AltFire()
  {
    if (componentRefs.lockShooting) return;

    WeaponBehavior weapon = componentRefs.weaponBehavior;

    if (weapon.awaitingDetonation.Count > 0)
    {
      foreach (Explosive explosive in weapon.awaitingDetonation)
      {
        explosive.Detonate();
      }
      weapon.awaitingDetonation.Clear();
    }
  }

  [TargetRpc]
  private void Target_UpdateUI(int bulletsInMag)
  {
    componentRefs.weaponBehavior.weapon.bulletsInMag = bulletsInMag;
    ((PlayerRefs)componentRefs).uiController.UpdateAmmoText(bulletsInMag);
  }

  [TargetRpc]
  private void Target_ShakeScreen()
  {
    StartCoroutine(Camera.main.GetComponent<CameraShake>().Shake(componentRefs.weaponBehavior.weapon.cameraShakeDuration,
                                                                 componentRefs.weaponBehavior.weapon.cameraShakeIntensity));
  }

  [TargetRpc]
  private void PlayHitSound(NetworkConnection conn)
  {
    AudioSystem.Instance.PlaySound("Hit");
  }
}
