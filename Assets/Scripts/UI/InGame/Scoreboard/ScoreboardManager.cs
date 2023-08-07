using System.Collections.Generic;
using System.Linq;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScoreboardManager : NetworkBehaviour
{
  [SerializeField]
  private GameObject itemPrefab, content;
  [SerializeField]
  private TextMeshProUGUI title;

  public static ScoreboardManager Instance { get; private set; }

  private void Awake()
  {
    Instance = this;
    GameManager.Instance.players.Callback += UpdateScoreboard;
  }

  private void Start()
  {
    UpdateScoreboard(SyncIDictionary<int, PlayerData>.Operation.OP_ADD, 0, null);
    UpdateTitle();
  }


  private void UpdateScoreboard(SyncIDictionary<int, PlayerData>.Operation op, int key, PlayerData changedItem)
  {
    if (content == null) return;

    for (int i = 0; i < content.transform.childCount; i++)
    {
      Destroy(content.transform.GetChild(i).gameObject);
    }

    List<PlayerData> t = GameManager.Instance.players.Values.OrderBy(x => x.kills).ToList();

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

  [ClientRpc]
  void UpdateTitle()
  {
    title.text = GameManager.settings.gameMode.ToString() + " - " + SceneManager.GetActiveScene().name.Replace("_", " ");
  }
}
