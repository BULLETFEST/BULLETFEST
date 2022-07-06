using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Spikes : NetworkBehaviour
{
  Dictionary<GameObject, Coroutine> dict = new();

  [ServerCallback]
  private void OnCollisionEnter2D(Collision2D other)
  {
    if (other.gameObject.tag != "Player") return;

    dict.Add(other.gameObject, StartCoroutine(DealDamage(other.gameObject)));
  }

  [ServerCallback]
  private void OnCollisionExit2D(Collision2D other)
  {
    if (other.gameObject.tag != "Player") return;

    StopCoroutine(dict[other.gameObject]);
    dict.Remove(other.gameObject);
  }

  [ServerCallback]
  IEnumerator DealDamage(GameObject go)
  {
    go.GetComponent<PlayerBehavior>().TakeDamage(2.5f, null);
    yield return new WaitForSecondsRealtime(1.75f);
    dict[go] = StartCoroutine(DealDamage(go));
  }
}
