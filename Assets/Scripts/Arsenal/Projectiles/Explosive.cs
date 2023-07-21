using Mirror;
using UnityEngine;

public class Explosive : Projectile
{
  [SyncVar]
  public bool detonateOnImpact;

  public GameObject explosionParticle;

  [SyncVar]
  bool detonated;

  [SerializeField] bool stickOnCollision;
  [SerializeField] float radius;

  protected override void OnCollisionEnter2D(Collision2D other)
  {
    if (detonateOnImpact && !detonated)
    {
      Detonate();
    }
    else if (stickOnCollision)
    {
      GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
      GetComponent<Rigidbody2D>().velocity = Vector2.zero;

      RaycastHit2D raycastHit = Physics2D.Raycast(transform.position, Vector2.up, 1.5f);
      bool top = raycastHit.collider != null && raycastHit.collider == other.collider;

      if (!top)
      {
        raycastHit = Physics2D.Raycast(transform.position, Vector2.right, 1.5f);
      }

      bool right = raycastHit.collider != null && raycastHit.collider == other.collider;

      if (!top && !right)
      {
        raycastHit = Physics2D.Raycast(transform.position, Vector2.down, 1.5f);
      }

      bool bottom = raycastHit.collider != null && raycastHit.collider == other.collider;

      if (top)
      {
        transform.rotation = Quaternion.Euler(0, 0, 90);
      }
      else
      {
        transform.rotation = right ? Quaternion.Euler(0, 0, 0) : bottom ? Quaternion.Euler(0, 0, -90) : Quaternion.Euler(0, 180, 0);
      }

      Vector3 contactPoint = other.GetContact(other.contactCount - 1).point;
      contactPoint.y += (top ? -1 : 1) * GetComponent<SpriteRenderer>().bounds.size.y / 2;
      contactPoint.x = transform.position.x;

      transform.position = contactPoint;
    }
  }

  [Command(requiresAuthority = false)]
  public void Detonate()
  {
    if (detonated)
    {
      return;
    }

    detonated = true;

    DamageController[] damageControllers = FindObjectsOfType<DamageController>();

    foreach (DamageController controller in damageControllers)
    {
      float dist = Utilities.CalculateDistance(transform.position, controller.transform.position);

      if (dist <= radius)
      {
        controller.TakeDamage(damage, owner);
      }
    }

    if (explosionParticle != null)
    {
      GameObject spawned = Instantiate(explosionParticle, transform.position, Quaternion.identity);
      NetworkServer.Spawn(spawned);
    }

    NetworkServer.Destroy(gameObject);
  }
}
