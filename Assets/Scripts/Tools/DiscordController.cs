using System.Collections.Generic;
using UnityEngine;
using Discord;
using System;
using UnityEngine.SceneManagement;

public class DiscordController : MonoBehaviour
{
  public static Discord.Discord discord;

  public static ActivityManager activityManager;
  public static DateTimeOffset now;
  public static RelationshipManager relationshipManager;

  public static string partyId;

  public static Dictionary<ulong, Relationship> relationships = new();

  public readonly static ulong applicationId = 1009938773137694800U;

#if UNITY_EDITOR
  bool debugMode = true;
#else
  bool debugMode = false;
#endif

  void Awake()
  {
    if (debugMode) return;

    discord = new Discord.Discord((long)applicationId, (ulong)CreateFlags.NoRequireDiscord);
    activityManager = discord.GetActivityManager();
    relationshipManager = discord.GetRelationshipManager();

    now = DateTimeOffset.UtcNow;

    Activity activity = new()
    {
      State = "In Main Menu",
      Timestamps = {
        Start = now.ToUnixTimeMilliseconds()
      }
    };

    UpdateActivity(activity);

    SceneManager.activeSceneChanged += (Scene oldScene, Scene newScene) =>
    {
      if (newScene.name == "MainMenu") UpdateActivity(activity);
    };

    activityManager.OnActivityJoin += _secret =>
    {
      if (Mirror.NetworkClient.isConnected || Mirror.NetworkServer.active) return;
      string[] secret = _secret.Split("|||");
      MyNetworkManager.instance.networkAddress = secret[0];
      MyNetworkManager.instance.roomCode = secret[1];
      partyId = secret[2];

      MyNetworkManager.instance.StartClient();
    };

    activityManager.OnActivityInvite += (ActivityActionType Type, ref User user, ref Activity activity2) =>
    {
      if (Type != ActivityActionType.Join) return;
      if (activity2.ApplicationId != (long)applicationId) return;

      Message.DisplayMessage("Invite received!", "Received invite from " + user.Username);
    };

    relationshipManager.OnRefresh += () =>
    {
      relationshipManager.Filter((ref Relationship relationship) =>
      {
        // Filter users to ones that are online on BULLETFEST
        return relationship.Type == RelationshipType.Friend;// && relationship.Presence.Activity.ApplicationId == 1009938773137694800;
      });

      for (var i = 0; i < relationshipManager.Count(); i++)
      {
        // Get an individual relationship from the list
        Relationship relationship = relationshipManager.GetAt((uint)i);

        relationships[(ulong)relationship.User.Id] = relationship;
      }

      if (Utilities.FindWithType(out Friendlist friendslist))
      {
        friendslist.UpdateList();
      }
    };

    // Update the matching user in dict
    relationshipManager.OnRelationshipUpdate += (ref Relationship relationship) =>
    {
      relationships[(ulong)relationship.User.Id] = relationship;

      if (Utilities.FindWithType(out Friendlist friendslist))
      {
        friendslist.UpdateList();
      }
    };
  }

  void Update()
  {
    if (discord != null)
    {
      discord.RunCallbacks();
    }
  }

  public static void UpdateActivity(Activity activity)
  {
    if (discord == null) return;

    activity.Timestamps.Start = now.ToUnixTimeMilliseconds();
    activity.Assets.LargeImage = "unity";
    activity.Assets.LargeText = "BULLETFEST | ALPHA";
    activity.ApplicationId = (long)applicationId;
    activity.Instance = true;

    activityManager.UpdateActivity(activity, (res) =>
    {
      if (res != Result.Ok) print(res);
      // else
      // {
      //   // print(JsonUtility.ToJson(activity, true));
      //   // print(JsonUtility.ToJson(activity.Party, true));
      //   // print(JsonUtility.ToJson(activity.Party.Size, true));
      //   // print(JsonUtility.ToJson(activity.Secrets, true));
      // }
    });
  }

  private void OnApplicationQuit()
  {
    if (debugMode) return;

    activityManager.ClearActivity(res => { });
    FirebaseManager.CloseLobby();
  }
}
