using Mirror;

public class PlayerData
{
  // public int netId { get; set; }
  public string displayName;
  public int kills;
  public int wins;
  public int deaths;
  public int connId;


  public PlayerData(string displayName, int connId)
  {
    this.displayName = displayName;
    this.connId = connId;
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

public static class PlayerDataSerializer
{
  public static void WritePlayerData(this NetworkWriter writer, PlayerData data)
  {
    writer.WriteString(data.displayName);
    writer.WriteInt(data.kills);
    writer.WriteInt(data.wins);
    writer.WriteInt(data.deaths);
    writer.WriteInt(data.connId);
  }

  public static PlayerData ReadPlayerData(this NetworkReader reader)
  {
    return new PlayerData
    {
      displayName = reader.ReadString(),
      kills = reader.ReadInt(),
      wins = reader.ReadInt(),
      deaths = reader.ReadInt(),
      connId = reader.ReadInt()
    };
  }
}