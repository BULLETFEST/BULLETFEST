using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using Mirror;

public class BotController : NetworkBehaviour
{
  [Header("Pathfinding")]
  public Transform target;
  public float activateDistance = 50f;
  public float pathUpdateSeconds = 0.5f;

  [Header("Physics")]
  public float speed = 13f;
  public float drag = 17.5f;
  public float nextWaypointDistance = 3f;
  public float jumpNodeHeightRequirement = 0.8f;
  public float jumpForce = 20f;
  //   public float jumpModifier = 0.3f;
  public float jumpCheckOffset = 0.1f;
  public LayerMask groundLm, playerLm;

  [Header("Custom Behavior")]
  public bool followEnabled = true;
  public bool jumpEnabled = true;
  public bool directionLookEnabled = true;
  public float threshold = 0.15f;

  [HideInInspector] public int currentWaypoint = 0;
  [HideInInspector] public bool dead;
  [HideInInspector] public Path path;
  [HideInInspector] public BotVars botVars;
  [HideInInspector] public Seeker seeker;
  [HideInInspector] public static GameObject[] nodes;

  RaycastHit2D isGrounded;
  BotBaseState currentState;

  public BotFleeState botFleeState = new BotFleeState();
  public BotLookForWeaponState botLookForWeaponState = new BotLookForWeaponState();
  public BotHauntPlayerState botHauntPlayerState = new BotHauntPlayerState();

  public System.Action<BotController> OnReachTarget;

  public override void OnStartServer()
  {
    base.OnStartServer();

    if (nodes == null)
    {
      nodes = GameObject.FindGameObjectsWithTag("NavigationPoint");
    }
  }

  void Start()
  {
    seeker = GetComponent<Seeker>();

    botVars = GetComponent<BotVars>();


    // target = FindObjectOfType<PlayerBehavior>().transform;

    currentState = botFleeState;
    currentState.EnterState(this);

    seeker.pathCallback += OnPathComplete;

    InvokeRepeating("UpdatePath", 0f, pathUpdateSeconds);
  }

  void OnDestroy()
  {
    nodes = null;
    seeker.pathCallback -= OnPathComplete;
  }

  void FixedUpdate()
  {
    if (Time.timeScale == 0) return;

    currentState.UpdateState(this);
    if (followEnabled)
    {
      PathFollow();
    }
  }

  public void SwitchState(BotBaseState state)
  {
    print("STATE SWITCHED!");

    currentState.ExitState(this);
    currentState = state;
    currentState.EnterState(this);
  }

  private void UpdatePath()
  {
    if (Time.timeScale == 0) return;

    if (followEnabled && seeker.IsDone())
    {
      currentState.CalculatePath(this);
    };
  }

  bool doubleJumped = false;

  private void PathFollow()
  {
    if (path == null || botVars.lockMovement)
    {
      // Add drag
      // https://forum.unity.com/threads/physics-drag-formula.252406/
      botVars.rb.velocity = new Vector2(botVars.rb.velocity.x * (1 - Time.fixedDeltaTime * drag), botVars.rb.velocity.y);

      return;
    }

    // Reached end of path
    if (currentWaypoint >= path.vectorPath.Count)
    {
      OnReachTarget?.Invoke(this);
      path = null;
      return;
    }

    // See if colliding with anything
    Vector3 startOffset = transform.position - new Vector3(0f, GetComponent<Collider2D>().bounds.extents.y + jumpCheckOffset);
    isGrounded = Physics2D.BoxCast(
      transform.position,
      botVars.bc.bounds.size, 0, Vector2.down,
      0.1f, groundLm);

    if (isGrounded && doubleJumped) doubleJumped = false;

    // Direction Calculation
    Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - botVars.rb.position).normalized;
    Vector2 force = direction * speed;
    // print(direction.x);
    // if (direction.x < threshold) direction = new Vector2(0, direction.y);


