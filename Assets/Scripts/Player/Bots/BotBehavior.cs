using Mirror;
using UnityEngine;

public class BotBehavior : Behavior
{

  protected override void Update()
  {
    if (!isServer)
    {
      return;
    }

    base.Update();
  }

  [Server]
  public void Fire(float playerPosX, float angle)
  {
    if (componentRefs.weaponBehavior.awaitingDetonation.Count >= 3)
    {
      foreach (Explosive explosive in componentRefs.weaponBehavior.awaitingDetonation)
      {
        explosive.Detonate();
      }

      componentRefs.weaponBehavior.awaitingDetonation.Clear();

      return;
    }

    componentRefs.weaponBehavior.transform.localRotation = Quaternion.Euler(playerPosX < 0 ? 180 : 0, playerPosX < 0 ? 180 : 0, ((playerPosX < 0 ? -1 : 1) * angle) + Random.Range(-25f, 25f));

    base.Cmd_Fire();
  }

  [Command(requiresAuthority = false)]
  public void SwitchWeapon(GameObject weapon) => base.Cmd_SwitchWeapon(weapon);
}
