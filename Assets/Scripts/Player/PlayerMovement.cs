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

  WeaponBehavior weaponBehavior;

  void Start()
  {
    playerVars = GetComponent<PlayerVars>();

    playerLm = LayerMask.GetMask("Player");

    weaponBehavior = GetComponentInChildren<WeaponBehavior>();

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

    Vector3 difference = Camera.main.ScreenToWorldPoint(Input.mousePosition) - weaponBehavior.gameObject.transform.position;
    difference.Normalize();

    playerVars.graphics.transform.rotation = Quaternion.Euler(0, difference.x < 0 ? 180 : 0, 0);

    float rotation_z = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
    weaponBehavior.gameObject.transform.rotation = Quaternion.Euler(0f, 0f, rotation_z);

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
    Rigidbody2D rb = playerObj.GetComponent<Rigidbody2D>();

    if ((PlayersOnLeft(playerObj, bc) && xRaw == -1) || (PlayersOnRight(playerObj, bc) && xRaw == 1)) x = 0;

    rb.AddForce(new Vector2(x * Time.deltaTime * moveForce, 0), ForceMode2D.Impulse);

    // if (xRaw != 0) playerVars.graphics.transform.rotation = Quaternion.Euler(0, xRaw == -1 ? 180 : 0, 0);

    LimitVelocity(xRaw, rb);
  }

  void LimitVelocity(float x, Rigidbody2D rb)
  {
    // Add drag
    if (rb.velocity.x != 0) rb.velocity = new Vector2(rb.velocity.x * (1 - 0.2f), rb.velocity.y);

    float speed = rb.velocity.x;
    if (speed > maxSpeedX)
    {
      float brakeSpeed = speed - maxSpeedX;  // calculate the speed decrease

      Vector3 normalisedVelocity = rb.velocity.normalized;
      Vector3 brakeVelocity = normalisedVelocity * brakeSpeed;  // make the brake Vector3 value

      rb.AddForce(new Vector2(-brakeVelocity.x, 0), ForceMode2D.Impulse);
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
