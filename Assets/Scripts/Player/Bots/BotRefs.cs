using UnityEngine;

public class BotRefs : ComponentRefs
{
  [HideInInspector] public BotBehavior botBehavior;

  protected override void Awake()
  {
    base.Awake();

    botBehavior = GetComponent<BotBehavior>();
  }
}