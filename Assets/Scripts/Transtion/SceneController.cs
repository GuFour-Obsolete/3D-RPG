using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;
public class SceneController : Singleton<SceneController>
{
  public GameObject playerPrefab;
  GameObject player;
  NavMeshAgent playerAgent;

  protected override void Awake()
  {
    base.Awake();
    DontDestroyOnLoad(this);
  }

  public void TranstionToDestination(TranstionPoint transtionPoint)
  {
    switch (transtionPoint.transtionType)
    {
      case TranstionPoint.TranstionType.SameScene:
        StartCoroutine(Transtion(SceneManager.GetActiveScene().name, transtionPoint.destinationTag));
        break;
      case TranstionPoint.TranstionType.DifferentScene:
        StartCoroutine(Transtion(transtionPoint.sceneName, transtionPoint.destinationTag));
        break;
    }
  }

  IEnumerator Transtion(string sceneName, TranstionDestination.DestinationTag destinationTag)
  {
    SaveManager.Instance.SavePlayerData();
    if (SceneManager.GetActiveScene().name != sceneName)
    {
      yield return SceneManager.LoadSceneAsync(sceneName);
      yield return Instantiate(playerPrefab, GetDestination(destinationTag).transform.position, GetDestination(destinationTag).transform.rotation);
      SaveManager.Instance.LoadPlayerData();
      yield break;
    }
    else
    {
      player = GameManager.Instance.playerStats.gameObject;
      playerAgent = player.GetComponent<NavMeshAgent>();
      playerAgent.enabled = false;
      player.transform.SetPositionAndRotation(GetDestination(destinationTag).transform.position, GetDestination(destinationTag).transform.rotation);
      playerAgent.enabled = true;
      yield return null;
    }
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

  public void TransitionToMain()
  {
    StartCoroutine(LoadMain());
  }

  public void TransitionToLoadGame()
  {
    StartCoroutine(LoadLevel(SaveManager.Instance.SceneName));
  }


  public void TransitionToFirstLevel()
  {
    StartCoroutine(LoadLevel("Game"));

  }

  IEnumerator LoadLevel(string scene)
  {
    if (!string.IsNullOrEmpty(scene))
    {
      yield return SceneManager.LoadSceneAsync(scene);
      yield return player = Instantiate(playerPrefab, GameManager.Instance.GetEntrance().position, GameManager.Instance.GetEntrance().rotation);

      //????
      SaveManager.Instance.SavePlayerData();
      yield break;

    }

  }

  IEnumerator LoadMain()
  {
    yield return SceneManager.LoadSceneAsync("Main");
    yield break;

  }
}
