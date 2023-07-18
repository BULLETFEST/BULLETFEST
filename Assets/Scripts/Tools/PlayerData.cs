public class PlayerData
{
  // public int netId { get; set; }
  public string displayName { get; set; }
  public int kills { get; set; }
  public int wins { get; set; }
  public int deaths { get; set; }


  public PlayerData(string displayName)
  {
    this.displayName = displayName;
    kills = 0;
    wins = 0;
    deaths = 0;
  }

  public PlayerData()
  {
    displayName = "Guest";
    kills = 0;
    wins = 0;
    deaths = 0;
  }
}