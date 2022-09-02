using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Friendlist : MonoBehaviour
{
  [SerializeField]
  GameObject friendPrefab,
             friendsContainer;

  public void UpdateList()
  {
    print(friendsContainer.transform.childCount);
    foreach (Transform child in friendsContainer.transform)
    {
      Destroy(child.gameObject);
    }

    foreach (KeyValuePair<ulong, Discord.Relationship> relationship in DiscordController.relationships)
    {
      GameObject friend = Instantiate(friendPrefab);
      friend.transform.SetParent(friendsContainer.transform);
      friend.transform.localScale = Vector3.one;

      friend.GetComponent<FriendlistItem>().username.text = relationship.Value.User.Username;
      print(JsonUtility.ToJson(relationship.Value.Presence.Activity, true));

      friend.GetComponent<FriendlistItem>().status.text = relationship.Value.Presence.Activity.ApplicationId == (long)DiscordController.applicationId ? "Online" : "Offline";
    }
  }
}
