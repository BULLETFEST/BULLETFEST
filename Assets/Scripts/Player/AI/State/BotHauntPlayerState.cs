using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class BotHauntPlayerState : BotBaseState
{
  GameObject nearestGun;

  public override void EnterState(BotController manager)
  {
    nearestGun = Utilities.FindNearest(manager.transform, "WeaponItem");

    manager.OnReachTarget += ReachedTarget;
  }

  bool pickedWeaponUp;

  public void ReachedTarget(BotController manager)
  {
    pickedWeaponUp = true;

    if (nearestGun != null)
      manager.SwitchWeapon(nearestGun);
    else pickedWeaponUp = false;
  }

  public override void ExitState(BotController manager)
  {
    manager.OnReachTarget -= ReachedTarget;
  }

  public override void UpdateState(BotController manager)
  {
    nearestGun = Utilities.FindNearest(manager.transform, "WeaponItem");

    if (nearestGun == null && !pickedWeaponUp) manager.SwitchState(manager.enemyFleeState);
  }

  public override void CalculatePath(BotController manager)
  {
    if (nearestGun != null)
      manager.seeker.StartPath(manager.transform.position, nearestGun.transform.position);
  }
}
