using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerMovement : NetworkBehaviour
{

  #region Public Vars

  public LayerMask groundLm;

  #endregion

  [SyncVar]
  float moveForce = 20;

  [SyncVar]
  float jumpForce = 15;

  [SyncVar]
  float maxSpeedX = 10;

  PlayerVars playerVars;

  LayerMask playerLm;

  void Start()
  {
    playerVars = GetComponent<PlayerVars>();

    playerLm = LayerMask.GetMask("Player");

    if (isLocalPlayer)
    {
      // Due To Time.DeltaTime multiplication, speeds must be 
      // Multiplied by 100.
      moveForce *= 100;
      jumpForce *= 100;
    }
  }


  void Update()
  {
    if (!isLocalPlayer) return;
    if (playerVars.lockMovement) return;

    float x = Input.GetAxis("Horizontal");
    float xRaw = Input.GetAxisRaw("Horizontal");

    ValidateMovement(x, xRaw);
  }

  void FixedUpdate()
  {
    if (!isLocalPlayer) return;
    if (playerVars.lockMovement) return;


    if (Input.GetKey(KeyCode.Space) && Grounded(gameObject, playerVars.bc))
    {
      playerVars.rb.AddForce(new Vector2(0, jumpForce));
    }
  }


  [Command]
  void ValidateMovement(float x, float xRaw)
  {
    GameObject playerObj = this.gameObject;
    x = Mathf.Clamp(x, -1, 1);
    xRaw = Mathf.Clamp(xRaw, -1, 1);

    HandleMovement(playerObj, x, xRaw, playerVars.lockMovement);
  }

  [ClientRpc]
  void HandleMovement(GameObject playerObj, float x, float xRaw, bool lockMovement)
  {
    BoxCollider2D bc = playerObj.GetComponent<BoxCollider2D>();

    if ((PlayersOnLeft(playerObj, bc) && xRaw == -1) || (PlayersOnRight(playerObj, bc) && xRaw == 1)) x = 0;

    playerObj.GetComponent<Rigidbody2D>().AddForce(new Vector2(x * Time.deltaTime * moveForce, 0), ForceMode2D.Impulse);

    if (xRaw != 0) playerVars.graphics.transform.rotation = Quaternion.Euler(0, xRaw == -1 ? 180 : 0, 0);

    LimitVelocity(xRaw, playerObj.GetComponent<Rigidbody2D>(), lockMovement);
  }

  void LimitVelocity(float x, Rigidbody2D rb, bool lockMovement)
  {
    if (Mathf.Abs(rb.velocity.x) > maxSpeedX)
    {
      rb.velocity = new Vector2(maxSpeedX * x, rb.velocity.y);
    }

    if (x == 0 && !lockMovement)
    {
      rb.velocity = new Vector2(0, rb.velocity.y);
    }

  }

  bool Grounded(GameObject player, BoxCollider2D bc)
  {
    RaycastHit2D ray = Physics2D.BoxCast(
      player.transform.position,
      bc.bounds.size, 0, Vector2.down,
      0.1f, groundLm);

    return ray.collider != null;
  }

  bool PlayersOnRight(GameObject player, BoxCollider2D bc)
  {
    RaycastHit2D ray = Physics2D.BoxCast(
      player.transform.position,
      bc.bounds.size, 0, Vector2.right,
      0.3f, playerLm);

    return ray.collider != null;
  }

  bool PlayersOnLeft(GameObject player, BoxCollider2D bc)
  {
    RaycastHit2D ray = Physics2D.BoxCast(
      player.transform.position,
      bc.bounds.size, 0, Vector2.left,
      0.3f, playerLm);

    return ray.collider != null;
  }
}
