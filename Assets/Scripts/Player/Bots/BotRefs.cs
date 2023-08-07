using System.Linq;

public class BotRefs : ComponentRefs
{
  public BotBehavior botBehavior { get; private set; }
  public int botId { get; private set; }

  protected override void Awake()
  {
    base.Awake();

    botBehavior = GetComponent<BotBehavior>();

    botId = int.Parse(string.Join("", gameObject.name.Where(char.IsDigit)));
  }


}