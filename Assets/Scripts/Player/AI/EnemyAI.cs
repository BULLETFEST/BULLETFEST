using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class EnemyAI : MonoBehaviour
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
  Seeker seeker;
  Rigidbody2D rb;
  BoxCollider2D bc;

  public void Start()
  {
    seeker = GetComponent<Seeker>();
    rb = GetComponent<Rigidbody2D>();
    bc = GetComponent<BoxCollider2D>();

    InvokeRepeating("UpdatePath", 0f, pathUpdateSeconds);

    target = FindObjectOfType<PlayerBehavior>().transform;
  }

  private void FixedUpdate()
  {
    if (TargetInDistance() && followEnabled)
    {
      PathFollow();
    }
  }

  private void UpdatePath()
  {
    if (followEnabled && TargetInDistance() && seeker.IsDone())
    {
      seeker.StartPath(rb.position, target.position, OnPathComplete);
    }
  }

  private void PathFollow()
  {
    if (path == null)
    {
      return;
    }

    // Reached end of path
    if (currentWaypoint >= path.vectorPath.Count)
    {
      return;
    }

    // See if colliding with anything
    Vector3 startOffset = transform.position - new Vector3(0f, GetComponent<Collider2D>().bounds.extents.y + jumpCheckOffset);
    isGrounded = Physics2D.BoxCast(
      transform.position,
      bc.bounds.size, 0, Vector2.down,
      0.1f, groundLm);

    // Direction Calculation
    Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
    // Vector2 force = direction * speed;// * Time.deltaTime;

    // print(direction.x);
    // if (direction.x < threshold) direction = new Vector2(0, direction.y);

    // Jump
    if (jumpEnabled && isGrounded)
    {
      if (direction.y > jumpNodeHeightRequirement)
      {
        rb.AddForce(Vector2.up * jumpForce);//speed * jumpModifier);
      }
    }

    // Movement
    if (direction != Vector2.zero)
    {
      // Get Desired moving direction
      Vector2 targetSpeed = direction * speed;

      //Check difference between current speed and desired speed
      Vector2 speedDiff = new Vector2(targetSpeed.x - Mathf.Clamp(rb.velocity.x, -speed, speed), targetSpeed.y);

      rb.AddForce(speedDiff, ForceMode2D.Impulse);
    }

    // Add drag
    // https://forum.unity.com/threads/physics-drag-formula.252406/
    rb.velocity = new Vector2(rb.velocity.x * (1 - Time.fixedDeltaTime * drag), rb.velocity.y);


    // Next Waypoint
    float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);
    if (distance < nextWaypointDistance)
    {
      currentWaypoint++;
    }

    // Direction Graphics Handling
    if (directionLookEnabled)
    {
      if (rb.velocity.x > 0.05f)
      {
        transform.localScale = new Vector3(-1f * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
      }
      else if (rb.velocity.x < -0.05f)
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
}
