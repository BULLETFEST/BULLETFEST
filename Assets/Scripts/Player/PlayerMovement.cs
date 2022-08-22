using UnityEngine;
using Mirror;

public class PlayerMovement : NetworkBehaviour
{

  #region Public Vars

  public LayerMask groundLm;

  #endregion

  [SerializeField]
  [SyncVar]
  private float moveForce = 13f,
                drag = 5f,
                jumpForce = 1500f;

  bool doubleJumped;

  // [SyncVar]
  // float maxSpeedX = 10;

  public PlayerVars playerVars;

  LayerMask playerLm;

  WeaponBehavior weaponBehavior;

  CharacterController characterController;

  void Start()
  {
    playerVars = GetComponent<PlayerVars>();

    playerLm = LayerMask.GetMask("Player");

    weaponBehavior = GetComponentInChildren<WeaponBehavior>();

    characterController = GetComponent<CharacterController>();
  }


  private void LateUpdate()
  {

    if (!isLocalPlayer) return;
    if (!NetworkClient.ready) return;

    Vector3 mousePos = Input.mousePosition;
    mousePos.z = 5.23f;

    Vector3 objectPos = Camera.main.WorldToScreenPoint(transform.position);
    mousePos.x -= objectPos.x;
    mousePos.y -= objectPos.y;

    float angle = Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg;

    Cmd_UpdateGun(Quaternion.Euler(0, mousePos.x < 0 ? 180 : 0, 0), Quaternion.Euler(mousePos.x < 0 ? 180 : 0, mousePos.x < 0 ? 180 : 0, (mousePos.x < 0 ? -1 : 1) * angle));
  }

  [Command]
  void Cmd_UpdateGun(Quaternion graphicsRotation, Quaternion gunRotation)
  {
    if (playerVars.lockWeapon) return;

    Rpc_UpdateGun(graphicsRotation, gunRotation);
  }

  [ClientRpc]
  void Rpc_UpdateGun(Quaternion graphicsRotation, Quaternion gunRotation)
  {
    if (playerVars == null) return;

    playerVars.graphics.transform.rotation = graphicsRotation;

    playerVars.weaponBehavior.transform.localRotation = gunRotation;
  }

  void Update()
  {
    if (!isLocalPlayer) return;
    if (playerVars.lockMovement) return;

    int xRaw = 0;
    if (Utilities.GetKeybind("lft") && Utilities.GetKeybind("rgt")) xRaw = 0;
    else if (Utilities.GetKeybind("lft")) xRaw = -1;
    else if (Utilities.GetKeybind("rgt")) xRaw = 1;

    if (SaveSystem.saveData.settings.invertControls) xRaw *= -1;

    bool grounded = false;
    if (playerVars.bc != null)
      grounded = Grounded(gameObject, playerVars.bc);

    if (Utilities.GetKeybindDown("jump") && (grounded || !doubleJumped))
    {
      if (!grounded)
      {
        doubleJumped = true;
        playerVars.rb.velocity = new Vector2(playerVars.rb.velocity.x, 0);
        if (playerVars.rb.velocity.y > 0)
        {
          playerVars.rb.AddForce(new Vector2(0, jumpForce * 0.75f));
          playerVars.audioSystem.PlaySound("Jump");
        }
        else
        {
          playerVars.rb.AddForce(new Vector2(0, jumpForce));
          playerVars.audioSystem.PlaySound("Jump");
        }
      }
      else
      {
        playerVars.rb.AddForce(new Vector2(0, jumpForce));
        playerVars.audioSystem.PlaySound("Jump");
      }
    }

    if (doubleJumped && grounded)
    {
      doubleJumped = false;
    }

    ValidateMovement(xRaw);
  }


  [Command]
  void ValidateMovement(float xRaw)
  {
    if (playerVars.lockMovement) return;

    if (transform.position.y <= -15) GetComponent<PlayerBehavior>().TakeDamage(9999999, null);

    xRaw = Mathf.Clamp(xRaw, -1, 1);

    HandleMovement(xRaw);
  }

  [ClientRpc]
  void HandleMovement(float xRaw)
  {
    if (playerVars == null) return;

    if ((PlayersOnLeft(gameObject, playerVars.bc) && xRaw == -1) || (PlayersOnRight(gameObject, playerVars.bc) && xRaw == 1)) xRaw = 0;

    if (xRaw != 0)
    {
      // Get Desired moving direction
      float targetSpeed = moveForce * xRaw;

      //Check difference between current speed and desired speed
      float speedDiff = targetSpeed - Mathf.Clamp(playerVars.rb.velocity.x, -moveForce, moveForce);

      playerVars.rb.AddForce(new Vector2(speedDiff, 0), ForceMode2D.Impulse);
    }

    // Add drag
    // https://forum.unity.com/threads/physics-drag-formula.252406/
    playerVars.rb.velocity = new Vector2(playerVars.rb.velocity.x * (1 - Time.fixedDeltaTime * drag), playerVars.rb.velocity.y);

    // playerVars.audioSystem.transform.position = gameObject.transform.position;
  }

  bool Grounded(GameObject player, BoxCollider2D bc)
  {
    RaycastHit2D ray = Physics2D.BoxCast(
      player.transform.position,
      bc.bounds.size, 0, Vector2.down,
      0.25f, groundLm);

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
