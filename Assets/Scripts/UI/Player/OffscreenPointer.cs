using UnityEngine;
using Mirror;
using System.Linq;

public class OffscreenPointer : NetworkBehaviour
{
  private RectTransform[] pointerRectTransform;

  [SerializeField] float borderSize = 25f;

  [SerializeField]
  private GameObject arrowContainer,
                     arrowPrefab;

  void Start()
  {
    if (!isLocalPlayer) Destroy(gameObject);
  }

  public override void OnStartAuthority()
  {
    base.OnStartAuthority();

    pointerRectTransform = new RectTransform[NetworkServer.connections.Count];

    for (int i = 0; i < NetworkServer.connections.Count; i++)
    {
      pointerRectTransform[i] = Instantiate(arrowPrefab, Vector3.zero, Quaternion.identity, arrowContainer.transform).GetComponent<RectTransform>();
      pointerRectTransform[i].gameObject.SetActive(false);
    }
  }

  void FixedUpdate()
  {
    PlayerNetworking[] players = GameObject.FindObjectsOfType<PlayerNetworking>();

    for (int i = 0; i < players.Length; i++)
    {
      IfIsOffScreen(players[i].gameObject, i);
    }
  }

  void IfIsOffScreen(GameObject target, int idx)
  {

    Vector3 targetPosition = target.transform.position;

    Vector3 targetPositionScreenPoint = Camera.main.WorldToScreenPoint(targetPosition);
    bool isOffScreen = targetPositionScreenPoint.x <= 0 || targetPositionScreenPoint.x >= Screen.width || targetPositionScreenPoint.y <= 0 || targetPositionScreenPoint.y >= Screen.height;

    if (isOffScreen)
    {
      pointerRectTransform[idx].gameObject.SetActive(true);

      RotatePointerTowardsTargetPosition(targetPosition, idx);

      Vector3 cappedTargetScreenPosition = targetPositionScreenPoint;
      if (cappedTargetScreenPosition.x <= borderSize) cappedTargetScreenPosition.x = borderSize;
      if (cappedTargetScreenPosition.x >= Screen.width - borderSize) cappedTargetScreenPosition.x = Screen.width - borderSize;
      if (cappedTargetScreenPosition.y <= borderSize) cappedTargetScreenPosition.y = borderSize;
      if (cappedTargetScreenPosition.y >= Screen.height - borderSize) cappedTargetScreenPosition.y = Screen.height - borderSize;

      Vector3 pointerWorldPosition = Camera.main.ScreenToWorldPoint(cappedTargetScreenPosition);
      pointerRectTransform[idx].position = pointerWorldPosition;
      pointerRectTransform[idx].localPosition = new Vector3(pointerRectTransform[idx].localPosition.x, pointerRectTransform[idx].localPosition.y, 0f);
    }
    else
    {
      pointerRectTransform[idx].gameObject.SetActive(false);
    }
  }

  private void RotatePointerTowardsTargetPosition(Vector3 targetPosition, int idx)
  {
    Vector3 toPosition = targetPosition;
    Vector3 fromPosition = Camera.main.transform.position;
    fromPosition.z = 0f;
    Vector3 dir = (toPosition - fromPosition).normalized;
    float angle = Utilities.GetAngleFromVectorFloat(dir);
    pointerRectTransform[idx].localEulerAngles = new Vector3(0, 0, angle);
  }
}
