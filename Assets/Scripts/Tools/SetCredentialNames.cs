#if UNITY_EDITOR
using EpicTransport;
using UnityEngine;

[ExecuteInEditMode]
public class SetCredentialNames : MonoBehaviour
{
  // Tutorial in https://github.com/FakeByte/EpicOnlineTransport/tree/v1.5.0#testing-multiplayer-on-one-device
  private void Start()
  {
    EOSSDKComponent eOSSDKComponent = GetComponent<EOSSDKComponent>();

    eOSSDKComponent.devAuthToolCredentialName = ParrelSync.ClonesManager.IsClone() ? "Main" : "Alt";
  }
}
#endif