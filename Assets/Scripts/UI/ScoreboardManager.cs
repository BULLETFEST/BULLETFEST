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

  public static ScoreboardManager instance { get; private set; }


  public readonly SyncList<PlayerData> data = new();

  public override void OnStartServer()
  {
    base.OnStartServer();


  }

  void Awake()
  {
    instance = this;
    data.Callback += UpdateScoreboard;
  }

  void Start()
  {
    if (isServer)
      InitializeScoreboard();
  }

  [Command(requiresAuthority = false)]
  void InitializeScoreboard()
  {
    foreach (PlayerData pd in MyNetworkManager.instance.players.Values)
    {
      data.Add(pd);
    }
  }

  void UpdateScoreboard(SyncList<PlayerData>.Operation op, int index, PlayerData oldItem, PlayerData newItem)
  {
    for (int i = 0; i < content.transform.childCount; i++)
    {
      Destroy(content.transform.GetChild(i).gameObject);
    }

    List<PlayerData> t = data.OrderBy(x => x.kills).ToList();

    t.Sort((a, b) => a.kills.CompareTo(b.kills));

    foreach (PlayerData dt in t)
    {
      print(dt.displayName);

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
