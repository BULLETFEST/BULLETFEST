using UnityEngine;
using Pathfinding;

public abstract class EnemyBaseState
{
  public abstract void EnterState(EnemyAI manager);
  public abstract void ExitState(EnemyAI manager);
  public abstract void UpdateState(EnemyAI manager);
  public abstract Path CalculatePath(EnemyAI manager);
  // public abstract void ();
}
