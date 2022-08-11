#if UNITY_EDITOR
using UnityEngine;
using EpicTransport;

[ExecuteInEditMode]
public class SetCredentialNames : MonoBehaviour
{
  // Tutorial in https://github.com/FakeByte/EpicOnlineTransport/tree/v1.5.0#testing-multiplayer-on-one-device
  void Start()
  {
    EOSSDKComponent eOSSDKComponent = GetComponent<EOSSDKComponent>();

    if (ParrelSync.ClonesManager.IsClone())
    {
      eOSSDKComponent.devAuthToolCredentialName = "Main";
    }
    else
    {
      eOSSDKComponent.devAuthToolCredentialName = "Alt";
    }
  }
}
#endif