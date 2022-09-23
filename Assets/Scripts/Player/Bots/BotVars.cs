using UnityEngine;
using TMPro;

public class BotVars : MonoBehaviour
{
  [HideInInspector] public Rigidbody2D rb;
  [HideInInspector] public BoxCollider2D bc;
  [HideInInspector] public BotWeaponBehavior botWb;
  [HideInInspector] public SpriteGroup graphics;
  [HideInInspector] public BotBehavior botBehavior;

  public TMP_Text uiName;

  [HideInInspector]
  public bool lockMovement = false,
            lockShooting = false,
            lockWeapon = false;

  void Start()
  {
    rb = GetComponent<Rigidbody2D>();
    bc = GetComponent<BoxCollider2D>();
    botWb = GetComponentInChildren<BotWeaponBehavior>();
    graphics = GetComponentInChildren<SpriteGroup>();
    botBehavior = GetComponent<BotBehavior>();
  }
}