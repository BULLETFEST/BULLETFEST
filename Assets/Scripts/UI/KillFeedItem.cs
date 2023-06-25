using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KillFeedItem : MonoBehaviour
{
  public TMP_Text killer, killed;

  public Image weapon;

  public float fadeOutTime, fadeOutDelay, fadeAmount;
  private CanvasGroup canvasGroup;

  private void Start()
  {
    _ = StartCoroutine(FadeOut());
    canvasGroup = GetComponent<CanvasGroup>();

  }

  private IEnumerator FadeOut()
  {
    yield return new WaitForSecondsRealtime(fadeOutDelay);

    while (canvasGroup.alpha > 0.01f)
    {
      canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 0, fadeOutTime * Time.unscaledDeltaTime);
      yield return null;

    }

    Destroy(gameObject);
  }
}
