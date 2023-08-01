using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(EventTrigger))]
public class ButtonManager : MonoBehaviour
{
  private EventTrigger eventTrigger;

  [SerializeField] private bool soundOnHover;
  [SerializeField] private bool soundOnClick;
  [SerializeField] private bool colorOnHover;

  [DrawIf(nameof(colorOnHover), true)]
  [SerializeField] private Color color;
  private TextMeshProUGUI text;
  private Color originalColor;

  private void Awake()
  {
    eventTrigger = GetComponent<EventTrigger>();
    text = GetComponentInChildren<TextMeshProUGUI>();
    originalColor = text.color;

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
      AudioSystem.Instance.PlaySound("Hover", true);
    }

    if (colorOnHover)
    {
      text.color = color;
    }
  }

  private void PointerExit(BaseEventData eventData)
  {
    text.color = originalColor;
  }

  private void PointerClick(BaseEventData eventData)
  {
    if (soundOnClick)
    {
      AudioSystem.Instance.PlaySound("Select");
    }
  }
}