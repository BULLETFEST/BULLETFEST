using System;
using Mirror;
using UnityEngine;

public class DamageController : NetworkBehaviour
{
  [SyncVar(hook = nameof(OnDamageTaken))]
  public float health, maxHealth;

  public Action<GameObject> onTakeDamage;
  public Action<GameObject> onDeath;

  [SyncVar]
  public bool dead = false;
  private GameObject damageDealer;

  private void Start()
  {
    health = maxHealth;
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

    onDeath?.Invoke(damageDealer ?? gameObject);
  }


  [ServerCallback]
  public void Server_Die(GameObject killer)
  {
    playerVars.lockMovement = true;
    playerVars.lockShooting = true;
    playerVars.lockWeapon = true;

    GameSettings.GameMode gm = MyNetworkManager.instance.settings.gameMode;

    if (gm == GameSettings.GameMode.Deathmatch)
    {
      playerVars.uiName.gameObject.SetActive(false);
    }

    string killerName;
    string killedName = GetComponent<PlayerVars>().displayName;

    bool botKiller = killer.GetComponent<BotVars>() != null;

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

      killerName = killer.GetComponent<PlayerVars>().displayName;
    }

    ClientRpc_Die();

    if (gm != GameSettings.GameMode.Deathmatch)
    {
      // GameObject spawnedGravestone = Instantiate(gravestone, new Vector2(transform.position.x,
      //                                                       playerVars.bc.bounds.min.y + (gravestone.GetComponentInChildren<SpriteRenderer>().bounds.size.y / 2)), Quaternion.Euler(0, 0, 0));
      // NetworkServer.Spawn(spawnedGravestone);
      LayerMask lm = 1 << 6;
      lm |= 1 << 12;

      RaycastHit2D hit = Utilities.Grounded(transform, playerVars.bc, lm, 999999f);
      if (hit.collider != null)
      {
        GameObject spawnedGravestone = Instantiate(gravestone, new Vector2(hit.point.x, hit.point.y + (gravestone.GetComponentInChildren<SpriteRenderer>().bounds.size.y / 2)), Quaternion.Euler(0, 0, 0));
        NetworkServer.Spawn(spawnedGravestone);
      }
    }

    foreach (System.Collections.Generic.KeyValuePair<int, NetworkConnectionToClient> player in NetworkServer.connections)
    {
      UpdateKillfeed(player.Value, killerName, killedName);
    }

    MyNetworkManager.instance.OnPlayerDie(connectionToClient);
    GameObject p = Instantiate(playerDeathParticles, gameObject.transform.position, Quaternion.identity);
    NetworkServer.Spawn(p);
  }

  [ClientRpc]
  public void ClientRpc_Die()
  {
    playerVars.graphics.DisableAll();
    playerVars.uiName.gameObject.SetActive(false);
    gameObject.GetComponent<BoxCollider2D>().enabled = false;
    gameObject.GetComponent<Rigidbody2D>().simulated = false;
  }

  [TargetRpc]
  public void UpdateKillfeed(NetworkConnection conn, string killer, string killed)
  {
    // print(gameObject);
    PlayerVars localVars = gameObject.GetComponent<PlayerVars>();
    GameObject spawnedKillfeedItem = Instantiate(killfeedItem, Vector3.zero, Quaternion.Euler(0, 0, 0), localVars.killfeed.transform);


    spawnedKillfeedItem.GetComponent<KillFeedItem>().killer.text = killer;
    spawnedKillfeedItem.GetComponent<KillFeedItem>().killed.text = killed;
  }
}
