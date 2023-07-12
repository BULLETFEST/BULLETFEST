using Pathfinding;
using UnityEngine;

public class BotFleeState : BotBaseState
{
  public override void EnterState(BotPathfinding manager)
  {
    manager.OnReachTarget += ReachedTarget;
  }

  public void ReachedTarget(BotPathfinding manager) { }

  public override void ExitState(BotPathfinding manager)
  {
    manager.OnReachTarget -= ReachedTarget;
  }

  public override void UpdateState(BotPathfinding manager)
  {
    if (GameObject.FindGameObjectWithTag("WeaponItem"))
    {
      manager.SwitchState(manager.botLookForWeaponState);
    }
  }

  public override void CalculatePath(BotPathfinding manager)
  {
    RandomPath path = RandomPath.Construct(manager.transform.position, 10 * 1000);
    manager.seeker.StartPath(path);
  }

  public override float Timer()
  {
    return 2.25f;
  }
}
