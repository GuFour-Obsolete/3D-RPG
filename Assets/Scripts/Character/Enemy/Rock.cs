using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Rock : MonoBehaviour
{
  public enum RockStates { HitPlayer, HitEnemy, HitNothing }
  Rigidbody rb;

  RockStates rockStates;

  [Header("Basic Settings")]
  public float force;
  public int damage;
  public GameObject target;
  Vector3 direction;
  private void Start()
  {
    rb = GetComponent<Rigidbody>();
    FlyToTarget();
  }

  public void FlyToTarget()
  {
    if (target == null)
    {
      target = FindObjectOfType<PlayerController>().gameObject;
    }
    direction = (target.transform.position - transform.position + Vector3.up).normalized;
    rb.AddForce(direction * force, ForceMode.Impulse);
  }

  private void OnCollisionEnter(Collision other)
  {
    switch (rockStates)
    {
      case RockStates.HitPlayer:
        if (other.gameObject.CompareTag("Player"))
        {
          other.gameObject.GetComponent<NavMeshAgent>().isStopped = true;
          other.gameObject.GetComponent<NavMeshAgent>().velocity = direction * force;
          other.gameObject.GetComponent<Animator>().SetTrigger("Dizzy");
          other.gameObject.GetComponent<CharacterStats>().TakeDamage(damage, other.gameObject.GetComponent<CharacterStats>());
          rockStates = RockStates.HitNothing;
        }

        break;

      case RockStates.HitEnemy:
        break;

      case RockStates.HitNothing:
        break;

    }
  }

}
