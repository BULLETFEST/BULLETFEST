public class BotWeaponBehavior : WeaponBehavior
{
  private void Start()
  {
    arsenal = FindObjectOfType<PlayerBehavior>().gameObject.GetComponentInChildren<WeaponBehavior>().arsenal;
  }
}
