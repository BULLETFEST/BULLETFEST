using UnityEditor;
using System.Collections.Generic;

public class BuildScript
{
  [MenuItem("Jobs/PerformCustomBuild")]
  static void PerformBuild()
  {
    List<string> scenes = new();

    foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
    {
      scenes.Add(scene.path);
    }

    string buildPath = "./build/";

    switch (EditorUserBuildSettings.activeBuildTarget)
    {
      case BuildTarget.StandaloneWindows64:
        buildPath += "Windows/BULLETFEST.exe";
        break;
      case BuildTarget.StandaloneOSX:
        buildPath += "MacOS/BULLETFEST.app";
        break;
      case BuildTarget.Android:
        buildPath += "Android/BULLETFEST.apk";
        break;
      case BuildTarget.StandaloneLinux64:
        buildPath += "Linux/BULLETFEST";
        break;
    }


    BuildPlayerOptions buildPlayerOptions = new()
    {
      locationPathName = buildPath,
      options = BuildOptions.None,
      target = EditorUserBuildSettings.activeBuildTarget,
      scenes = scenes.ToArray()
    };

    BuildPipeline.BuildPlayer(buildPlayerOptions);
  }
}
