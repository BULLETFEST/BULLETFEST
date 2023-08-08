using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using TMPro;
using UnityEngine;

public class ChatManager : NetworkBehaviour
{
  public SyncList<string> messages = new();
  private List<GameObject> messageObjects = new();

  [SerializeField]
  private GameObject messagePrefab;

  private void Start()
  {
    DontDestroyOnLoad(gameObject);
  }

  private void Awake()
  {
    if (MyNetworkManager.Instance.Chat == null)
      MyNetworkManager.Instance.Chat = this;
    else NetworkServer.Destroy(gameObject);

    messages.Callback += OnCollectionChanged;
  }

  public override void OnStartServer()
  {
    base.OnStartServer();

    MyNetworkManager.Instance.PlayerUpdate += NotifyJoin;
  }

  private void OnDestroy()
  {
    MyNetworkManager.Instance.PlayerUpdate -= NotifyJoin;
    messages.Callback -= OnCollectionChanged;
  }

  [ServerCallback]
  private void NotifyJoin()
  {
    messages.Add($"G|{GameManager.Instance.players.Last().Value.displayName} has joined the game");
  }

  private void OnCollectionChanged(SyncList<string>.Operation op, int index, string oldItem, string newItem)
  {
    if (op == SyncList<string>.Operation.OP_ADD)
    {
      InstantiateMessage(messages.Last());

      if (messageObjects.Count > 5)
      {
        GameObject go = messageObjects.First();
        messageObjects.Remove(go);
        Destroy(go);
      }
    }
  }

  // [ClientRpc]
  private void InstantiateMessage(string content)
  {
    GameObject message = Instantiate(messagePrefab, Vector3.zero, Quaternion.identity, transform.GetChild(0));
    string[] c = content.Split('|', 2);

    switch (c[0])
    {
      case "W":
      default:
        message.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
        break;

      case "R":
        message.GetComponentInChildren<TextMeshProUGUI>().color = Color.red;
        break;

      case "G":
        message.GetComponentInChildren<TextMeshProUGUI>().color = Color.green;
        break;
    }

    message.GetComponentInChildren<TextMeshProUGUI>().text = c[1];
    messageObjects.Add(message);
    StartCoroutine(DestroyMessage(message, 5));
  }

  private IEnumerator DestroyMessage(GameObject go, float time)
  {
    yield return new WaitForSecondsRealtime(time);

    messageObjects.Remove(go);
    Destroy(go);
  }
}

