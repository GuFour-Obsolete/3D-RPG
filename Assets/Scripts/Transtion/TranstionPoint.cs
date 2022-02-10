using System.Security;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TranstionPoint : MonoBehaviour
{
  public enum TranstionType { SameScene, DifferentScene }

  [Header("Transtion Info")]
  public string sceneName;
  public TranstionType transtionType;
  public TranstionDestination.DestinationTag destinationTag;
  bool canTrans;

  private void Update()
  {
    if (Input.GetKeyDown(KeyCode.E) && canTrans)
    {
      SceneController.Instance.TranstionToDestination(this);
    }
  }

  void OnTriggerStay(Collider other)
  {
    if (other.CompareTag("Player"))
    {
      canTrans = true;
    }
  }

  private void OnTriggerExit(Collider other)
  {
    if (other.CompareTag("Player"))
    {
      canTrans = false;
    }
  }
}
