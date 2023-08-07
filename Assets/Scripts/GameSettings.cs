public class GameSettings
{
  public bool goldenGun,
              enableBots;

  public int lobbySize = 4,
             rounds,
             chosenMap = 0;

  public PrivacyType privacyType = PrivacyType.Public;

  public GameMode gameMode = GameMode.Elimination;

  public float deathmatchLength = 1;

  public bool allowLateJoin = false;

  public enum GameMode
  {
    Elimination = 0,
    Deathmatch = 1,
  }

  public enum PrivacyType
  {
    Public = 0,
    Private = 1,
  }
}
