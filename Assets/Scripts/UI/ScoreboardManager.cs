using System.Collections.Generic;
using System.Linq;
using Mirror;
using TMPro;
using UnityEngine;

public class ScoreboardManager : NetworkBehaviour
{
  [SerializeField]
  GameObject itemPrefab, content;
  [SerializeField]
  TextMeshProUGUI title;


  SyncList<PlayerData> data = new();

  public override void OnStartServer()
  {
    base.OnStartServer();

    foreach (PlayerData pd in MyNetworkManager.instance.players.Values)
    {
      data.Add(pd);
    }
  }

  void Start()
  {
    data.Callback += UpdateScoreboard;
  }

  void UpdateScoreboard(SyncList<PlayerData>.Operation op, int index, PlayerData oldItem, PlayerData newItem)
  {
    for (int i = 0; i < content.transform.childCount; i++)
    {
      Destroy(content.transform.GetChild(i));
    }

    List<PlayerData> t = data.ToList();

    t.Sort((a, b) => a.kills.CompareTo(b.kills));

    foreach (PlayerData dt in t)
    {
      GameObject item = Instantiate(itemPrefab, Vector3.zero, Quaternion.identity, content.transform);
      ScoreboardItem scoreboardItem = item.GetComponent<ScoreboardItem>();

      scoreboardItem.t_Name.text = dt.displayName;
      scoreboardItem.t_Kills.text = dt.kills.ToString();
      scoreboardItem.t_Wins.text = dt.wins.ToString();
      scoreboardItem.t_Deaths.text = dt.deaths.ToString();
    }
  }
  // public override void OnStartClient()
  // {
  //   base.OnStartClient();


  // }
}
