public class BotWeaponBehavior : WeaponBehavior
{
  private void Start()
  {
    arsenal = FindFirstObjectByType<PlayerBehavior>().gameObject.GetComponentInChildren<WeaponBehavior>().arsenal;
  }
}
