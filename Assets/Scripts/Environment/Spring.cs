using Mirror;
using UnityEngine;

public class Spring : NetworkBehaviour
{
  private Animator anim;
  private bool extended = false;

  // Start is called before the first frame update
  private void Start()
  {
    anim = GetComponent<Animator>();
  }

  private GameObject target;

  private void OnTriggerEnter2D(Collider2D other)
  {
    if (other.gameObject.tag is not "Player" and not "Bot")
    {
      return;
    }

    if (extended)
    {
      return;
    }

    target = other.gameObject;

    extended = true;

    anim.Play("Extend");
  }

  private void OnTriggerStay2D(Collider2D other)
  {
    if (other.gameObject.tag is not "Player" and not "Bot")
    {
      return;
    }

    if (extended)
    {
      return;
    }

    target = other.gameObject;

    extended = true;

    anim.Play("Extend");
  }

  private void AddForce()
  {
    target.GetComponent<Rigidbody2D>().velocity = 40f * Vector2.up;
  }

  private void Update()
  {
    // if (Utilities.AnimatorStateDonePlaying(anim, "Contract")) extended = false;
    if (anim.GetCurrentAnimatorStateInfo(0).IsName("Empty"))
    {
      extended = false;
    }
    // if (anim.GetCurrentAnimatorStateInfo(0).("Contract")) extended = false;
  }
}
