using UnityEngine;

public class BotRefs : ComponentRefs
{
  public BotBehavior botBehavior { get; private set; }

  protected override void Awake()
  {
    base.Awake();

    botBehavior = GetComponent<BotBehavior>();
  }
}