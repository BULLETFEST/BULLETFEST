using UnityEngine;
using System;

public class PlayerData
{
  // public int netId { get; set; }
  public string displayName { get; set; }
  public int kills { get; set; }
  public int wins { get; set; }

  public PlayerData(string displayName)
  {
    this.displayName = displayName;
    kills = 0;
    wins = 0;
  }
}