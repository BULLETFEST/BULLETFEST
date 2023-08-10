using System;
using System.Collections.Generic;
using Discord;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DiscordController : MonoBehaviour
{
  public static Discord.Discord discord;

  public static ActivityManager activityManager;
  public static DateTimeOffset now;
  public static RelationshipManager relationshipManager;

  public static string partyId;

  public static Dictionary<ulong, Relationship> relationships = new();

  public static readonly ulong applicationId = 1009938773137694800U;

  private void Awake()
  {
    if (Application.isEditor)
    {
      return;
    }

    if (SystemInfo.deviceType == DeviceType.Handheld) return;

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
      if (newScene.name == "MainMenu")
      {
        UpdateActivity(activity);
      }
    };

    activityManager.OnActivityJoin += _secret =>
    {
      if (Mirror.NetworkClient.isConnected || Mirror.NetworkServer.active)
      {
        return;
      }

      string[] secret = _secret.Split("|||");
      MyNetworkManager.Instance.networkAddress = secret[0];
      MyNetworkManager.Instance.roomCode = secret[1];
      partyId = secret[2];

      MyNetworkManager.Instance.StartClient();
    };

    activityManager.OnActivityInvite += (ActivityActionType Type, ref User user, ref Activity activity2) =>
    {
      if (Type != ActivityActionType.Join)
      {
        return;
      }

      if (activity2.ApplicationId != (long)applicationId)
      {
        return;
      }

      Message.DisplayMessage("Invite received!", "Received invite from " + user.Username);
    };

    relationshipManager.OnRefresh += () =>
    {
      relationshipManager.Filter((ref Relationship relationship) =>
      {
        // Filter users to ones that are online on BULLETFEST
        return relationship.Type == RelationshipType.Friend;// && relationship.Presence.Activity.ApplicationId == 1009938773137694800;
      });

      for (int i = 0; i < relationshipManager.Count(); i++)
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

  private void Update()
  {
    discord?.RunCallbacks();
  }

  public static void UpdateActivity(Activity activity)
  {
    if (discord == null)
    {
      return;
    }

    activity.Timestamps.Start = now.ToUnixTimeMilliseconds();
    activity.Assets.LargeImage = "unity";
    activity.Assets.LargeText = "BULLETFEST | ALPHA";
    activity.ApplicationId = (long)applicationId;
    activity.Instance = true;

    activityManager.UpdateActivity(activity, (res) =>
    {
      if (res != Result.Ok)
      {
        print(res);
      }
    });
  }

  private void OnApplicationQuit()
  {
    if (Application.isEditor)
    {
      return;
    }

    activityManager.ClearActivity(res => { });
    FirebaseManager.CloseLobby();
  }
}
