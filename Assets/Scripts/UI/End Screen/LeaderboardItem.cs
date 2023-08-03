using Mirror;
using TMPro;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class LeaderboardItem : NetworkBehaviour
{
  public TMP_Text uiDisplayname;
  public TMP_Text uiKills;
  public TMP_Text uiWins;

  [SyncVar(hook = nameof(UpdateRichPresence))]
  private int place;

  [SyncVar(hook = nameof(HandleUpdateDisplayName))]
  [HideInInspector] public string displayName;

  [SyncVar(hook = nameof(HandleUpdateKills))]
  [HideInInspector] public string kills;

  [SyncVar(hook = nameof(HandleUpdateWins))]
  [HideInInspector] public string wins;

  private void Start()
  {
    transform.SetParent(GameObject.FindGameObjectWithTag("Leaderboard").transform);
    transform.localScale = Vector3.one;
    transform.position = new Vector3(transform.position.x, transform.position.y, 0);
    Time.timeScale = 1;
  }

  public override void OnStartClient()
  {
    base.OnStartClient();

    UpdateStats(connectionToClient);
  }

  [Command]
  private void UpdateStats(NetworkConnectionToClient conn)
  {
    displayName = GameManager.Instance.players[conn.connectionId].displayName;
    kills = GameManager.Instance.players[conn.connectionId].kills.ToString();
    wins = GameManager.Instance.players[conn.connectionId].wins.ToString();

    List<PlayerData> sortedList = GameManager.Instance.players.Values.ToList();
    sortedList.Sort((a, b) => a.kills.CompareTo(b.kills));
    place = sortedList.FindIndex(x => x.connId == conn.connectionId);
    ChangeItemIndex(conn.identity.gameObject, place);
  }

  private void UpdateRichPresence(int oldValue, int newValue)
  {
    DiscordController.UpdateActivity(new Discord.Activity
    {
      State = $"Finished in {newValue}{(newValue == 1 ? "st" : place == 2 ? "nd" : "rd")} out of {NetworkServer.connections.Count}"
    });
  }

  [ClientRpc]
  private void ChangeItemIndex(GameObject owner, int index)
  {
    print(index);
    owner.transform.SetSiblingIndex(Mathf.Max(0, index) + 1);
  }

  private void HandleUpdateDisplayName(string oldName, string newName)
  {
    uiDisplayname.text = newName;
  }

  private void HandleUpdateKills(string oldKills, string newKills)
  {
    uiKills.text = newKills;
  }

  private void HandleUpdateWins(string oldWins, string newWins)
  {
    uiWins.text = newWins;
  }
}
