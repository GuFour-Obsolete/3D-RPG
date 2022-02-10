using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
  private NavMeshAgent agent;

  private Animator anim;
  private CharacterStats characterStats;

  private GameObject attackTarget;
  private float lastAttackTime;
  private bool isDead;

  public float stopDistence;

  private void Awake()
  {
    agent = GetComponent<NavMeshAgent>();
    anim = GetComponent<Animator>();
    characterStats = GetComponent<CharacterStats>();

    stopDistence = agent.stoppingDistance;
  }

  private void Start()
  {
    SaveManager.Instance.LoadPlayerData();
  }

  private void OnEnable()
  {
    MouseManager.Instance.OnMouseClicked += MoveToTarget;
    MouseManager.Instance.OnEnemyClicked += EventAttact;
    GameManager.Instance.RigisterPlayer(characterStats);
  }

  private void OnDisable()
  {
    if (!MouseManager.IsInitialized) return;
    MouseManager.Instance.OnMouseClicked -= MoveToTarget;
    MouseManager.Instance.OnEnemyClicked -= EventAttact;
  }

  private void Update()
  {
    isDead = characterStats.CurrentHealth == 0;

    if (isDead)
      GameManager.Instance.NotifyObserver();

    SwitchAnimation();

    lastAttackTime -= Time.deltaTime;
  }

  private void SwitchAnimation()
  {
    anim.SetFloat("Speed", agent.velocity.sqrMagnitude);
    anim.SetBool("Death", isDead);
  }

  public void MoveToTarget(Vector3 target)
  {
    StopAllCoroutines();
    if (isDead)
      return;
    agent.stoppingDistance = stopDistence;
    agent.isStopped = false;
    agent.destination = target;
  }

  private void EventAttact(GameObject target)
  {
    if (isDead)
      return;
    if (target != null)
    {
      attackTarget = target;
      characterStats.isCritical = UnityEngine.Random.value < characterStats.attackData.criticalChance;
      StartCoroutine(MoveToAttackTarget());
    }
  }

  /// <summary>
  /// 移动到攻击目标
  /// </summary>
  /// <returns></returns>
  private IEnumerator MoveToAttackTarget()
  {
    //取消停止状态，看向目标
    agent.isStopped = false;
    agent.stoppingDistance = characterStats.attackData.attackRange;
    transform.LookAt(attackTarget.transform);

    //当目标与任务距离，小于攻击距离时，追逐目标
    while (Vector3.Distance(attackTarget.transform.position,
        transform.position) > characterStats.attackData.attackRange)
    {
      agent.destination = attackTarget.transform.position;
      yield return null;
    }

    //跳出追逐循环后，停止移动，计算攻击CD
    agent.isStopped = true;
    if (lastAttackTime < 0)
    {
      //若冷却完毕，判断此次是否暴击，执行攻击并重置攻击CD
      anim.SetBool("Critical", characterStats.isCritical);
      anim.SetTrigger("Attack");

      lastAttackTime = characterStats.attackData.coolDown;
    }
  }

  //Animation Event
  private void Hit()
  {
    if (attackTarget.CompareTag("Attackable"))
    {
      if (attackTarget.GetComponent<Rock>() && attackTarget.GetComponent<Rock>().rockStates == Rock.RockStates.HitNothing)
      {
        attackTarget.GetComponent<Rock>().rockStates = Rock.RockStates.HitEnemy;
        attackTarget.GetComponent<Rigidbody>().velocity = Vector3.one * 1.1f;
        attackTarget.GetComponent<Rigidbody>().AddForce(transform.forward * 20f, ForceMode.Impulse);

      }
    }
    else
    {
      var targetStats = attackTarget.GetComponent<CharacterStats>();
      targetStats.TakeDamage(characterStats, targetStats);
    }

  }
}