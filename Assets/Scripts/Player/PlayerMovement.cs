using Mirror;
using UnityEngine;

// This one is required :)
using System.Linq;

public class PlayerMovement : NetworkBehaviour
{
  [SerializeField]
  private LayerMask groundLm;

  [SerializeField]
  [SyncVar]
  private float moveForce = 13f,
                drag = 5f,
                jumpForce = 1500f;
  private bool doubleJumped;

  private PlayerRefs playerRefs;
  private LayerMask playerLm;

  private void Start()
  {
    playerRefs = GetComponent<PlayerRefs>();

    playerLm = LayerMask.GetMask("Player");
  }

  private void LateUpdate()
  {
    if (!isLocalPlayer)
    {
      return;
    }

    if (!NetworkClient.ready)
    {
      return;
    }

    if (Time.timeScale == 0)
    {
      return;
    }


#if UNITY_IOS || UNITY_ANDROID
    if (playerRefs.weapon != null)
    {
      GameObject nearestPlayer;
      nearestPlayer = Utilities.FindNearest(transform, FindObjectsOfType<DamageController>().Where(x => x.gameObject != gameObject && !x.dead).ToArray());

      Vector2 playerPos = nearestPlayer.transform.position;

      playerPos.x -= transform.position.x;
      playerPos.y -= transform.position.y;

      float angle = Mathf.Atan2(playerPos.y, playerPos.x) * Mathf.Rad2Deg;

      Cmd_UpdateGun(Quaternion.Euler(0, playerPos.x < 0 ? 180 : 0, 0),
                    Quaternion.Euler(playerPos.x < 0 ? 180 : 0, playerPos.x < 0 ? 180 : 0, (playerPos.x < 0 ? -1 : 1) * angle),
                    Quaternion.Euler(0, playerPos.x < 0 ? 180 : 0, 0));
    }
#else
    Vector3 mousePos = Input.mousePosition;
    mousePos.z = 5.23f;

    Vector3 objectPos = Camera.main.WorldToScreenPoint(transform.position);
    mousePos.x -= objectPos.x;
    mousePos.y -= objectPos.y;

    float angle = Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg;

    Cmd_UpdateGun(Quaternion.Euler(0, mousePos.x < 0 ? 180 : 0, 0),
                  Quaternion.Euler(mousePos.x < 0 ? 180 : 0, mousePos.x < 0 ? 180 : 0, (mousePos.x < 0 ? -1 : 1) * angle),
                  Quaternion.Euler(0, mousePos.x < 0 ? 180 : 0, 0));
#endif

  }

  [Command]
  private void Cmd_UpdateGun(Quaternion graphicsRotation, Quaternion gunRotation, Quaternion globalGunRotation)
  {
    if (playerRefs.lockWeapon)
    {
      return;
    }

    Rpc_UpdateGun(graphicsRotation, gunRotation, globalGunRotation);
  }

  [ClientRpc]
  private void Rpc_UpdateGun(Quaternion graphicsRotation, Quaternion gunRotation, Quaternion globalGunRotation)
  {
    if (playerRefs == null)
    {
      return;
    }

    playerRefs.graphics.transform.rotation = graphicsRotation;

    if (playerRefs.weapon == null) return;

    playerRefs.transform.localRotation = gunRotation;

    if (playerRefs.graphics.sprites.Count >= 4)
    {
      if (!playerRefs.weapon.rotateWithCursor)
      {
        playerRefs.graphics.sprites[3].gameObject.transform.rotation = globalGunRotation;
      }
    }
  }

  public void SetDir(int dir)
  {
    xRaw = dir;
  }

  int xRaw = 0;
  bool grounded = false;

