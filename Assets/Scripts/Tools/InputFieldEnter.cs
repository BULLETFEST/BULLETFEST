using TMPro;
using UnityEngine;

public class InputFieldEnter : MonoBehaviour
{
  private TMP_InputField inputField;

  public UnityEngine.Events.UnityEvent onSubmit;
  private bool allowSubmit;

  public TMP_InputField nextField;

  // Start is called before the first frame update
  private void Start()
  {
    inputField = GetComponent<TMP_InputField>();
  }

  // Update is called once per frame
  private void Update()
  {
    if (allowSubmit)
    {
      if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
      {
        onSubmit?.Invoke();

        inputField.DeactivateInputField();// = false;
        inputField.ActivateInputField();
      }
      else if (Input.GetKeyDown(KeyCode.Tab) && nextField != null)
      {
        nextField.Select();
      }
    }
    else
    {
      allowSubmit = inputField.isFocused;
    }
  }
}
