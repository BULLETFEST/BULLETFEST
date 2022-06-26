using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroyOnLoad : MonoBehaviour
{
  private void Start()
  {
    GameObject[] objects = GameObject.FindGameObjectsWithTag(gameObject.tag);
    if (objects.Length > 1) Destroy(gameObject);


    DontDestroyOnLoad(gameObject);
    Destroy(this);
  }
}
