using System.Collections;
using Mirror;
using UnityEngine;

public class DestroyTimer : MonoBehaviour
{
  public float time;
  // Start is called before the first frame update
  void Start()
  {
    StartCoroutine(DestroySelf());
  }

  IEnumerator DestroySelf()
  {
    yield return new WaitForSecondsRealtime(time);
    NetworkServer.Destroy(gameObject);
  }
}
