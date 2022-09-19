using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class BotHauntPlayerState : BotBaseState
{
  GameObject nearestPlayer;

  public override void EnterState(BotController manager)
  {
    nearestPlayer = Utilities.FindNearest(manager.transform, "Player");

    // manager.OnReachTarget += ReachedTarget;
  }

  public void ReachedTarget(BotController manager)
  {
    // pickedWeaponUp = true;

    if (nearestPlayer != null)
      manager.SwitchWeapon(nearestPlayer);
    // else pickedWeaponUp = false;
  }

  public override void ExitState(BotController manager)
  {
    // manager.OnReachTarget -= ReachedTarget;
  }

  bool haunt = false;

  public override void UpdateState(BotController manager)
  {
    nearestPlayer = Utilities.FindNearest(manager.transform, "Player");

    if (nearestPlayer != null)
    {
      Vector2 dir = (nearestPlayer.transform.position - manager.transform.position).normalized;

      RaycastHit2D rh = Physics2D.Raycast(manager.transform.position, dir, Mathf.Infinity, manager.playerLm);

      /*manager.botVars.botWb.transform.localRotation*/

      if (rh.collider == null)
      {
        haunt = true;
        return;
      }

      if (rh.collider.gameObject.tag == "Player")
      {
        haunt = false;

        manager.path = null;
        manager.currentWaypoint = 0;

        if (dir.x > 0)
        {
          manager.botVars.graphics.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else
        {
          manager.botVars.graphics.transform.rotation = Quaternion.Euler(0, 180, 0);
        }

        Vector2 playerPos = rh.collider.transform.position;

        playerPos.x -= manager.transform.position.x;
        playerPos.y -= manager.transform.position.y;

        float angle = Mathf.Atan2(playerPos.y, playerPos.x) * Mathf.Rad2Deg;
        // manager.botVars.botWb.transform.LookAt(nearestPlayer.transform);

        manager.Shoot(playerPos.x, angle);

        if (manager.botVars.botWb.weapon.bulletsInMag <= 0) manager.SwitchState(manager.botFleeState);
      }
      else
      {
        haunt = true;
      }
    }
  }

  public override void CalculatePath(BotController manager)
  {
    if (nearestPlayer != null && haunt)
      manager.seeker.StartPath(manager.transform.position, nearestPlayer.transform.position);
  }

  public override float Timer() => 0.25f;
}
