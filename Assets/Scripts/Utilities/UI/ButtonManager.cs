using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(EventTrigger))]
public class ButtonManager : MonoBehaviour
{
  private EventTrigger eventTrigger;

  [SerializeField] private bool soundOnHover;
  [SerializeField] private bool soundOnClick;
  [SerializeField] private bool colorOnHover;
  [SerializeField] private bool imageOnHover;

  [DrawIf(nameof(colorOnHover), true)]
  [SerializeField] private Color color;

  [DrawIf(nameof(soundOnHover), true)]
  [SerializeField] private string hoverSound = "Hover";

  [DrawIf(nameof(imageOnHover), true)]
  [SerializeField] private Sprite onHoverImage;
  private Sprite originalImage;

  private TextMeshProUGUI text;
  private Color originalColor;

  private Image image;

  private void Awake()
  {
    image = GetComponent<Image>();
    eventTrigger = GetComponent<EventTrigger>();
    text = GetComponentInChildren<TextMeshProUGUI>();

    if (text != null)
    {
      originalColor = text.color;
    }

    if (image != null)
    {
      originalImage = image.sprite;
    }

    EventTrigger.Entry entry = new()
    {
      eventID = EventTriggerType.PointerEnter
    };
    entry.callback.AddListener((eventData) => PointerEnter(eventData));
    eventTrigger.triggers.Add(entry);


    entry = new()
    {
      eventID = EventTriggerType.PointerExit
    };
    entry.callback.AddListener((eventData) => PointerExit(eventData));
    eventTrigger.triggers.Add(entry);

    entry = new()
    {
      eventID = EventTriggerType.PointerClick
    };
    entry.callback.AddListener((eventData) => PointerClick(eventData));
    eventTrigger.triggers.Add(entry);
  }

  private void PointerEnter(BaseEventData eventData)
  {
    if (soundOnHover)
    {
      AudioSystem.Instance.PlaySound(hoverSound, true);
    }

    if (colorOnHover)
    {
      text.color = color;
    }

    if (imageOnHover)
    {
      image.sprite = onHoverImage;
    }
  }

  private void PointerExit(BaseEventData eventData)
  {
    if (colorOnHover)
    {
      text.color = originalColor;
    }

    if (imageOnHover)
    {
      image.sprite = originalImage;
    }
  }

  private void PointerClick(BaseEventData eventData)
  {
    if (soundOnClick)
    {
      AudioSystem.Instance.PlaySound("Select");
    }
  }
}