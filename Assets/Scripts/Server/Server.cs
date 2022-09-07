using System.Collections;
using System.Linq;
using UnityEngine;
using Mirror;
using System;
using UnityEngine.UI;

public class Server : NetworkBehaviour
{
  Coroutine timerRoutine;

  public override void OnStartAuthority()
  {
    base.OnStartAuthority();
    if (!isServer)
    {
      // Destroy(GetComponent<PlayerVars>().publicCanvas.gameObject);
      Destroy(GetComponent<Server>());
    }
    if ((int)FindObjectOfType<MyNetworkManager>().gameMode == 1) timerRoutine = StartCoroutine(CalcTimeLeft());
  }

  IEnumerator CalcTimeLeft()
  {
    TimeSpan timeSpan = new TimeSpan(0, 5, 0);
    while (timeSpan.TotalSeconds >= 0)
    {
      timeSpan = FindObjectOfType<PlayerSpawnSystem>().timeStamp.Subtract(DateTime.UtcNow);//FindObjectOfType<PlayerSpawnSystem>().timeStamp.Subtract(DateTime.Now);
      // int secondsLeft = (int)timeSpan.Minutes;
      // uiTimeLeft.text = $"{Mathf.Floor(secondsLeft / 60)}:{Mathf.Floor(secondsLeft / Mathf.Floor(secondsLeft / 60))}";
      Cmd_UpdateTimer($"{timeSpan.Minutes}:{(timeSpan.Seconds < 10 ? "0" + timeSpan.Seconds.ToString() : timeSpan.Seconds)}");

      yield return new WaitForSeconds(1);
    }

    FindObjectOfType<MyNetworkManager>().AnnounceWinner();
  }

  [Command(requiresAuthority = false)]
  void Cmd_UpdateTimer(string timeString)
  {
    foreach (NetworkConnectionToClient conn in NetworkServer.connections.Values.ToArray())
    {
      Rpc_UpdateTimer(conn, timeString);
    }
  }

  [TargetRpc]
  void Rpc_UpdateTimer(NetworkConnection conn, string timeString)
  {
    conn.identity.GetComponent<PlayerUI>().uiTimeLeft.text = timeString;
  }

  Color[] colors = new Color[] {
    new Color(0.5882353f, 0.1137255f, 0.04313726f), // 961D0B
    new Color(0.0993236f, 0.4487756f, 0.6792453f), // 1972AD
    new Color(0.1027946f, 0.6226415f, 0.1877513f), // 1A9F30
    new Color(0.6235294f, 0.6018561f, 0.1019608f), // 9F991A
  };

  [TargetRpc]
  public void SetWinnerText(NetworkConnection conn, string text, int idx)
  {
    GameObject.FindObjectOfType<WinnerUI>().winnerText.text = text;
    GameObject.FindObjectOfType<WinnerUI>().GetComponentInChildren<Image>().color = colors[idx];
  }
}
