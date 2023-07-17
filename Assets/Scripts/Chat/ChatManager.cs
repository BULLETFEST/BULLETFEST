using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using TMPro;
using UnityEngine;

public class ChatManager : NetworkBehaviour
{
  public SyncList<string> messages = new();

  List<GameObject> messageObjects = new();

  [SerializeField]
  GameObject messagePrefab;

  void Start()
  {
    DontDestroyOnLoad(gameObject);
  }

  void Awake()
  {
    MyNetworkManager.instance.Chat = this;
    messages.Callback += OnCollectionChanged;
  }

  public override void OnStartServer()
  {
    base.OnStartServer();

    MyNetworkManager.instance.PlayerUpdate += NotifyJoin;
  }

  void OnDestroy()
  {
    MyNetworkManager.instance.PlayerUpdate -= NotifyJoin;
    messages.Callback -= OnCollectionChanged;
  }

  [ServerCallback]
  void NotifyJoin()
  {
    messages.Add($"G|{MyNetworkManager.instance.players.Last().Value.displayName} has joined the game");
  }

  void OnCollectionChanged(SyncList<string>.Operation op, int index, string oldItem, string newItem)
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
  void InstantiateMessage(string content)
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

  IEnumerator DestroyMessage(GameObject go, float time)
  {
    yield return new WaitForSecondsRealtime(time);

    messageObjects.Remove(go);
    Destroy(go);
  }
}

