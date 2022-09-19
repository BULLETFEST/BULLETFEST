using UnityEngine;
using Pathfinding;

public abstract class BotBaseState
{
  public abstract void EnterState(BotController manager);
  public abstract void ExitState(BotController manager);
  public abstract void UpdateState(BotController manager);
  public abstract void CalculatePath(BotController manager);

  public abstract float Timer();
  // public abstract void ();
}
