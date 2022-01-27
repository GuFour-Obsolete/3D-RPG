using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : Singleton<SceneController>
{
  GameObject player;
  public void TranstionToDestination(TranstionPoint transtionPoint)
  {
    switch (transtionPoint.transtionType)
    {

      case TranstionPoint.TranstionType.SameScene:
        StartCoroutine(Transtion(SceneManager.GetActiveScene().name, transtionPoint.destinationTag));
        break;
      case TranstionPoint.TranstionType.DifferentScene:

        break;
    }
  }

  IEnumerator Transtion(string sceneName, TranstionDestination.DestinationTag destinationTag)
  {
    player = GameManager.Instance.playerStats.gameObject;
    player.transform.SetPositionAndRotation(GetDestination(destinationTag).transform.position, GetDestination(destinationTag).transform.rotation);
    yield return null;
  }

  TranstionDestination GetDestination(TranstionDestination.DestinationTag destinationTag)
  {
    var entrances = FindObjectsOfType<TranstionDestination>();
    for (int i = 0; i < entrances.Length; i++)
    {
      if (entrances[i].destinationTag == destinationTag)
      {
        return entrances[i];
      }
    }
    return null;
  }
}
