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

    NetworkClient.Ready();
  }

  public override void OnStartAuthority()
  {
    base.OnStartAuthority();



    UpdateStats();
  }

  [Command]
  void UpdateStats()
  {
    displayName = Room.players[connectionToClient].displayName;
    kills = Room.players[connectionToClient].kills.ToString();
    wins = Room.players[connectionToClient].wins.ToString();
  }

  void HandleUpdateDisplayName(string oldName, string newName)
  {
    uiDisplayname.text = newName;
    Rpc_UpdateDisplayName();
  }

  [ClientRpc]
  void Rpc_UpdateDisplayName()
  {
    uiDisplayname.text = displayName;
  }


  void HandleUpdateKills(string oldKills, string newKills)
  {
    uiKills.text = newKills;
    Rpc_UpdateKills();
    SortItems();
  }

  [ClientRpc]
  void Rpc_UpdateKills()
  {
    uiKills.text = kills;
  }

  void HandleUpdateWins(string oldWins, string newWins)
  {
    uiWins.text = newWins;
    Rpc_UpdateWins();
    SortItems();
  }

  [ClientRpc]
  void Rpc_UpdateWins()
  {
    uiWins.text = wins;
  }

  [Command]
  void SortItems()
  {
    GameObject[] items = GameObject.FindGameObjectsWithTag("LeaderboardItem");
    int[] goKills = new int[items.Length];

    for (int i = 0; i < items.Length; i++)
    {
      goKills[i] = int.Parse(items[i].GetComponent<LeaderboardItem>().kills);
    }

    int[] dictKeys = goKills;
    System.Array.Sort(dictKeys);

    Rpc_SortItems(items, goKills, dictKeys);
  }

  [ClientRpc]
  void Rpc_SortItems(GameObject[] go, int[] goKills, int[] dictKeys)
  {
    Dictionary<int, GameObject> dict = new();

    foreach (GameObject g in go)
    {
      dict.Add(goKills[System.Array.IndexOf(go, g)], g);
    }

    for (int i = 0; i < dictKeys.Length; i++)
    {
      dict[dictKeys[i]].transform.SetSiblingIndex(i + 1);
    }
  }
}
