using UnityEngine;

public class BotVars : MonoBehaviour
{
  [HideInInspector] public Rigidbody2D rb;
  [HideInInspector] public BoxCollider2D bc;
  [HideInInspector] public BotWeaponBehaviour botWb;
  [HideInInspector] public SpriteGroup graphics;


  [HideInInspector]
  public bool lockMovement = false,
            lockShooting = false,
            lockWeapon = false;

  void Start()
  {
    rb = GetComponent<Rigidbody2D>();
    bc = GetComponent<BoxCollider2D>();
    botWb = GetComponentInChildren<BotWeaponBehaviour>();
    graphics = GetComponentInChildren<SpriteGroup>();
  }
}