using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class EndScreenUI : NetworkBehaviour
{
  MyNetworkManager Room;

  [SerializeField] private Button playAgain, exit;

  // Start is called before the first frame update
  void Awake()
  {
    Room = MyNetworkManager.instance;
    Cursor.visible = true;
  }

  public override void OnStartClient()
  {
    base.OnStartClient();

    if (Room.isHost)
    {
      playAgain.gameObject.SetActive(true);

      playAgain.onClick.AddListener(delegate
      {
        Room.ServerChangeScene("Lobby");
        FindObjectOfType<AudioSystem>().PlaySound("Select");
      });
    }
    else Destroy(playAgain);
  }

  public void Exit()
  {
    Room.Disconnect();
    FindObjectOfType<AudioSystem>().PlaySound("Select");
  }
}
