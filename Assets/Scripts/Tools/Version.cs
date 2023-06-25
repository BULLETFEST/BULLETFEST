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
    else
    {
      build++;
    }
  }

  public string GetVersionString()
  {
    return $"{major}.{minor}.{patch}.{versionType}-{build}";
  }

  public bool IsMoreRecent(Version other)
  {
    if (versionType > other.versionType)
    {
      return true;
    }
    else if (versionType == other.versionType)
    {
      if (major > other.major)
      {
        return true;
      }
      else if (major == other.major)
      {
        if (minor > other.minor)
        {
          return true;
        }
        else if (minor == other.minor)
        {
          if (patch > other.patch)
          {
            return true;
          }
          else
          {
            return patch == other.patch ? build > other.build : false;
          }
        }
        else
        {
          return false;
        }
      }
      else
      {
        return false;
      }
    }
    else
    {
      return false;
    }
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
