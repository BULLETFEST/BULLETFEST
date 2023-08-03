using UnityEngine;

public class Globals : MonoBehaviour
{
  [SerializeField]
  private bool enableTestMode = false, enableFirebaseTestMode = false;

  public static Globals Instance { get; private set; }

  public static bool _testMode { get; private set; }
  public static bool _firebaseTestMode { get; private set; }

  public static Color[] colors = new Color[] {
    new Color(0.5882353f, 0.1137255f, 0.04313726f), // 961D0B
    new Color(0.0993236f, 0.4487756f, 0.6792453f), // 1972AD
    new Color(0.1027946f, 0.6226415f, 0.1877513f), // 1A9F30
    new Color(0.6235294f, 0.6018561f, 0.1019608f), // 9F991A
  };

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
    _firebaseTestMode = enableFirebaseTestMode && Debug.isDebugBuild;
  }
}
