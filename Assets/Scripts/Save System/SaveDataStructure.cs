using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class SaveDataStructure
{
  public int[] worldProgress;
  public float hp;
  public string[] gunIDs;
  public SettingsClass settings;

  public SaveDataStructure(SettingsClass settings)
  {
    this.settings = settings;
  }

}
