using Pathfinding;
using UnityEngine;

public class BotFleeState : BotBaseState
{
<<<<<<< Updated upstream
  private GameObject furthestNode;
  private GameObject nearestPlayer;
  private bool escaping;
  private GameObject tempFurthest;

  public override void EnterState(BotPathfinding manager)
  {
    furthestNode = Utilities.FindFurthest(manager.transform, "NavigationPoint");
    nearestPlayer = Utilities.FindNearest(manager.transform, Object.FindObjectsOfType<DamageController>());

    escaping = true;
    tempFurthest = furthestNode;

    manager.OnReachTarget += ReachedTarget;
  }

  public void ReachedTarget(BotPathfinding manager)
  {
    escaping = false;
  }
=======

  public override void EnterState(BotPathfinding manager)
  {
    manager.OnReachTarget += ReachedTarget;
  }

  public void ReachedTarget(BotPathfinding manager) { }
>>>>>>> Stashed changes

  public override void ExitState(BotPathfinding manager)
  {
    manager.OnReachTarget -= ReachedTarget;
  }

  public override void UpdateState(BotPathfinding manager)
  {
<<<<<<< Updated upstream
    furthestNode = Utilities.FindFurthest(manager.transform, "NavigationPoint");
    nearestPlayer = Utilities.FindNearest(manager.transform, Object.FindObjectsOfType<DamageController>().Where(x => x.gameObject != manager.gameObject && !x.dead).ToArray());

=======
>>>>>>> Stashed changes
    if (GameObject.FindGameObjectWithTag("WeaponItem"))
    {
      manager.SwitchState(manager.botLookForWeaponState);
    }
  }

  public override void CalculatePath(BotPathfinding manager)
  {
<<<<<<< Updated upstream
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
=======
    RandomPath path = RandomPath.Construct(manager.transform.position, 10 * 1000);
    manager.seeker.StartPath(path);
>>>>>>> Stashed changes
  }

  public override float Timer()
  {
    return 2.25f;
  }
}
