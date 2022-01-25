using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyStates
{ GUARD, PATROL, CHASE, DEAD }

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(CharacterStats))]
public class EnemyController : MonoBehaviour, IEndGameObserver
{
    private EnemyStates enemyStates;//????×???

    //???????
    private NavMeshAgent agent;
    private Animator anim;
    private Collider coll;

    protected CharacterStats characterStats;

    [Header("Basic Settings")]
    public float sightRadius;//???°°???
    public bool isGuard;//??·???×?
    private float speed;//????????

    protected GameObject attackTarget;//???÷??±ê
    public float lookAtTime;//×????±?¤??????
    private float remainLookAtTime;//????×????±?¤
    private float lastAttackTime;//???÷CD
    private Quaternion guardRotation;//站桩角度

    [Header("Patrol State")]
    public float patrolRange;//????°???

    private Vector3 wayPoint;//?????·??
    private Vector3 guardPos;//???????????¨??×?????

    private bool isWalk;//×??·????
    private bool isChase;//×·??????
    private bool isFollow;//?ú??????
    private bool isDead;
    private bool playerDead;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        characterStats = GetComponent<CharacterStats>();
        coll = GetComponent<Collider>();

        speed = agent.speed;
        guardPos = transform.position;
        guardRotation = transform.rotation;
        remainLookAtTime = lookAtTime;
    }

    private void Start()
    {
        //初始化怪物状态
        if (isGuard)
        {
            enemyStates = EnemyStates.GUARD;
        }
        else
        {
            enemyStates = EnemyStates.PATROL;
            GetNewWayPoint();
        }

        //TODO:场景切换后修改掉
        GameManager.Instance.AddObserver(this);
    }

    //切换场景时启用
    //private void OnEnable()
    //{
    //    GameManager.Instance.AddObserver(this);
    //}

    private void OnDisable()
    {
        if (!GameManager.IsInitialized)
            return;

        GameManager.Instance.RemoveObserver(this);
    }

    private void Update()
    {
        if (characterStats.CurrentHealth == 0)
            isDead = true;

        if (!playerDead)
        {
            SwitchStates();
            SwitchAnimation();
            lastAttackTime -= Time.deltaTime;
        }
    }

    private void SwitchAnimation()
    {
        anim.SetBool("Walk", isWalk);
        anim.SetBool("Chase", isChase);
        anim.SetBool("Follow", isFollow);
        anim.SetBool("Critical", characterStats.isCritical);
        anim.SetBool("Death", isDead);
    }

    /// <summary>
    /// 切换状态
    /// </summary>
    private void SwitchStates()
    {
        //如果死了
        if (isDead)
            enemyStates = EnemyStates.DEAD;

        //如果发现Player，切换到CHASE
        else if (FoundPlayer())
            enemyStates = EnemyStates.CHASE;

        switch (enemyStates)
        {
            case EnemyStates.GUARD:
                isChase = false;
                if (transform.position != guardPos)
                {
                    isWalk = true;
                    agent.isStopped = false;
                    agent.destination = guardPos;

                    if (Vector3.SqrMagnitude(guardPos - transform.position) <= agent.stoppingDistance)
                    {
                        isWalk = false;
                        transform.rotation = Quaternion.Lerp(transform.rotation, guardRotation, .01f);
                    }
                }
                break;

            case EnemyStates.PATROL:
                //??????????????????????????????
                isChase = false;
                agent.speed = speed * .5f;
                //?§??????????????
                if (Vector3.Distance(wayPoint, transform.position) <= agent.stoppingDistance)
                {
                    //????????????????????????????????
                    isWalk = false;
                    if (remainLookAtTime > 0)
                    {
                        remainLookAtTime -= Time.deltaTime;
                    }
                    else
                    {
                        GetNewWayPoint();
                    }
                }
                else
                {
                    //????????????????
                    isWalk = true;
                    agent.destination = wayPoint;
                }

                break;

            case EnemyStates.CHASE:

                //???????????????
                isWalk = false;
                isChase = true;
                agent.speed = speed;

                //?§??????????????
                if (!FoundPlayer())
                {
                    isFollow = false;

                    if (remainLookAtTime > 0)
                    {
                        agent.destination = transform.position;
                        remainLookAtTime -= Time.deltaTime;
                    }
                    else if (isGuard)
                    {
                        enemyStates = EnemyStates.GUARD;
                    }
                    else
                    {
                        enemyStates = EnemyStates.PATROL;
                    }
                }
                else
                {
                    //???????????????????
                    isFollow = true;
                    agent.isStopped = false;
                    agent.destination = attackTarget.transform.position;
                }
                //??????????????
                if (TargetInAttackRange() || TargetInSkillRange())
                {
                    isFollow = false;
                    agent.isStopped = true;
                    if (lastAttackTime < 0)
                    {
                        lastAttackTime = characterStats.attackData.coolDown;
                        //?????§??
                        characterStats.isCritical = Random.value < characterStats.attackData.criticalChance;
                        //??§????
                        Attack();
                    }
                }
                break;

            case EnemyStates.DEAD:
                coll.enabled = false;
                //agent.enabled = false;
                agent.radius = 0f;
                Destroy(gameObject, 2f);
                break;
        }
    }

    /// <summary>
    /// ????
    /// </summary>
    private void Attack()
    {
        transform.LookAt(attackTarget.transform);
        if (TargetInAttackRange())
        {
            //???????????
            anim.SetTrigger("Attack");
        }
        if (TargetInSkillRange())
        {
            //???????????
            anim.SetTrigger("Skill");
        }
    }

    /// <summary>
    /// 寻找视野内的玩家——帧调用
    /// </summary>
    /// <returns></returns>
    private bool FoundPlayer()
    {
        var colliders = Physics.OverlapSphere(transform.position, sightRadius);

        foreach (var target in colliders)
        {
            if (target.CompareTag("Player"))
            {
                attackTarget = target.gameObject;
                return true;
            }
        }
        attackTarget = null;
        return false;
    }

    /// <summary>
    /// 寻找攻击范围内的玩家
    /// </summary>
    /// <returns></returns>
    private bool TargetInAttackRange()
    {
        if (attackTarget != null)
            return Vector3.Distance(attackTarget.transform.position, transform.position) <= characterStats.attackData.attackRange;
        else
            return false;
    }

    /// <summary>
    /// 寻找技能范围内的玩家
    /// </summary>
    /// <returns></returns>
    private bool TargetInSkillRange()
    {
        if (attackTarget != null)
            return Vector3.Distance(attackTarget.transform.position, transform.position) <= characterStats.attackData.skillRange;
        else
            return false;
    }

    /// <summary>
    /// 获取新的巡逻路点
    /// </summary>
    private void GetNewWayPoint()
    {
        remainLookAtTime = lookAtTime;

        float randomX = Random.Range(-patrolRange, patrolRange);
        float randomZ = Random.Range(-patrolRange, patrolRange);

        Vector3 randomPoint = new Vector3(guardPos.x + randomX,
            transform.position.y, guardPos.z + randomZ);

        NavMeshHit hit;
        wayPoint = NavMesh.SamplePosition(randomPoint, out hit,
            patrolRange, 1) ? hit.position : transform.position;
    }

    /// <summary>
    /// 绘制视野范围
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, sightRadius);
    }

    private void Hit()
    {
        if (attackTarget != null && transform.isFacingTarget(attackTarget.transform))
        {
            var targetStats = attackTarget.GetComponent<CharacterStats>();
            targetStats.TakeDamage(characterStats, targetStats);
        }
    }

    public void EndNottify()
    {
        //获胜动画
        //停止所有移动
        //停止Agent

        anim.SetBool("Win", true);
        playerDead = true;
        isChase = false;
        isWalk = false;
        attackTarget = null;
    }
}