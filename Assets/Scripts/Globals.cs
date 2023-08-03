using UnityEngine;

public class Globals : MonoBehaviour
{
  [SerializeField]
  private bool enableTestMode = false;

  public static Globals Instance { get; private set; }

  public static bool _testMode { get; private set; }

  private void Awake()
  {
    if (Instance == null)
    {
      Instance = this;
    }
    else
    {
      Destroy(gameObject);
    }

    _testMode = enableTestMode && Debug.isDebugBuild;
  }
}
