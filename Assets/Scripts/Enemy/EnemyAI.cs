using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
    public enum State { Patrol, Chase, Search }

    [Header("Refs")]
    [SerializeField] EnemyPatrol patrol;
    [SerializeField] EnemyVisionFOV vision;
    [SerializeField] Transform player;
    [SerializeField] PlayerHide playerHide;
    [SerializeField] EnemyAnimator enemyAnim;

    [Header("Patrol")] [SerializeField] float patrolSpeed = 3f;
    [Header("Chase")]  [SerializeField] float chaseSpeed = 5f;
    [SerializeField] float fovExpandMultiplier = 1.6f;
    [SerializeField] float fovShrinkPerSecond = 0.25f;
    [SerializeField] float repathInterval = 0.05f;
    [Header("Lose/Search")] [SerializeField] float loseSightTime = 2.0f;
    [SerializeField] float searchTime = 1.5f;
    [Header("Catch")] [SerializeField] float catchDistance = 0.5f;
    [SerializeField] bool captureOnHideInSight = true;

    State state = State.Patrol;
    Rigidbody2D rb;
    float baseViewRadius, baseViewAngle, loseSightTimer, repathTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (!vision) vision = GetComponent<EnemyVisionFOV>();
        if (!patrol) patrol = GetComponent<EnemyPatrol>();
        if (!enemyAnim) enemyAnim = GetComponent<EnemyAnimator>();

        if (!player) { var p = GameObject.FindWithTag("Player"); if (p) player = p.transform; }
        if (!playerHide && player) playerHide = player.GetComponent<PlayerHide>();

        baseViewRadius = vision.ViewRadius;
        baseViewAngle  = vision.ViewAngle;
        if (patrol) patrol.Speed = patrolSpeed;
        SetState(State.Patrol);
    }

    void Update()
    {
        switch (state)
        {
            case State.Patrol: if (vision.IsSeeingPlayer) EnterChase(); break;
            case State.Chase:  ChaseUpdate(); break;
            case State.Search: SearchUpdate(); break;
        }
    }

    void ChaseUpdate()
    {
        if (captureOnHideInSight && playerHide && playerHide.IsHidden && vision.IsSeeingPlayer) { CapturePlayer(); return; }

        if (player)
        {
            repathTimer += Time.deltaTime;
            if (repathTimer >= repathInterval)
            {
                float dt = repathTimer; repathTimer = 0f;
                MoveTowards(player.position, chaseSpeed, dt);
            }
        }

        if (vision.IsSeeingPlayer) loseSightTimer = 0f;
        else { loseSightTimer += Time.deltaTime; if (loseSightTimer >= loseSightTime) { EnterSearch(); return; } }

        if (player && Vector2.Distance(rb.position, player.position) <= catchDistance) { CapturePlayer(); return; }

        RelaxFOV(Time.deltaTime);
    }

    void SearchUpdate()
    {
        if (vision.IsSeeingPlayer) { EnterChase(); return; }
        loseSightTimer += Time.deltaTime;
        if (loseSightTimer >= searchTime) EnterPatrol();
        RelaxFOV(Time.deltaTime);
    }

    void SetState(State s) { state = s; if (patrol) patrol.enabled = (s == State.Patrol); if (s == State.Patrol && patrol) patrol.Speed = patrolSpeed; }
    void EnterPatrol(){ vision.ViewRadius=baseViewRadius; vision.ViewAngle=baseViewAngle; loseSightTimer=0; repathTimer=0; enemyAnim?.SetChase(false); SetState(State.Patrol); }
    void EnterChase(){ vision.ViewRadius=baseViewRadius*fovExpandMultiplier; vision.ViewAngle=baseViewAngle*Mathf.Lerp(1f,fovExpandMultiplier,0.5f); loseSightTimer=0; repathTimer=0; enemyAnim?.SetChase(true); SetState(State.Chase); }
    void EnterSearch(){ loseSightTimer=0; repathTimer=0; enemyAnim?.SetChase(false); SetState(State.Search); }
    void RelaxFOV(float dt){ vision.ViewRadius=Mathf.MoveTowards(vision.ViewRadius,baseViewRadius,baseViewRadius*fovShrinkPerSecond*dt); vision.ViewAngle=Mathf.MoveTowards(vision.ViewAngle,baseViewAngle,baseViewAngle*fovShrinkPerSecond*dt); }
    void CapturePlayer()=> GameManager.Instance?.GameOver();

    void MoveTowards(Vector2 target, float speed, float dt)
    {
        Vector2 pos = rb.position;
        Vector2 dir = (target - pos);
        if (dir.sqrMagnitude < 0.000001f) return;
        dir.Normalize();

        Vector2 next = pos + dir * speed * dt;
        rb.MovePosition(next);

        // >>> alimenta Animator no chase
        rb.linearVelocity = dir * speed;
    }
}
