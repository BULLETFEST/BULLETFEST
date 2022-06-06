using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class PlayerMovement : NetworkBehaviour
{

  #region Public Vars

  public LayerMask groundLm;
  public TextMeshProUGUI uiName;

  #endregion

  [SyncVar]
  float moveForce = 20;

  [SyncVar]
  float jumpForce = 15;

  [SyncVar]
  float maxSpeedX = 10;

  [HideInInspector]
  public bool lockMovement = false;

  bool isGrounded = false;

  float prevDir = 0;

  Rigidbody2D rb;
  BoxCollider2D bc;


  LayerMask playerLm;

  private void OnCollisionEnter2D(Collision2D other)
  {
    if (other.gameObject.layer == 31)
    {
      rb.velocity = Vector3.ProjectOnPlane(rb.velocity, other.contacts[0].normal);
    }
  }

  [Command]
  void Server_InitializePlayer()
  {
    string name = PlayerPrefs.GetString("name");
    uiName.text = name;

    ClientRpc_InitializePlayer(name);
  }

  [ClientRpc]
  void ClientRpc_InitializePlayer(string name)
  {
    uiName.text = name;
  }

  void Start()
  {
    rb = GetComponent<Rigidbody2D>();
    bc = GetComponent<BoxCollider2D>();

    playerLm = LayerMask.GetMask("Player");


    // Due To Time.DeltaTime multiplication, speeds must be 
    // Multiplied by 100.
    moveForce *= 100;
    jumpForce *= 100;

    if (isLocalPlayer)
      gameObject.layer = 30;

    Server_InitializePlayer();
  }

  void Update()
  {
    if (!isLocalPlayer) return;
    if (lockMovement) return;

    float x = Input.GetAxis("Horizontal");
    float xRaw = Input.GetAxisRaw("Horizontal");

    ValidateMovement(x, xRaw);
  }

  [Command]
  void ValidateMovement(float x, float xRaw)
  {
    GameObject playerObj = gameObject;
    x = Mathf.Clamp(x, -1, 1);
    xRaw = Mathf.Clamp(xRaw, -1, 1);

    HandleMovement(playerObj, x, xRaw);
  }

  [ClientRpc]
  void HandleMovement(GameObject playerObj, float x, float xRaw)
  {

    BoxCollider2D bc = playerObj.GetComponent<BoxCollider2D>();

    if (Input.GetKeyDown(KeyCode.Space) && Grounded(playerObj, bc))
    {
      rb.AddForce(new Vector2(0, jumpForce));
    }

    if ((PlayersOnLeft(playerObj, bc) && xRaw == -1) || (PlayersOnRight(playerObj, bc) && xRaw == 1)) x = 0;

    playerObj.GetComponent<Rigidbody2D>().AddForce(new Vector2(x * Time.deltaTime * moveForce, 0), ForceMode2D.Impulse);


    LimitVelocity(xRaw, playerObj.GetComponent<Rigidbody2D>());
  }

  void LimitVelocity(float x, Rigidbody2D rb)
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
