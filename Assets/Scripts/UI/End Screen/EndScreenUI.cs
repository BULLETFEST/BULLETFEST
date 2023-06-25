using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class EndScreenUI : NetworkBehaviour
{
  private MyNetworkManager Room;

  [SerializeField] private Button playAgain, exit;

  // Start is called before the first frame update
  private void Awake()
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
    else
    {
      Destroy(playAgain);
    }
  }

  public void Exit()
  {
    MyNetworkManager.instance.Disconnect();
    FindObjectOfType<AudioSystem>().PlaySound("Select");
  }
}
