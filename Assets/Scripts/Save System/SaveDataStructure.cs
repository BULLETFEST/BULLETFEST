[System.Serializable]
public class SaveDataStructure
{
  public SettingsClass settings;
  public string token;

  public SaveDataStructure(SettingsClass settings, string token)
  {
    this.settings = settings;
    this.token = token;
  }

}
