using Mirror;
using Pathfinding;
using UnityEngine;

public class BotPathfinding : NetworkBehaviour
{
  [Header("Physics")]
  public float speed = 13f;
  public float drag = 17.5f;
  public float nextWaypointDistance = 3f;
  public float jumpNodeHeightRequirement = 0.8f;
  public float jumpForce = 20f;
  public float jumpCheckOffset = 0.1f;
  public LayerMask groundLm, playerLm;

  [SerializeField]
  private bool enablePathfinding = true;

  [HideInInspector] public int currentWaypoint = 0;
  [HideInInspector] public Path path;
  [HideInInspector] public BotVars botVars;
  [HideInInspector] public Seeker seeker;
  [HideInInspector] public bool AvoidOthers = false;

  private RaycastHit2D isGrounded;
  private BotBaseState currentState;
  private bool dead;

  public BotFleeState botFleeState = new();
  public BotLookForWeaponState botLookForWeaponState = new();
  public BotHauntPlayerState botHauntPlayerState = new();

  public System.Action<BotPathfinding> OnReachTarget;

  public override void OnStartServer()
  {
    base.OnStartServer();
  }

  private void Start()
  {
    if (!isServer)
    {
      return;
    }

    seeker = GetComponent<Seeker>();

    botVars = GetComponent<BotVars>();

    currentState = botFleeState;
    currentState.EnterState(this);

    seeker.pathCallback += OnPathComplete;
  }

  private void OnDestroy()
  {
    seeker.pathCallback -= OnPathComplete;
  }

  private void Update()
  {
    if (Time.timeScale == 0 || !isServer || botVars.damageController.dead || !enablePathfinding)
    {
      return;
    }

    if (!botVars.lockMovement)
    {
      PathFollow();
    }

    currentState.UpdateState(this);
  }

  public void SwitchState(BotBaseState state)
  {
    currentState.ExitState(this);
    currentState = state;
    currentState.EnterState(this);

    CancelInvoke("UpdatePath");
    InvokeRepeating("UpdatePath", 1f, state.Timer());
  }

  private void UpdatePath()
  {
    if (Time.timeScale == 0 || botVars.lockMovement || !isServer || !enablePathfinding)
    {
      return;
    }

    if (path == null || seeker.IsDone())
    {
      currentState.CalculatePath(this);
    };
  }

  private bool doubleJumped = false;

  private void PathFollow()
  {
    if (path == null || botVars.lockMovement)
    {
      // Add drag
      // https://forum.unity.com/threads/physics-drag-formula.252406/
      botVars.rb.velocity = new Vector2(botVars.rb.velocity.x * (1 - (Time.deltaTime * drag)), botVars.rb.velocity.y);

      return;
    }

    // Reached end of path
    if (currentWaypoint >= path.vectorPath.Count)
    {
      OnReachTarget?.Invoke(this);
      path = null;
      currentState.CalculatePath(this);
      return;
    }

    // See if colliding with anything
    isGrounded = Physics2D.BoxCast(
      transform.position,
      botVars.bc.bounds.size, 0, Vector2.down,
      0.05f, groundLm);

    if (isGrounded && doubleJumped)
    {
      doubleJumped = false;
    }

    // Direction Calculation
    Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - botVars.rb.position).normalized;

    bool hasObstacle = Physics2D.BoxCast(transform.position, botVars.bc.bounds.size, 0, transform.right, Random.Range(0.01f, 0.5f), playerLm);

    // Jump
    if (direction.y > jumpNodeHeightRequirement || hasObstacle)
    {
      if (isGrounded && botVars.rb.velocity.y < 0.5f)
      {
        botVars.rb.AddForce(Vector2.up * jumpForce);//speed * jumpModifier);
      }
      else if (!doubleJumped && botVars.rb.velocity.y < 0.5f)
      {
        doubleJumped = true;

        botVars.rb.velocity = new Vector2(botVars.rb.velocity.x, 0);
        botVars.rb.AddForce(Vector2.up * jumpForce);

      }
    }

    // Movement
    if (Mathf.Abs(direction.x) >= 0.15f)
    {
      // Get Desired moving direction
      float targetSpeed = (direction.x > 0 ? 1 : -1) * speed;

      //Check difference between current speed and desired speed
      float speedDiff = targetSpeed - Mathf.Clamp(botVars.rb.velocity.x, -speed, speed);

      botVars.rb.AddForce(new Vector2(speedDiff, 0), ForceMode2D.Impulse); ;
    }

    // Add drag
    // https://forum.unity.com/threads/physics-drag-formula.252406/
    botVars.rb.velocity = new Vector2(botVars.rb.velocity.x * (1 - (Time.fixedDeltaTime * drag)), botVars.rb.velocity.y);


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

  private void OnPathComplete(Path p)
  {
    if (!p.error)
    {
      path = p;
      currentWaypoint = 0;
    }
  }
}
