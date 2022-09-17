using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class BotFleeState : BotBaseState
{

  GameObject furthestNode;
  GameObject nearestPlayer;

  bool escaping;
  GameObject tempFurthest;

  public override void EnterState(BotController manager)
  {
    furthestNode = Utilities.FindFurthest(manager.transform, "NavigationPoint");
    nearestPlayer = Utilities.FindNearest(manager.transform, "Player");

    escaping = true;
    tempFurthest = furthestNode;

    manager.OnReachTarget += ReachedTarget;
  }

  public void ReachedTarget(BotController manager) => escaping = false;

  public override void ExitState(BotController manager)
  {
    manager.OnReachTarget -= ReachedTarget;
  }

  public override void UpdateState(BotController manager)
  {
    furthestNode = Utilities.FindFurthest(manager.transform, "NavigationPoint");
    nearestPlayer = Utilities.FindNearest(manager.transform, "Player");

    if (GameObject.FindGameObjectWithTag("WeaponItem")) manager.SwitchState(manager.botLookForWeaponState);
  }

  public override void CalculatePath(BotController manager)
  {
    if (escaping)
    {
      manager.seeker.StartPath(manager.transform.position, tempFurthest.transform.position);
    }
    if (Vector2.Distance(manager.transform.position, nearestPlayer.transform.position) < 5)
    {
      manager.seeker.StartPath(manager.transform.position, furthestNode.transform.position);
      tempFurthest = furthestNode;
      escaping = true;
    }
  }
}
