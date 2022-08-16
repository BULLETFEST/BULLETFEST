using System.Collections.Generic;
using UnityEngine;

public class SpriteGroup : MonoBehaviour
{
  public List<SpriteRenderer> sprites;

  public void DisableAll()
  {
    foreach (SpriteRenderer sprite in sprites) sprite.enabled = false;
  }

  public void EnableAll()
  {
    foreach (SpriteRenderer sprite in sprites) sprite.enabled = true;
  }
}
