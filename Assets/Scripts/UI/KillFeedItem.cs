using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class KillFeedItem : MonoBehaviour
{
  public TextMeshProUGUI killer, killed;

  public float fadeOutTime, fadeOutDelay, fadeAmount;

  CanvasGroup canvasGroup;
  void Start()
  {
    StartCoroutine(FadeOut());
    canvasGroup = GetComponent<CanvasGroup>();

  }

  IEnumerator FadeOut()
  {
    yield return new WaitForSecondsRealtime(fadeOutDelay);

    while (canvasGroup.alpha > 0.01f)
    {
      canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 0, fadeOutTime * Time.deltaTime);
      yield return null;

    }

    Destroy(gameObject);
  }
}
