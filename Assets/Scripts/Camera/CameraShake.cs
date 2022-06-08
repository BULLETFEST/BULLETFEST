using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CameraShake : NetworkBehaviour
{
  IEnumerator Shake(float duration, float magnitude)
  {
    Vector2 origin = transform.localPosition;

    float elapsed = 0.0f;

    while (elapsed < duration)
    {
      float x = Random.Range(-1f, 1f) * magnitude;
      float y = Random.Range(-1f, 1f) * magnitude;

      transform.localPosition = new Vector2(x, y);

      elapsed += Time.deltaTime;

      yield return null;
    }

    transform.localPosition = origin;
  }

  [ClientRpc]
  void ShakeAll(float duration, float magnitude) => Shake(duration, magnitude);
}
