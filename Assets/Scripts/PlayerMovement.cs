using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerMovement : NetworkBehaviour
{
  public float moveForce = 20;
  public float jumpForce = 20;
  public float maxSpeedX = 10;
  public float maxSpeedY = 20;

  float prevDir = 0;

  Rigidbody2D rb;

  void Start()
  {
    rb = GetComponent<Rigidbody2D>();


    // Due To Time.DeltaTime multiplication, speeds must be 
    // Multiplied by 100.
    // moveForce *= 100;
    // jumpForce *= 100;
  }

  void Update()
  {
    if (!isLocalPlayer) return;

    if (Input.GetKeyDown(KeyCode.Space))
    {
      rb.AddForce(new Vector2(0, jumpForce));
    }

    HandleMovement();


  }

  // [Command]
  void HandleMovement()
  {
    float x = Input.GetAxis("Horizontal");
    float xRaw = Input.GetAxisRaw("Horizontal");

    rb.AddForce(new Vector2(x * Time.deltaTime * moveForce, 0), ForceMode2D.Impulse); //.velocity = new Vector2(x * Time.deltaTime * speed, rb.velocity.y);


    LimitVelocity(xRaw);
  }

  void LimitVelocity(float x)
  {
    if (Mathf.Abs(rb.velocity.x) > maxSpeedX)
    {
      rb.velocity = new Vector2(maxSpeedX * x, rb.velocity.y);
    }

    // if (Mathf.Abs(rb.velocity.y) > maxSpeedY)
    // {
    //   rb.velocity = new Vector2(rb.velocity.x, maxSpeedY);
    // }
  }
}
