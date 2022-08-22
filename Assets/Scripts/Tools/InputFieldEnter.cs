using UnityEngine;
using TMPro;

public class InputFieldEnter : MonoBehaviour
{
  TMP_InputField inputField;

  public UnityEngine.Events.UnityEvent onSubmit;

  bool allowSubmit;

  // Start is called before the first frame update
  void Start()
  {
    inputField = GetComponent<TMP_InputField>();
  }

  // Update is called once per frame
  void Update()
  {
    if (allowSubmit && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
    {
      onSubmit.Invoke();

      inputField.DeactivateInputField();// = false;
      inputField.ActivateInputField();
    }
    else
    {
      allowSubmit = inputField.isFocused;
    }
  }
}
