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
  public LayerMask groundLm;

  [Header("Custom Behavior")]
  public bool followEnabled = true;
  public bool jumpEnabled = true;
  public bool directionLookEnabled = true;
  public float threshold = 0.15f;

  private Path path;
  private int currentWaypoint = 0;
  RaycastHit2D isGrounded;
  BotBaseState currentState;
  BotVars botVars;

  [HideInInspector] public Seeker seeker;


  public BotFleeState enemyFleeState = new BotFleeState();
  public BotLookForWeaponState enemyLookForWeaponState = new BotLookForWeaponState();

  public static GameObject[] nodes;

  public System.Action<BotController> OnReachTarget;

  public override void OnStartServer()
  {
    base.OnStartServer();

    if (nodes == null)
    {
      nodes = GameObject.FindGameObjectsWithTag("NavigationPoint");
    }
  }

  private void OnDestroy()
  {
    nodes = null;
    seeker.pathCallback -= OnPathComplete;
  }

  public void Start()
  {
    seeker = GetComponent<Seeker>();

    botVars = GetComponent<BotVars>();


    // target = FindObjectOfType<PlayerBehavior>().transform;

    currentState = enemyFleeState;
    currentState.EnterState(this);

    seeker.pathCallback += OnPathComplete;

    InvokeRepeating("UpdatePath", 0f, pathUpdateSeconds);
  }



  private void FixedUpdate()
  {
    // target = Utilities.FindNearest(transform, "Player").transform;
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
    if (followEnabled && seeker.IsDone())
    {
      currentState.CalculatePath(this);
    };
  }

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

    // Direction Calculation
    Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - botVars.rb.position).normalized;
    Vector2 force = direction * speed;
    // print(direction.x);
    // if (direction.x < threshold) direction = new Vector2(0, direction.y);

    // Jump
    if (jumpEnabled && isGrounded)
    {
      if (direction.y > jumpNodeHeightRequirement)
      {
        botVars.rb.AddForce(Vector2.up * jumpForce);//speed * jumpModifier);
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

    // Direction Graphics Handling
    if (directionLookEnabled)
    {
      if (botVars.rb.velocity.x > 0.05f)
      {
        transform.localScale = new Vector3(-1f * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
      }
      else if (botVars.rb.velocity.x < -0.05f)
      {
        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
      }
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
  void Shoot()
  {
    WeaponClass weapon = botVars.botWb.weapon;

    if (weapon == null) return;
    if (weapon.fireTimeout > NetworkTime.time) return;
    if (weapon.bulletsInMag <= 0) return;

    weapon.bulletsInMag--;
    weapon.fireTimeout = (float)NetworkTime.time + (1f / weapon.fireRate);

    Rpc_AddForce(gameObject, weapon.shootSound);
    botVars.botWb.Shoot(weapon.ID, connectionToClient);
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

    // if (playerVars.reloadRoutine != null) 
  }
}
