using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public class BuildVersionProcessor : IPreprocessBuildWithReport
{
  public int callbackOrder => 0;

  public void OnPreprocessBuild(BuildReport report)
  {
    Version version = new(FindCurrentVersion());
    // version.IncreaseVersion();
    PlayerSettings.bundleVersion = version.GetVersionString();
    PlayerSettings.SplashScreen.show = false;

#if UNITY_STANDALONE_OSX
    UnityEditor.OSXStandalone.UserBuildSettings.architecture = OSArchitecture.x64ARM64;
#endif
  }

  private string FindCurrentVersion()
  {
    return PlayerSettings.bundleVersion;
  }
}
