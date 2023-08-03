using System.Collections.Generic;
using System.Linq;
using Mirror;
using TMPro;
using UnityEngine;

public class ScoreboardManager : NetworkBehaviour
{
  [SerializeField]
  private GameObject itemPrefab, content;
  [SerializeField]
  private TextMeshProUGUI title;

  public static ScoreboardManager Instance { get; private set; }


  // public readonly SyncList<PlayerData> data = new();

  public override void OnStartServer()
  {
    base.OnStartServer();


  }

  private void Awake()
  {
    Instance = this;
    GameManager.Instance.players.Callback += UpdateScoreboard;
  }

  private void Start()
  {
    UpdateScoreboard(SyncIDictionary<int, PlayerData>.Operation.OP_ADD, 0, null);
    if (isServer)
    {
      // InitializeScoreboard();
    }
  }

  // [Command(requiresAuthority = false)]
  // private void InitializeScoreboard()
  // {
  //   foreach (PlayerData pd in GameManager.Instance.players.Values)
  //   {
  //     data.Add(pd);
  //   }
  // }


  private void UpdateScoreboard(SyncIDictionary<int, PlayerData>.Operation op, int key, PlayerData changedItem)
  {
    for (int i = 0; i < content.transform.childCount; i++)
    {
      Destroy(content.transform.GetChild(i).gameObject);
    }

    List<PlayerData> t = GameManager.Instance.players.Values.OrderBy(x => x.kills).ToList();

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
}
