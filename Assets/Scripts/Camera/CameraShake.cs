using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CameraShake : NetworkBehaviour
{
  public IEnumerator Shake(float duration, float magnitude)
  {
    Vector3 origin = transform.position;

    float elapsed = 0.0f;

    while (elapsed < duration)
    {
      float x = Random.Range(-1f, 1f) * magnitude;
      float y = Random.Range(-1f, 1f) * magnitude;

      transform.position = new Vector3(x, y, -10);

      elapsed += Time.unscaledDeltaTime;

      yield return null;
    }

    transform.position = new Vector3(0, 0, -10);
  }

  [ClientRpc]
  void ShakeAll(float duration, float magnitude) => StartCoroutine(Shake(duration, magnitude));
}
