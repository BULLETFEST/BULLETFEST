[System.Serializable]
public class SaveDataStructure
{
  public SettingsClass settings { get; private set; }
  public string token;

  public SaveDataStructure(SettingsClass settings, string token)
  {
    this.settings = settings;
    this.token = token;
  }

}
