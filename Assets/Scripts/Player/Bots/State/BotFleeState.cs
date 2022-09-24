using System.Linq;
using UnityEngine;

public class BotFleeState : BotBaseState
{

  GameObject furthestNode;
  GameObject nearestPlayer;

  bool escaping;
  GameObject tempFurthest;

  public override void EnterState(BotPathfinding manager)
  {
    furthestNode = Utilities.FindFurthest(manager.transform, "NavigationPoint");
    nearestPlayer = Utilities.FindNearest(manager.transform, GameObject.FindObjectsOfType<DamageController>());

    escaping = true;
    tempFurthest = furthestNode;

    manager.OnReachTarget += ReachedTarget;
  }

  public void ReachedTarget(BotPathfinding manager) => escaping = false;

  public override void ExitState(BotPathfinding manager)
  {
    manager.OnReachTarget -= ReachedTarget;
  }

  public override void UpdateState(BotPathfinding manager)
  {
    furthestNode = Utilities.FindFurthest(manager.transform, "NavigationPoint");
    nearestPlayer = Utilities.FindNearest(manager.transform, GameObject.FindObjectsOfType<DamageController>().Where(x => x.gameObject != manager.gameObject && !x.dead).ToArray());

    if (GameObject.FindGameObjectWithTag("WeaponItem")) manager.SwitchState(manager.botLookForWeaponState);
  }

  public override void CalculatePath(BotPathfinding manager)
  {
    if (escaping)
    {
      manager.seeker.StartPath(manager.transform.position, tempFurthest.transform.position);
    }
    if (nearestPlayer != null && Vector2.Distance(manager.transform.position, nearestPlayer.transform.position) < 5)
    {
      manager.seeker.StartPath(manager.transform.position, furthestNode.transform.position);
      tempFurthest = furthestNode;
      escaping = true;
    }
  }

  public override float Timer() => 0.15f;
}
