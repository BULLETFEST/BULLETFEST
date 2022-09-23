public abstract class BotBaseState
{
  public abstract void EnterState(BotPathfinding manager);
  public abstract void ExitState(BotPathfinding manager);
  public abstract void UpdateState(BotPathfinding manager);
  public abstract void CalculatePath(BotPathfinding manager);

  public abstract float Timer();
}
