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
    MouseManager.Instance.OnMouseClicked += MoveToTarget;
    MouseManager.Instance.OnEnemyClicked += EventAttact;

    GameManager.Instance.RigisterPlayer(characterStats);

    //characterStats.CurrentHealth = 2;
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
  /// �ƶ�������Ŀ��
  /// </summary>
  /// <returns></returns>
  private IEnumerator MoveToAttackTarget()
  {
    //ȡ��ֹͣ״̬������Ŀ��
    agent.isStopped = false;
    agent.stoppingDistance = characterStats.attackData.attackRange;
    transform.LookAt(attackTarget.transform);

    //��Ŀ����������룬С�ڹ�������ʱ��׷��Ŀ��
    while (Vector3.Distance(attackTarget.transform.position,
        transform.position) > characterStats.attackData.attackRange)
    {
      agent.destination = attackTarget.transform.position;
      yield return null;
    }

    //����׷��ѭ����ֹͣ�ƶ������㹥��CD
    agent.isStopped = true;
    if (lastAttackTime < 0)
    {
      //����ȴ��ϣ��жϴ˴��Ƿ񱩻���ִ�й��������ù���CD
      anim.SetBool("Critical", characterStats.isCritical);
      anim.SetTrigger("Attack");

      lastAttackTime = characterStats.attackData.coolDown;
    }
  }

  //Animation Event
  private void Hit()
  {
    var targetStats = attackTarget.GetComponent<CharacterStats>();
    targetStats.TakeDamage(characterStats, targetStats);
  }
}