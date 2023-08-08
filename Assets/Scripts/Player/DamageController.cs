using System;
using System.Diagnostics;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

public class DamageController : NetworkBehaviour
{
  [SyncVar(hook = nameof(OnDamageTaken))]
  public float health, maxHealth;

  public Action<GameObject> onTakeDamage;
  // public Action<GameObject> onDeath;

  [SerializeField] private GameObject killfeedItem, playerDeathParticles, gravestone;
  private ComponentRefs refs;

  [SyncVar]
  public bool dead = false;
  private GameObject damageDealer;

  [SerializeField] UnityEvent onDie;

  private void Start()
  {
    health = maxHealth;
    refs = GetComponent<ComponentRefs>();
  }

  [Command(requiresAuthority = false)]
  public void TakeDamage(float damage, GameObject owner)
  {
    if (dead)
    {
      return;
    }

    damageDealer = owner;

    health -= damage;

    onTakeDamage?.Invoke(damageDealer);
  }

  private void OnDamageTaken(float oldHealth, float newHealth)
  {
    if (health <= 0)
    {
      // Die();
      onDie?.Invoke();
    }
  }

  [Command(requiresAuthority = false)]
  public void PlayerDie()
  {
    StackFrame frame = new(1);
    if (frame.GetMethod().DeclaringType != typeof(DamageController)) return;

    if (health > 0 || dead)
    {
      return;
    }

    dead = true;

    GameObject killer = damageDealer != null ? damageDealer : gameObject;

    refs.lockMovement = true;
    refs.lockShooting = true;
    refs.lockWeapon = true;

    GameSettings.GameMode gm = GameManager.settings.gameMode;

    if (gm == GameSettings.GameMode.Deathmatch)
    {
      refs.uiName.gameObject.SetActive(false);
    }

    string killerName;
    string killedName = GetComponent<ComponentRefs>().uiName.text;

    bool botKiller = killer.GetComponent<BotRefs>() != null;

    if (botKiller)
    {
      killerName = "BOT";
    }
    else
    {
      NetworkConnectionToClient killerIdentity = killer.GetComponent<NetworkIdentity>().connectionToClient;

      if (killer == gameObject)
      {
        GameManager.Instance.players[killerIdentity.connectionId].kills--;
      }
      else
      {
        GameManager.Instance.players[killerIdentity.connectionId].kills++;
      }

      killerName = killer.GetComponent<ComponentRefs>().uiName.text;

      // I have to set the Value to itself because SyncDictionary cannot listen to changes on properties of value classes
      GameManager.Instance.players[killerIdentity.connectionId] = GameManager.Instance.players[killerIdentity.connectionId];
    }

    if (!GetComponent<BotRefs>())
    {
      GameManager.Instance.players[connectionToClient.connectionId].deaths++;

      // Same as I've mentioned before
      GameManager.Instance.players[connectionToClient.connectionId] = GameManager.Instance.players[connectionToClient.connectionId];
    }

    ClientRpc_Die();

    if (gm != GameSettings.GameMode.Deathmatch)
    {
      LayerMask lm = LayerMask.GetMask("Environment/Floor", "Bounds/Walls");

      RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, Mathf.Infinity, lm);
      if (hit.collider != null)
      {
        GameObject spawnedGravestone = Instantiate(gravestone, new Vector2(hit.point.x, hit.point.y + (gravestone.GetComponentInChildren<SpriteRenderer>().bounds.size.y / 2)), Quaternion.Euler(0, 0, 0));
        NetworkServer.Spawn(spawnedGravestone);
      }
    }


    foreach (System.Collections.Generic.KeyValuePair<int, NetworkConnectionToClient> player in NetworkServer.connections)
    {
      UpdateKillfeed(player.Value, killerName, killedName, player.Value.identity.gameObject);
    }

    GameManager.Instance.OnPlayerDie(this);


    // NetworkServer.Spawn(p);
  }

  [ClientRpc]
  public void ClientRpc_Die()
  {
    refs.graphics.DisableAll();
    refs.uiName.gameObject.SetActive(false);

    gameObject.GetComponent<BoxCollider2D>().enabled = false;
    gameObject.GetComponent<Rigidbody2D>().simulated = false;

    GameObject p = Instantiate(playerDeathParticles, new Vector2(transform.position.x, transform.position.y + 2.5f), Quaternion.identity);

    ParticleSystem.MainModule s = p.GetComponent<ParticleSystem>().main;
    ParticleSystem.TrailModule t = p.GetComponent<ParticleSystem>().trails;

    s.startColor = new ParticleSystem.MinMaxGradient(refs.graphics.sprites[0].color);
    t.colorOverLifetime = refs.graphics.sprites[0].color;
  }

  [TargetRpc]
  public void UpdateKillfeed(NetworkConnection conn, string killer, string killed, GameObject target)
  {
    PlayerRefs localVars = (PlayerRefs)target.GetComponent<ComponentRefs>();
    GameObject spawnedKillfeedItem = Instantiate(killfeedItem, Vector3.zero, Quaternion.Euler(0, 0, 0), localVars.killfeed.transform);


    spawnedKillfeedItem.GetComponent<KillFeedItem>().killer.text = killer;
    spawnedKillfeedItem.GetComponent<KillFeedItem>().killed.text = killed;
  }
}
