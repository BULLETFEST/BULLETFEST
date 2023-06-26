using Mirror;
using UnityEngine;

public class BotBehavior : NetworkBehaviour
{
  private BotVars botVars;
  private GameObject killfeedItem, gravestone;

  private void Start()
  {
    botVars = GetComponent<BotVars>();

    gravestone = FindObjectOfType<PlayerBehavior>().gravestone;
    killfeedItem = FindObjectOfType<PlayerBehavior>().killfeedItem;

    botVars.damageController.onDeath += Die;
  }

  private void OnDestroy()
  {
    botVars.damageController.onDeath -= Die;
  }

  private void Update()
  {
    if (!isServer)
    {
      return;
    }

    if (transform.position.y is <= (-15) or >= 50)
    {
      botVars.damageController.TakeDamage(9999999, null);
    }
  }

  public void Fire(float playerPosX, float angle)
  {
    WeaponClass weapon = botVars.botWb.weapon;

    if (weapon == null)
    {
      return;
    }

    if (weapon.fireTimeout > Time.time)
    {
      return;
    }

    if (weapon.bulletsInMag <= 0)
    {
      return;
    }

    botVars.botWb.transform.localRotation = Quaternion.Euler(playerPosX < 0 ? 180 : 0, playerPosX < 0 ? 180 : 0, ((playerPosX < 0 ? -1 : 1) * angle) + Random.Range(-25f, 25f));

    weapon.bulletsInMag--;
    weapon.fireTimeout = (float)Time.time + (1f / weapon.fireRate * (weapon.firingMode == WeaponClass.FireMode.Single ? 1.65f : 1));

    Rpc_AddForce(gameObject, weapon.shootSound);
    botVars.botWb.Fire(weapon.ID, gameObject);

    if (botVars.botWb.awaitingDetonation.Count >= 3)
    {
      foreach (Explosive explosive in botVars.botWb.awaitingDetonation)
      {
        explosive.Detonate();
      }

      botVars.botWb.awaitingDetonation.Clear();
    }
  }

  [ClientRpc]
  private void Rpc_AddForce(GameObject target, string shootSound)
  {
    botVars.botWb.AddForce(target);
    if (shootSound != "")
    {
      FindObjectOfType<AudioSystem>().PlaySound(shootSound);
    }
  }

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
  private void TargetRpc_SwitchWeapon(string WeaponID)
  {
    botVars.botWb.SwitchWeapon(WeaponID);
  }

  [ServerCallback]
  public void Die(GameObject killer)
  {
    botVars.lockMovement = true;
    botVars.lockShooting = true;
    botVars.lockWeapon = true;

    GameSettings.GameMode gm = MyNetworkManager.instance.settings.gameMode;

    if (gm == GameSettings.GameMode.Deathmatch)
    {
      botVars.uiName.gameObject.SetActive(false);
    }

    string killerName;

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

    ClientRpc_Die(killerName);

    if (gm != GameSettings.GameMode.Deathmatch)
    {
      LayerMask lm = 1 << 6;
      lm |= 1 << 12;

      RaycastHit2D hit = Utilities.Grounded(transform, botVars.bc, lm, 999999f);
      if (hit.collider != null)
      {
        GameObject spawnedGravestone = Instantiate(gravestone, new Vector2(hit.point.x, hit.point.y + (gravestone.GetComponentInChildren<SpriteRenderer>().bounds.size.y / 2)), Quaternion.Euler(0, 0, 0));
        NetworkServer.Spawn(spawnedGravestone);
      }
    }

    foreach (System.Collections.Generic.KeyValuePair<int, NetworkConnectionToClient> player in NetworkServer.connections)
    {
      UpdateKillfeed(player.Value, killerName, player.Key);
    }

    MyNetworkManager.instance.OnPlayerDie(null);

    if (MyNetworkManager.instance.settings.gameMode != GameSettings.GameMode.Elimination)
    {
      StartCoroutine(FindObjectOfType<PlayerSpawnSystem>().Cmd_RespawnBot(gameObject));
    }
  }

  [ClientRpc]
  public void ClientRpc_Die(string killer)
  {
    botVars.graphics.DisableAll();
    botVars.uiName.gameObject.SetActive(false);

    GetComponent<BoxCollider2D>().enabled = false;
    GetComponent<Rigidbody2D>().simulated = false;
  }

  [TargetRpc]
  public void UpdateKillfeed(NetworkConnection conn, string killer, int connId)
  {
    PlayerVars localVars = NetworkServer.connections[connId].identity.gameObject.GetComponent<PlayerVars>();
    GameObject spawnedKillfeedItem = Instantiate(killfeedItem, Vector3.zero, Quaternion.Euler(0, 0, 0), localVars.killfeed.transform);

    spawnedKillfeedItem.GetComponent<KillFeedItem>().killer.text = killer;
    spawnedKillfeedItem.GetComponent<KillFeedItem>().killed.text = "BOT";
  }
}
