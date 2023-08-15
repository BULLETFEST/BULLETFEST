public class Version
{
  public int major;
  public int minor;
  public int patch;
  public VersionType versionType;

  public Version(string ver)
  {
    string[] versionStructure = ver.Replace('-', '.').Split('.');
    major = int.Parse(versionStructure[0]);
    minor = int.Parse(versionStructure[1]);
    patch = int.Parse(versionStructure[2]);

    if (versionStructure.Length == 4)
    {
      versionType = System.Enum.Parse<VersionType>(versionStructure[3], true);
    }
    else
    {
      versionType = VersionType.release;
    }

  }

  public void IncreaseVersion(VersionIncrease versionIncrease = VersionIncrease.Patch)
  {
    if (versionIncrease == VersionIncrease.Major)
    {
      major++;
      minor = 0;
      patch = 0;
    }
    else if (versionIncrease == VersionIncrease.Minor)
    {
      minor++;
      patch = 0;
    }
    else if (versionIncrease == VersionIncrease.Patch)
    {
      patch++;
    }
  }

  public string GetVersionString()
  {
    return $"{major}.{minor}.{patch}-{versionType}";
  }

  public bool IsMoreRecent(Version other)
  {
    if (versionType > other.versionType)
    {
      return true;
    }

    if (versionType == other.versionType)
    {
      if (major > other.major)
      {
        return true;
      }

      if (major == other.major)
      {
        if (minor > other.minor)
        {
          return true;
        }

        if (minor == other.minor)
        {
          return patch >= other.patch;
        }
      }
    }

    return false;
  }


  public enum VersionIncrease
  {
    Major,
    Minor,
    Patch
  }

  public enum VersionType
  {
    alpha = 0,
    beta = 1,
    release = 2
  }
}
