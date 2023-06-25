using UnityEngine;

public class DontDestroyOnLoad : MonoBehaviour
{
  private void Start()
  {
    GameObject[] objects = GameObject.FindGameObjectsWithTag(gameObject.tag);
    if (objects.Length > 1)
    {
      Destroy(gameObject);
    }

    transform.SetParent(null);
    DontDestroyOnLoad(gameObject);
    Destroy(this);
  }
}
