using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;

public class PlayerObjects : NetworkBehaviour
{
  public TextMeshProUGUI uiName;

  public Rigidbody2D rb { get; set; }
  public BoxCollider2D bc { get; set; }

  // Start is called before the first frame update
  void Start()
  {
    rb = GetComponent<Rigidbody2D>();
    bc = GetComponent<BoxCollider2D>();
  }
}
