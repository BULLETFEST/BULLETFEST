using System.Collections.Generic;

[System.Serializable]
public class SettingsClass
{
  public Dictionary<string, string> keybinds;
  public float masterVolume;
  public float musicVolume;
  public float sfxVolume;

  public bool fpsCounter;
  public bool invertControls;

  public int targetFps;
  public int screenMode;

  /// <summary> 
  /// 0 = Highest res available
  /// </summary>
  public int resolution;

  /// <param name="resolution"> 0 = Highest res available. <param>
  public SettingsClass(Dictionary<string, string> keybinds, float masterVolume, float musicVolume, float sfxVolume, bool fpsCounter, bool invertControls, int targetFps, int screenMode, int resolution)
  {
    this.keybinds = keybinds;
    this.masterVolume = masterVolume;
    this.musicVolume = musicVolume;
    this.sfxVolume = sfxVolume;
    this.fpsCounter = fpsCounter;
    this.invertControls = invertControls;
    this.targetFps = targetFps;
    this.screenMode = screenMode;
    this.resolution = resolution;
  }
}
