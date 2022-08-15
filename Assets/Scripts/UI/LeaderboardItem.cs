using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;
using System.Linq;
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

  private MyNetworkManager room;
  public MyNetworkManager Room
  {
    get
    {
      if (room != null) { return room; }
      return room = NetworkManager.singleton as MyNetworkManager;
    }
  }


  private void Start()
  {
    transform.SetParent(GameObject.FindGameObjectWithTag("Leaderboard").transform);
    transform.localScale = Vector3.one;
    Time.timeScale = 1;
    // NetworkClient.Ready();
  }

  public override void OnStartClient()
  {
    base.OnStartClient();

    UpdateStats(connectionToClient);
  }

  [Command]
  void UpdateStats(NetworkConnectionToClient conn)
  {
    displayName = Room.players[conn].displayName;
    kills = Room.players[conn].kills.ToString();
    wins = Room.players[conn].wins.ToString();

    ChangeItemIndex(conn.identity.gameObject, System.Array.IndexOf(Room.sortedPlayerList, conn));
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
