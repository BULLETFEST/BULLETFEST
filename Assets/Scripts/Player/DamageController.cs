using System;
using Mirror;
using UnityEngine;

public class DamageController : NetworkBehaviour
{
  [SyncVar(hook = nameof(OnDamageTaken))]
  public float health, maxHealth;

  public Action<GameObject> onTakeDamage;
  public Action<GameObject> onDeath;

  [SerializeField] private GameObject killfeedItem, playerDeathParticles, gravestone;
  private ComponentRefs refs;

  [SyncVar]
  public bool dead = false;
  private GameObject damageDealer;

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
      Die();
    }
  }

  [Command(requiresAuthority = false)]
  public void Die()
  {
    if (health > 0 || dead)
    {
      return;
    }

    dead = true;

    // onDeath?.Invoke(damageDealer ?? gameObject);

    Server_Die(damageDealer != null ? damageDealer : gameObject);
  }


  [ServerCallback]
  public void Server_Die(GameObject killer)
  {
    refs.lockMovement = true;
    refs.lockShooting = true;
    refs.lockWeapon = true;

    GameSettings.GameMode gm = MyNetworkManager.instance.settings.gameMode;

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
        MyNetworkManager.instance.players[killerIdentity].kills--;
      }
      else
      {
        MyNetworkManager.instance.players[killerIdentity].kills++;
      }

      killerName = killer.GetComponent<ComponentRefs>().uiName.text;
    }

    // ScoreboardManager.instance.data.IndexOf(ScoreboardManager.instance.data.Find(x => x.connId == killer.GetComponent<NetworkIdentity>().connectionToClient.connectionId));

    if (!GetComponent<BotRefs>())
    {
      MyNetworkManager.instance.players[connectionToClient].deaths++;
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

    ScoreboardManager.Instance.data.Clear();

    foreach (System.Collections.Generic.KeyValuePair<NetworkConnectionToClient, PlayerData> dt in MyNetworkManager.instance.players)
    {
      ScoreboardManager.Instance.data.Add(dt.Value);
    }


    foreach (System.Collections.Generic.KeyValuePair<int, NetworkConnectionToClient> player in NetworkServer.connections)
    {
      UpdateKillfeed(player.Value, killerName, killedName, player.Value.identity.gameObject);
    }

    MyNetworkManager.instance.OnPlayerDie(gameObject);


    // NetworkServer.Spawn(p);
  }

  [ClientRpc]
  public void ClientRpc_Die()
  {
    refs.graphics.DisableAll();
    refs.uiName.gameObject.SetActive(false);

    gameObject.GetComponent<BoxCollider2D>().enabled = false;
    gameObject.GetComponent<Rigidbody2D>().simulated = false;

    GameObject p = Instantiate(playerDeathParticles, transform.position, Quaternion.identity);

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
