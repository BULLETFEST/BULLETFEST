using System.Collections;
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
  public static LobbyManager lobbyManager;

  public static ulong lobbyId = 0;

#if UNITY_EDITOR
  bool debugMode = true;
#else
  bool debugMode = false;
#endif

  void Start()
  {
    if (debugMode) return;

    discord = new Discord.Discord(1009938773137694800, (System.UInt64)Discord.CreateFlags.NoRequireDiscord);
    activityManager = discord.GetActivityManager();
    lobbyManager = discord.GetLobbyManager();

    now = DateTimeOffset.UtcNow;

    Activity activity = new Activity
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
      string[] secret = _secret.Split("|||");
      MyNetworkManager.instance.networkAddress = secret[0];
      MyNetworkManager.instance.RoomCode = secret[1];
      MyNetworkManager.instance.StartClient();

      lobbyManager.ConnectLobbyWithActivitySecret(_secret, (Discord.Result result, ref Discord.Lobby lobby) =>
      {
        if (result == Discord.Result.Ok)
        {
          lobbyId = (ulong)lobby.Id;
        }
      });
    };

    activityManager.OnActivityInvite += (ActivityActionType Type, ref Discord.User user, ref Discord.Activity activity2) =>
    {
      if (Type != ActivityActionType.Join) return;
      if (activity2.ApplicationId != 1009938773137694800) return;

      Message.DisplayMessage("Invite received!", "Received invite from " + user.Username);
    };
  }

  void Update()
  {
    if (discord != null) discord.RunCallbacks();
  }

  public static void UpdateActivity(Discord.Activity activity)
  {
    activity.Timestamps.Start = now.ToUnixTimeMilliseconds();
    activity.Assets.LargeImage = "Unity";
    activity.Assets.LargeText = "BULLETFEST | ALPHA";
    if (discord != null)
      activityManager.UpdateActivity(activity, (res) => { });
  }

  private void OnApplicationQuit()
  {
    activityManager.ClearActivity(res => { });
  }
}
