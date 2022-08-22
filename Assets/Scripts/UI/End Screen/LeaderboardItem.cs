using UnityEngine;
using TMPro;
using Mirror;

public class LeaderboardItem : NetworkBehaviour
{
  public TMP_Text uiDisplayname;
  public TMP_Text uiKills;
  public TMP_Text uiWins;

  [SyncVar(hook = nameof(UpdateRichPresence))]
  int place;

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
    Time.timeScale = 1;
  }

  public override void OnStartClient()
  {
    base.OnStartClient();

    UpdateStats(connectionToClient);
  }

  [Command]
  void UpdateStats(NetworkConnectionToClient conn)
  {
    displayName = MyNetworkManager.instance.players[conn].displayName;
    kills = MyNetworkManager.instance.players[conn].kills.ToString();
    wins = MyNetworkManager.instance.players[conn].wins.ToString();

    place = System.Array.IndexOf(MyNetworkManager.instance.sortedPlayerList, conn);
    ChangeItemIndex(conn.identity.gameObject, place);
  }

  void UpdateRichPresence(int oldValue, int newValue)
  {
    DiscordController.UpdateActivity(new Discord.Activity
    {
      State = $"Finished in {newValue}{(newValue == 1 ? "st" : place == 2 ? "nd" : "rd")} out of {NetworkServer.connections.Count}"
    });
  }

  [ClientRpc]
  void ChangeItemIndex(GameObject owner, int index)
  {
    owner.transform.SetSiblingIndex(index + 1);
  }

  void HandleUpdateDisplayName(string oldName, string newName)
  {
    uiDisplayname.text = newName;
  }

  void HandleUpdateKills(string oldKills, string newKills)
  {
    uiKills.text = newKills;
  }

  void HandleUpdateWins(string oldWins, string newWins)
  {
    uiWins.text = newWins;
  }
}
