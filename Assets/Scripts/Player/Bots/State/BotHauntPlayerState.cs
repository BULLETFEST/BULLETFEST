using System.Linq;
using UnityEngine;

public class BotHauntPlayerState : BotBaseState
{
  private GameObject nearestPlayer;

  public override void EnterState(BotPathfinding manager)
  {
    nearestPlayer = Utilities.FindNearest(manager.transform, Object.FindObjectsOfType<DamageController>());
  }

  public void ReachedTarget(BotPathfinding manager)
  {
    if (nearestPlayer != null)
    {
      manager.botVars.botBehavior.SwitchWeapon(nearestPlayer);
    }
  }

  public override void ExitState(BotPathfinding manager) { }

  private bool haunt = false;

  public override void UpdateState(BotPathfinding manager)
  {
    nearestPlayer = Utilities.FindNearest(manager.transform, Object.FindObjectsOfType<DamageController>().Where(x => x.gameObject != manager.gameObject && !x.dead).ToArray());

    if (nearestPlayer != null)
    {
      Vector2 dir = (nearestPlayer.transform.position - manager.transform.position).normalized;

      RaycastHit2D rh = Physics2D.Raycast(manager.transform.position, dir, Mathf.Infinity);

      /*manager.botVars.botWb.transform.localRotation*/

      if (rh.collider == null)
      {
        haunt = true;
        return;
      }

      if (rh.collider.gameObject.tag is "Player" or "Bot")
      {
        haunt = false;

        manager.path = null;
        manager.currentWaypoint = 0;

        manager.botVars.graphics.transform.rotation = dir.x > 0 ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 180, 0);

        Vector2 playerPos = rh.collider.transform.position;

        playerPos.x -= manager.transform.position.x;
        playerPos.y -= manager.transform.position.y;

        float angle = Mathf.Atan2(playerPos.y, playerPos.x) * Mathf.Rad2Deg;
        // manager.botVars.botWb.transform.LookAt(nearestPlayer.transform);

        manager.botVars.botBehavior.Fire(playerPos.x, angle);

        if (manager.botVars.botWb.weapon.bulletsInMag <= 0)
        {
          manager.SwitchState(manager.botFleeState);
        }
      }
      else
      {
        haunt = true;
      }
    }
  }

  public override void CalculatePath(BotPathfinding manager)
  {
    if (nearestPlayer != null && haunt)
    {
      manager.seeker.StartPath(manager.transform.position, nearestPlayer.transform.position);
    }
  }

  public override float Timer()
  {
    return 0.15f;
  }
}
