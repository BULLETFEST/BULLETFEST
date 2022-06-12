using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NewPB : NetworkBehaviour
{

  public PlayerVars playerVars;
  public PlayerUI uiController;

  [SyncVar]
  float health = 100f;

  bool shootKeyUp = true;

  void Start()
  {
    playerVars = GetComponent<PlayerVars>();
    uiController = GetComponent<PlayerUI>();
  }

  // Update is called once per frame
  void Update()
  {
    if (!isLocalPlayer) return;

    if (Input.GetKeyDown(KeyCode.Mouse0)) Shoot();

    if (Input.GetKeyUp(KeyCode.Mouse0)) ShootKeyUp();
  }

  [Command] void ShootKeyUp() => shootKeyUp = true;

  [Command]
  void Shoot()
  {
    WeaponClass weapon = playerVars.weaponBehavior.weapon;

    if (playerVars.isReloading)
    {
      if (weapon.reloadType == WeaponClass.ReloadType.Shells &&
          weapon.bulletsInMag > 0)
      {
        StopCoroutine(playerVars.reloadRoutine);
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
      if (weapon.bulletsInMag <= 0)
      {
        playerVars.reloadRoutine = StartCoroutine(playerVars.weaponBehavior.Reload());
        Target_Reload();
      }
      Target_UpdateUI(weapon.bulletsInMag);
      shootKeyUp = false;
    }
  }

  [TargetRpc]
  void Target_UpdateUI(int bulletsInMag)
  {
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
    StopCoroutine(playerVars.reloadRoutine);
    playerVars.reloadRoutine = null;

    uiController.uiReloadCircle.enabled = false;
    playerVars.isReloading = false;
  }
}
