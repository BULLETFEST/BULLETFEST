using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MainMenu : MonoBehaviour
{
  NetworkManager nm;

  [HideInInspector]
  public string addr;

  [HideInInspector]
  public string playerName;

  void Start()
  {
    nm = FindObjectOfType<NetworkManager>();
  }

  public void InitializeGame()
  {
    nm.networkAddress = addr;
    PlayerPrefs.SetString("name", playerName);
  }

  public void Connect()
  {
    InitializeGame();
    nm.StartClient();
  }
  public void Host()
  {
    InitializeGame();
    nm.StartHost();
  }


  public void ChangeName(string newPlayerName)
  {
    playerName = newPlayerName;
  }

  public void ChangeAddr(string newAddr)
  {
    addr = newAddr;
  }
}
