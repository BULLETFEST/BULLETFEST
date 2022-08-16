using System.Collections;
using System.Linq;
using UnityEngine;
using Mirror;
using System;

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

  [TargetRpc]
  public void SetWinnerText(NetworkConnection conn, string text)
  {
    GameObject.FindObjectOfType<WinnerUI>().winnerText.text = text;
  }
}
