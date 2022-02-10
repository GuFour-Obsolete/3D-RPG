using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class GameManager : Singleton<GameManager>
{
  public CharacterStats playerStats;

  private List<IEndGameObserver> endGameObservers = new List<IEndGameObserver>();

  CinemachineFreeLook followCamera;

  protected override void Awake()
  {
    base.Awake();
    DontDestroyOnLoad(this);
  }
  public void RigisterPlayer(CharacterStats player)
  {
    playerStats = player;
    followCamera = FindObjectOfType<CinemachineFreeLook>();
    if (followCamera)
    {
      followCamera.Follow = playerStats.transform.GetChild(2);
      followCamera.LookAt = playerStats.transform.GetChild(2);
    }
  }

  public void AddObserver(IEndGameObserver observer)
  {
    endGameObservers.Add(observer);
  }

  public void RemoveObserver(IEndGameObserver observer)
  {
    endGameObservers.Remove(observer);
  }

  /// <summary>
  /// ¹ã²¥
  /// </summary>
  public void NotifyObserver()
  {
    foreach (var item in endGameObservers)
    {
      item.EndNottify();
    }
  }

  public Transform GetEntrance()
  {
    foreach (var item in FindObjectsOfType<TranstionDestination>())
    {
      if (item.destinationTag == TranstionDestination.DestinationTag.ENTER)
      {
        return item.transform;
      }
    }
    return null;

  }
}