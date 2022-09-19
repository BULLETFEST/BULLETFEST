using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class BotLookForWeaponState : BotBaseState
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
    {
      manager.SwitchWeapon(nearestGun);
      manager.SwitchState(manager.botHauntPlayerState);
    }
    else pickedWeaponUp = false;
  }

  public override void ExitState(BotController manager)
  {
    manager.OnReachTarget -= ReachedTarget;
  }

  public override void UpdateState(BotController manager)
  {
    nearestGun = Utilities.FindNearest(manager.transform, "WeaponItem");

    if (nearestGun == null && !pickedWeaponUp) manager.SwitchState(manager.botFleeState);
  }

  public override void CalculatePath(BotController manager)
  {
    if (nearestGun != null)
      manager.seeker.StartPath(manager.transform.position, nearestGun.transform.position);
  }

  public override float Timer() => 1.75f;
}
