using Mirror;
using TMPro;
using UnityEngine;

public class ComponentRefs : NetworkBehaviour
{
  [HideInInspector] public Rigidbody2D rb;
  [HideInInspector] public BoxCollider2D bc;
  [HideInInspector] public WeaponBehavior weaponBehavior;
  [HideInInspector] public SpriteGroup graphics;
  [HideInInspector] public DamageController damageController;
  [HideInInspector] public NetworkAnimator weaponAnimator;
  public TMP_Text uiName;

  [SyncVar]
  [HideInInspector]
  public bool lockMovement = false,
            lockShooting = false,
            lockWeapon = false;

  protected virtual void Awake()
  {
    rb = GetComponent<Rigidbody2D>();
    bc = GetComponent<BoxCollider2D>();
    weaponBehavior = GetComponentInChildren<WeaponBehavior>();
    graphics = GetComponentInChildren<SpriteGroup>();
    damageController = GetComponent<DamageController>();
    weaponAnimator = GetComponent<NetworkAnimator>();
  }
}