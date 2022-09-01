using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Spring : NetworkBehaviour
{
  Animator anim;

  bool extended = false;

  // Start is called before the first frame update
  void Start()
  {
    anim = GetComponent<Animator>();
  }

  GameObject target;

  private void OnTriggerEnter2D(Collider2D other)
  {
    if (other.gameObject.tag != "Player") return;
    if (extended) return;

    target = other.gameObject;

    extended = true;

    anim.Play("Extend");
  }

  private void OnTriggerStay2D(Collider2D other)
  {
    if (other.gameObject.tag != "Player") return;
    if (extended) return;

    target = other.gameObject;

    extended = true;

    anim.Play("Extend");
  }

  void AddForce()
  {
    target.GetComponent<Rigidbody2D>().AddForce(2500f * Vector2.up);
  }

  void Update()
  {
    // if (Utilities.AnimatorStateDonePlaying(anim, "Contract")) extended = false;
    if (anim.GetCurrentAnimatorStateInfo(0).IsName("Empty")) extended = false;
    // if (anim.GetCurrentAnimatorStateInfo(0).("Contract")) extended = false;
  }
}
