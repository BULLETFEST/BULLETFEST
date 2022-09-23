using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Spikes : NetworkBehaviour
{
  Dictionary<GameObject, Coroutine> dict = new();

  [ServerCallback]
  private void OnCollisionEnter2D(Collision2D other)
  {
    if (!other.gameObject.GetComponent<DamageController>()) return;

    dict.Add(other.gameObject, StartCoroutine(DealDamage(other.gameObject)));
  }

  [ServerCallback]
  private void OnCollisionExit2D(Collision2D other)
  {
    if (!other.gameObject.GetComponent<DamageController>()) return;

    StopCoroutine(dict[other.gameObject]);
    dict.Remove(other.gameObject);
  }

  [ServerCallback]
  IEnumerator DealDamage(GameObject go)
  {
    go.GetComponent<DamageController>().TakeDamage(2.5f, null);
    yield return new WaitForSecondsRealtime(1.75f);
    dict[go] = StartCoroutine(DealDamage(go));
  }
}
