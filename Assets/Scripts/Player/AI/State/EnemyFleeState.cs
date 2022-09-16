using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class EnemyFleeState : EnemyBaseState
{

  GameObject furthestNode;
  GameObject nearestPlayer;

  public override void EnterState(EnemyAI manager)
  {
    furthestNode = Utilities.FindFurthest(manager.transform, "NavigationPoint");
    nearestPlayer = Utilities.FindNearest(manager.transform, "Player");
  }

  public override void ExitState(EnemyAI manager)
  {

  }

  public override void UpdateState(EnemyAI manager)
  {
    furthestNode = Utilities.FindFurthest(manager.transform, "NavigationPoint");
    nearestPlayer = Utilities.FindNearest(manager.transform, "Player");

    if (GameObject.FindGameObjectWithTag("WeaponItem")) manager.SwitchState(manager.enemyLookForWeaponState);
  }

  public override Path CalculatePath(EnemyAI manager)
  {
    if (Vector2.Distance(manager.gameObject.transform.position, nearestPlayer.transform.position) < 5)
      return manager.seeker.StartPath(manager.gameObject.transform.position, furthestNode.transform.position);

    return null;
  }
}