  private void Update()
  {
    if (!isLocalPlayer)
    {
      return;
    }

    if (playerRefs.lockMovement)
    {
      return;
    }

    // if (Input.GetKeyDown(KeyCode.X)) GetComponent<DamageController>().TakeDamage(5f, null);
#if !UNITY_IOS && !UNITY_ANDROID
    xRaw = 0;
    grounded = false;
    if (Utilities.GetKeybind("lft") && Utilities.GetKeybind("rgt"))
    {
      xRaw = 0;
    }
    else if (Utilities.GetKeybind("lft"))
    {
      xRaw = -1;
    }
    else if (Utilities.GetKeybind("rgt"))
    {
      xRaw = 1;
    }
#endif

    xRaw = Mathf.Clamp(xRaw, -1, 1);

    if (SaveSystem.saveData.settings.invertControls)
    {
      xRaw *= -1;
    }

    if (playerRefs.bc != null)
    {
      grounded = Utilities.Grounded(transform, playerRefs.bc, groundLm).collider != null;
    }

    if (Utilities.GetKeybindDown("jump"))
    {
      Jump();
    }

    if (doubleJumped && grounded)
    {
      doubleJumped = false;
    }

    ValidateMovement(xRaw);
    HandleMovement(xRaw);
  }

  public void Jump()
  {
    if (!grounded && !doubleJumped)
    {
      doubleJumped = true;
      playerRefs.rb.velocity = new Vector2(playerRefs.rb.velocity.x, 0);
      if (playerRefs.rb.velocity.y > 0)
      {
        playerRefs.rb.AddForce(new Vector2(0, jumpForce * 0.75f));
      }
      else
      {
        playerRefs.rb.AddForce(new Vector2(0, jumpForce));
      }
    }
    else if (grounded)
    {
      playerRefs.rb.AddForce(new Vector2(0, jumpForce));
    }
    else return;

    AudioSystem.Instance.PlaySound("Jump");
  }


  [Command]
  private void ValidateMovement(float xRaw)
  {
    if (playerRefs.lockMovement)
    {
      return;
    }

    if (Time.timeScale == 0)
    {
      return;
    }

    if (transform.position.y is <= (-15) or >= 50)
    {
      GetComponent<DamageController>().TakeDamage(9999999, gameObject);
    }

    xRaw = Mathf.Clamp(xRaw, -1, 1);

    HandleMovement(xRaw);
  }

  private void HandleMovement(float xRaw)
  {
    if (playerRefs == null)
    {
      return;
    }

    if ((PlayersOnLeft(gameObject, playerRefs.bc) && xRaw == -1) || (PlayersOnRight(gameObject, playerRefs.bc) && xRaw == 1))
    {
      xRaw = 0;
    }

    if (xRaw != 0)
    {
      // Get Desired moving direction
      float targetSpeed = moveForce * xRaw;

      //Check difference between current speed and desired speed
      float speedDiff = targetSpeed - Mathf.Clamp(playerRefs.rb.velocity.x, -moveForce, moveForce);

      playerRefs.rb.AddForce(new Vector2(speedDiff, 0), ForceMode2D.Impulse);
    }

    // Add drag
    // https://forum.unity.com/threads/physics-drag-formula.252406/
    playerRefs.rb.velocity = new Vector2(playerRefs.rb.velocity.x * (1 - (Time.fixedDeltaTime * (Utilities.Grounded(transform, playerRefs.bc, groundLm).collider != null ? drag * 1.15f : drag))), playerRefs.rb.velocity.y);
  }

  private bool PlayersOnRight(GameObject player, BoxCollider2D bc)
  {
    RaycastHit2D ray = Physics2D.BoxCast(
      player.transform.position,
      bc.bounds.size, 0, Vector2.right,
      0.3f, playerLm);

    return ray.collider != null;
  }

  private bool PlayersOnLeft(GameObject player, BoxCollider2D bc)
  {
    RaycastHit2D ray = Physics2D.BoxCast(
      player.transform.position,
      bc.bounds.size, 0, Vector2.left,
      0.3f, playerLm);

    return ray.collider != null;
  }
}
