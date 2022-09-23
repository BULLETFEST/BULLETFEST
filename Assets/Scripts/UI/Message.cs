using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Message : MonoBehaviour
{
  public static TMP_Text title, content;

  public static Canvas canvas;

  public static Button closeBtn;

  void Start()
  {
    canvas = GetComponent<Canvas>();

    closeBtn = GetComponentInChildren<Button>();

    TMP_Text[] texts = GetComponentsInChildren<TMP_Text>();

    foreach (TMP_Text text in texts)
    {
      if (text.gameObject.name == "Title") title = text;
      else if (text.gameObject.name == "Content") content = text;
    }
  }

  public static void DisplayMessage(string titleText, string contentText, bool closable, HorizontalAlignmentOptions alignment = HorizontalAlignmentOptions.Left)
  {
    DisplayMessage(titleText, contentText, alignment);

    closeBtn.gameObject.SetActive(closable);
  }

  public static void DisplayMessage(string titleText, string contentText, HorizontalAlignmentOptions alignment = HorizontalAlignmentOptions.Left)
  {
    closeBtn.gameObject.SetActive(true);

    title.text = titleText;
    content.text = contentText;
    content.horizontalAlignment = alignment;

    canvas.enabled = true;
  }

  public static void DisplayMessage(string titleText, string contentText, System.Action onClose, HorizontalAlignmentOptions alignment = HorizontalAlignmentOptions.Left)
  {
    closeBtn.gameObject.SetActive(true);

    title.text = titleText;
    content.text = contentText;
    content.horizontalAlignment = alignment;


    closeBtn.onClick.AddListener(delegate
    {
      onClose();
      closeBtn.onClick.RemoveAllListeners();
      closeBtn.onClick.AddListener(() => HideMessage());
    });

    canvas.enabled = true;
  }

  public static void HideMessage()
  {
    canvas.enabled = false;
  }

  public struct ServerMessge : Mirror.NetworkMessage
  {
    public string titleText;
    public string contentText;
    public int _alignment;
    public bool disconnect;

    public HorizontalAlignmentOptions alignment
    {
      get
      {
        return (HorizontalAlignmentOptions)_alignment;
      }
    }
  }
}
