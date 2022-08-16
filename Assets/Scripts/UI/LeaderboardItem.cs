using UnityEngine;
using TMPro;
using Mirror;

public class LeaderboardItem : NetworkBehaviour
{
  public TextMeshProUGUI uiDisplayname;
  public TextMeshProUGUI uiKills;
  public TextMeshProUGUI uiWins;


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

    ChangeItemIndex(conn.identity.gameObject, System.Array.IndexOf(MyNetworkManager.instance.sortedPlayerList, conn));
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
