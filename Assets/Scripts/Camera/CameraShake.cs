using System.Collections;
using Mirror;
using UnityEngine;

public class CameraShake : NetworkBehaviour
{
  private bool isShaking = false;
  private Vector3 origin = new(0, 0, -10);

  public IEnumerator Shake(float duration, float magnitude)
  {
    if (!isShaking)
    {
      origin = transform.position;
    }

    isShaking = true;

    float elapsed = 0.0f;

    while (elapsed < duration)
    {
      float x = Random.Range(-1f, 1f) * magnitude;
      float y = Random.Range(-1f, 1f) * magnitude;

      transform.position = new Vector3(x, y, -10);

      elapsed += Time.unscaledDeltaTime;

      yield return null;
    }

    transform.position = origin;
    isShaking = false;
  }

  [ClientRpc]
  public void ShakeAll(float duration, float magnitude)
  {
    StartCoroutine(Shake(duration, magnitude));
  }
}
