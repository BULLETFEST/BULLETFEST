public class BotWeaponBehavior : WeaponBehavior
{
  private BotRefs botRefs;

  private void Start()
  {
    arsenal = FindObjectOfType<PlayerBehavior>().gameObject.GetComponentInChildren<WeaponBehavior>().arsenal;
    botRefs = GetComponentInParent<BotRefs>();
  }

  // public override void SwitchWeapon(string weaponID)
  // {
  //   if (weapon != null)
  //   {
  //     Destroy(weapon.gameObject);
  //   }

  //   GameObject newWeapon = Instantiate(arsenal.Where(w => w.ID == weaponID).ToArray()[0].gameObject, transform.position, transform.rotation, transform);
  //   weapon = newWeapon.GetComponent<WeaponClass>();
  //   weapon.bulletsInMag = weapon.magazineSize;
  //   weapon.fireTimeout = 0;

  //   if (botRefs.graphics.sprites.Count > 2)
  //   {
  //     botRefs.graphics.sprites.RemoveAt(2);
  //   }

  //   botRefs.graphics.sprites.Add(newWeapon.GetComponentInChildren<SpriteRenderer>());

  //   if (weapon.animateOnShot)
  //   {
  //     botRefs.weaponAnimator.animator = weapon.GetComponent<Animator>();
  //   }
  // }
}
