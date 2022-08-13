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

    print("A");

    UpdateStats(connectionToClient);
  }

  [Command]
  void UpdateStats(NetworkConnectionToClient conn)
  {
    displayName = Room.players[conn].displayName;
    kills = Room.players[conn].kills.ToString();
    wins = Room.players[conn].wins.ToString();

    // Rpc_UpdateDisplayName(Room.players[conn].displayName);
    // Rpc_UpdateKills(Room.players[conn].kills.ToString());
    // Rpc_UpdateWins(Room.players[conn].wins.ToString());

  }

  [ClientRpc]
  void Rpc_UpdateKills(string kills)
  {
    uiKills.text = kills;
    print("AA");
  }

  [ClientRpc]
  void Rpc_UpdateDisplayName(string displayName)
  {
    uiDisplayname.text = displayName;
  }

  [ClientRpc]
  void Rpc_UpdateWins(string wins)
  {
    uiWins.text = wins;
  }

  void HandleUpdateDisplayName(string oldName, string newName)
  {
    uiDisplayname.text = newName;

    print("A");

    // Rpc_UpdateDisplayName();
  }

  void HandleUpdateKills(string oldKills, string newKills)
  {
    uiKills.text = newKills;

    print("A");

    // Rpc_UpdateKills();
    SortItems();
  }

  void HandleUpdateWins(string oldWins, string newWins)
  {
    uiWins.text = newWins;
    // Rpc_UpdateWins();
  }

  // [ClientRpc]
  // void Rpc_UpdateWins(string wins)
  // {
  //   uiWins.text = wins;
  // }

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
