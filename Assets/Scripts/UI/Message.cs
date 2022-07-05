using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Message : MonoBehaviour
{
  public static TMP_Text title, content;

  public static Canvas canvas;

  void Start()
  {
    canvas = GetComponent<Canvas>();

    TMP_Text[] texts = GetComponentsInChildren<TMP_Text>();

    foreach (TMP_Text text in texts)
    {
      if (text.gameObject.name == "Title") title = text;
      else if (text.gameObject.name == "Content") content = text;
    }
  }

  public static void DisplayMessage(string titleText, string contentText, HorizontalAlignmentOptions alignment = HorizontalAlignmentOptions.Left)
  {
    title.text = titleText;
    content.text = contentText;
    content.horizontalAlignment = alignment;

    canvas.enabled = true;
  }


  public struct ServerMessge : Mirror.NetworkMessage
  {
    public string titleText;
    public string contentText;
    public int _alignment;
    public bool disconnect;

    public TMPro.HorizontalAlignmentOptions alignment
    {
      get
      {
        return (TMPro.HorizontalAlignmentOptions)_alignment;
      }
    }
  }
}