    // Jump
    if ((direction.y > jumpNodeHeightRequirement))
    {
      if (isGrounded)
      {
        botVars.rb.AddForce(Vector2.up * jumpForce);//speed * jumpModifier);
      }
      else if (!doubleJumped && botVars.rb.velocity.y < 0.5f)
      {
        doubleJumped = true;
        print("DJUMP!");

        botVars.rb.AddForce(new Vector2(0, jumpForce));

      }
    }

    // print(direction);

    // Movement
    if (Mathf.Abs(direction.x) >= 0.45f)
    {
      // Get Desired moving direction
      float targetSpeed = (direction.x > 0 ? 1 : -1) * speed;

      // //Check difference between current speed and desired speed
      float speedDiff = targetSpeed - Mathf.Clamp(botVars.rb.velocity.x, -speed, speed);

      botVars.rb.AddForce(new Vector2(speedDiff, 0), ForceMode2D.Impulse);

      //Check difference between current speed and desired speed
      // float speedDiff = force.x - Mathf.Clamp(rb.velocity.x, -speed, speed);

      // rb.AddForce(new Vector2(speedDiff, 0), ForceMode2D.Impulse);
    }

    // Add drag
    // https://forum.unity.com/threads/physics-drag-formula.252406/
    botVars.rb.velocity = new Vector2(botVars.rb.velocity.x * (1 - Time.fixedDeltaTime * drag), botVars.rb.velocity.y);


    // Next Waypoint
    float distance = Vector2.Distance(botVars.rb.position, path.vectorPath[currentWaypoint]);
    if (distance < nextWaypointDistance)
    {
      currentWaypoint++;
    }

    if (botVars.rb.velocity.x > 0.05f)
    {
      botVars.graphics.transform.rotation = Quaternion.Euler(0, 0, 0);
    }
    else if (botVars.rb.velocity.x < -0.05f)
    {
      botVars.graphics.transform.rotation = Quaternion.Euler(0, 180, 0);
    }
  }

  private bool TargetInDistance()
  {
    return Vector2.Distance(transform.position, target.transform.position) < activateDistance;
  }

  private void OnPathComplete(Path p)
  {
    if (!p.error)
    {
      path = p;
      currentWaypoint = 0;
    }
  }

  // [Command]
  public void Shoot(float playerPosX, float angle)
  {
    WeaponClass weapon = botVars.botWb.weapon;

    if (weapon == null) return;
    if (weapon.fireTimeout > NetworkTime.time) return;
    if (weapon.bulletsInMag <= 0) return;

    botVars.botWb.transform.localRotation = Quaternion.Euler(playerPosX < 0 ? 180 : 0, playerPosX < 0 ? 180 : 0, (playerPosX < 0 ? -1 : 1) * angle + Random.Range(-15f, 15f));

    weapon.bulletsInMag--;
    weapon.fireTimeout = (float)NetworkTime.time + (1f / weapon.fireRate) * (weapon.firingMode == WeaponClass.FireMode.Single ? 2.1f : 1);

    Rpc_AddForce(gameObject, weapon.shootSound);
    botVars.botWb.Shoot(weapon.ID, gameObject);
  }

  [ClientRpc]
  void Rpc_AddForce(GameObject target, string shootSound)
  {
    botVars.botWb.AddForce(target);
    if (shootSound != "")
      FindObjectOfType<AudioSystem>().PlaySound(shootSound);
  }

  // [Command(requiresAuthority = false)]
  public void SwitchWeapon(GameObject weapon)
  {
    if (weapon != null && !botVars.lockMovement)
    {
      WeaponItem weaponItem = weapon.GetComponent<WeaponItem>();

      botVars.botWb.SwitchWeapon(weaponItem.WeaponID);
      TargetRpc_SwitchWeapon(weaponItem.WeaponID);
      NetworkServer.Destroy(weapon);
    }
  }

  [ClientRpc]
  void TargetRpc_SwitchWeapon(string WeaponID)
  {
    botVars.botWb.SwitchWeapon(WeaponID);
  }
}
