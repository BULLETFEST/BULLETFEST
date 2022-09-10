using UnityEngine;

public class Version
{
  public int major;
  public int minor;
  public int patch;
  public VersionType versionType;
  public int build;

  public Version(string ver)
  {
    string[] versionStructure = ver.Replace('-', '.').Split('.');
    major = int.Parse(versionStructure[0]);
    minor = int.Parse(versionStructure[1]);
    patch = int.Parse(versionStructure[2]);

    if (versionStructure.Length == 5)
    {
      versionType = System.Enum.Parse<VersionType>(versionStructure[3], true);
      build = int.Parse(versionStructure[4]);
    }
    else
    {
      versionType = VersionType.release;
      build = int.Parse(versionStructure[3]);
    }

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
    return $"{major}.{minor}.{patch}.{versionType.ToString()}-{build}";
  }

  public bool IsMoreRecent(Version other)
  {
    if (this.versionType > other.versionType) return true;
    else if (this.versionType == other.versionType)
    {
      if (this.major > other.major) return true;
      else if (this.major == other.major)
      {
        if (this.minor > other.minor) return true;
        else if (this.minor == other.minor)
        {
          if (this.patch > other.patch) return true;
          else if (this.patch == other.patch)
          {
            if (this.build > other.build) return true;
            else return false;
          }
          else return false;
        }
        else return false;
      }
      else return false;
    }
    else return false;
  }


  public enum VersionIncrease
  {
    Major,
    Minor,
    Patch,
    Build
  }

  public enum VersionType
  {
    alpha = 0,
    beta = 1,
    release = 2
  }
}
