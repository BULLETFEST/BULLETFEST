using UnityEngine;

public class BotLookForWeaponState : BotBaseState
{
  private GameObject nearestGun;

  public override void EnterState(BotPathfinding manager)
  {
    nearestGun = Utilities.FindNearest(manager.transform, "WeaponItem");

    manager.OnReachTarget += ReachedTarget;
  }

  private bool pickedWeaponUp;

  public void ReachedTarget(BotPathfinding manager)
  {
    Vector2 dir = (nearestGun.transform.position - manager.transform.position).normalized;
    RaycastHit2D rh = Physics2D.Raycast(manager.transform.position, dir, 7f);

    if (rh.collider == null || rh.distance > 6.5f)
    {
      return;
    }

    pickedWeaponUp = true;

    if (nearestGun != null)
    {
      manager.botRefs.botBehavior.SwitchWeapon(nearestGun);
      manager.SwitchState(manager.botHauntPlayerState);
    }
    else
    {
      pickedWeaponUp = false;
    }
  }

  public override void ExitState(BotPathfinding manager)
  {
    manager.OnReachTarget -= ReachedTarget;
  }

  public override void UpdateState(BotPathfinding manager)
  {
    nearestGun = Utilities.FindNearest(manager.transform, "WeaponItem", 10);

    if (nearestGun == null && !pickedWeaponUp)
    {
      manager.SwitchState(manager.botFleeState);
    }
  }

  public override void CalculatePath(BotPathfinding manager)
  {
    if (nearestGun != null)
    {
      manager.seeker.StartPath(manager.transform.position, nearestGun.transform.position);
    }
  }

  public override float Timer()
  {
    return 1.75f;
  }
}
