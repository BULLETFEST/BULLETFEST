using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor;

public class BuildVersionProcessor : IPreprocessBuildWithReport
{
  public int callbackOrder => 0;

  public void OnPreprocessBuild(BuildReport report)
  {
    Version version = new(FindCurrentVersion());
    version.IncreaseVersion();
    PlayerSettings.bundleVersion = version.GetVersionString();
    Debug.Log(PlayerSettings.bundleVersion);
  }

  private string FindCurrentVersion()
  {
    return PlayerSettings.bundleVersion;
  }


  class Version
  {
    public int major;
    public int minor;
    public int patch;
    public int build;

    public Version(string ver)
    {
      string[] versionStructure = ver.Replace('-', '.').Split('.');
      Utilities.PrintArr(versionStructure);
      major = int.Parse(versionStructure[0]);
      minor = int.Parse(versionStructure[1]);
      patch = int.Parse(versionStructure[2]);
      build = int.Parse(versionStructure[3]);
    }

    public void IncreaseVersion(VersionIncrease versionIncrease = VersionIncrease.Build)
    {
      if (versionIncrease == VersionIncrease.Major)
      {
        major++;
        minor = 0;
        patch = 0;
        build = 1;
      }
      else if (versionIncrease == VersionIncrease.Minor)
      {
        minor++;
        patch = 0;
        build = 1;
      }
      else if (versionIncrease == VersionIncrease.Patch)
      {
        patch++;
        build = 0;
      }
      else build++;
    }

    public string GetVersionString()
    {
      return $"{major}.{minor}.{patch}-{build}";
    }
  }

  public enum VersionIncrease
  {
    Major,
    Minor,
    Patch,
    Build
  }
}
