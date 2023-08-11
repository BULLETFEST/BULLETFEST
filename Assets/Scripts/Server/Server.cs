using System;
using System.Collections;
using Mirror;
using UnityEngine;

public class Server : NetworkBehaviour
{
  PlayerSpawnSystem system;

  public override void OnStartServer()
  {
    base.OnStartServer();

    if (GameManager.settings.gameMode == GameSettings.GameMode.Deathmatch)
    {
      StartCoroutine(CalcTimeLeft());
    }

    system = FindAnyObjectByType<PlayerSpawnSystem>();
  }

  [ClientRpc]
  public void Rpc_PlaySoundAll(string sound, bool interrupt, bool varyPitch)
  {
    AudioSystem.Instance.PlaySound(sound, interrupt, varyPitch);
  }

  [ClientRpc]
  public void Rpc_PlaySoundAll(string sound)
  {
    AudioSystem.Instance.PlaySound(sound);
  }

  [TargetRpc]
  public void Target_PlaySound(NetworkConnectionToClient target, string sound, bool interrupt, bool varyPitch)
  {
    AudioSystem.Instance.PlaySound(sound, interrupt, varyPitch);
  }

  [TargetRpc]
  public void Target_PlaySound(NetworkConnectionToClient target, string sound)
  {
    AudioSystem.Instance.PlaySound(sound);
  }

  [Server]
  private IEnumerator CalcTimeLeft()
  {
    // Temporarily set time
    TimeSpan timeSpan = new(0, 1, 0);
    while (timeSpan.TotalSeconds >= 0)
    {
      timeSpan = system.timeStamp.Subtract(DateTime.UtcNow);
      Rpc_UpdateTimer($"{timeSpan.Minutes}:{(timeSpan.Seconds < 10 ? "0" + timeSpan.Seconds.ToString() : timeSpan.Seconds)}");

      yield return new WaitForSecondsRealtime(1);
    }

    GameManager.Instance.AnnounceWinner(null, false);
  }

  [ClientRpc]
  private void Rpc_UpdateTimer(string timeString)
  {
    gameObject.GetComponent<PlayerUI>().UpdateTimer(timeString);
  }
}
