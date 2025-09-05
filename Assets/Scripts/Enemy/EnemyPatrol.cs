using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class EnemyPatrol : MonoBehaviour
{
    [Header("Path")]
    [SerializeField] Transform pathRoot;
    [SerializeField] Transform[] waypoints;

    [Header("Movement")]
    [SerializeField] float speed = 3f;
    public float Speed { get => speed; set => speed = value; }

    [SerializeField] float arriveThreshold = 0.1f;
    [SerializeField] float waitAtPoint = 0.2f;
    [SerializeField] bool pingPong = false;

    [Header("Rotation")]
    [Tooltip("Se verdadeiro, gira o Transform na direção do movimento.")]
    [SerializeField] bool rotateTransform = true;

    [Header("Look Around")]
    [SerializeField] bool lookFourOnPause = true;
    [SerializeField] float lookStepDuration = 0.25f;
    [Tooltip("Se verdadeiro: Right→Down→Left→Up (horário). Se falso: Right→Up→Left→Down (anti-horário).")]
    [SerializeField] bool lookClockwise = true;

    int index = 0, dirSign = 1;
    float waitTimer = 0f;
    Rigidbody2D rb;

    // refs
    EnemyVisionFOV vision;
    EnemyAnimator enemyAnim;

    // controle
    bool lookingRoutineRunning = false;
    Vector2 lastMoveDir = Vector2.right;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        vision = GetComponent<EnemyVisionFOV>();
        enemyAnim = GetComponent<EnemyAnimator>();

        if (pathRoot && (waypoints == null || waypoints.Length == 0))
        {
            int c = pathRoot.childCount;
            waypoints = new Transform[c];
            for (int i = 0; i < c; i++) waypoints[i] = pathRoot.GetChild(i);
        }
    }

    void FixedUpdate()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Transform target = waypoints[index];
        Vector2 pos = rb.position;
        Vector2 to = (Vector2)target.position - pos;
        float dist = to.magnitude;

        if (dist <= arriveThreshold)
        {
            // Zera movimento
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;

            // Quando olhar 4x está ligado:
            if (lookFourOnPause)
            {
                // se ainda não começou, dispara e sai
                if (!lookingRoutineRunning)
                    StartCoroutine(LookAroundThenAdvance());

                // EVITA o avanço por waitAtPoint enquanto a rotina roda
                return;
            }

            // Comportamento padrão (sem olhar 4x)
            waitTimer += Time.fixedDeltaTime;
            if (waitTimer >= waitAtPoint)
            {
                waitTimer = 0f;
                if (pingPong)
                {
                    if (index == waypoints.Length - 1) dirSign = -1;
                    else if (index == 0) dirSign = 1;
                    index += dirSign;
                }
                else index = (index + 1) % waypoints.Length;
            }
            return;
        }

        Vector2 dir = to.normalized;
        Vector2 next = pos + dir * speed * Time.fixedDeltaTime;
        rb.MovePosition(next);

        // >>> alimenta Animator
        rb.linearVelocity = dir * speed;
        lastMoveDir = dir;

        if (rotateTransform && dir.sqrMagnitude > 0.0001f)
        {
            float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            rb.MoveRotation(ang);
        }
        else rb.angularVelocity = 0f;
    }

    IEnumerator LookAroundThenAdvance()
    {
        lookingRoutineRunning = true;

        // direção inicial = última direção de movimento
        Vector2 startDir = (lastMoveDir.sqrMagnitude < 0.0001f) ? Vector2.right : lastMoveDir.normalized;

        // ordem fixa
        Vector2[] cw  = new[] { Vector2.right, Vector2.down, Vector2.left, Vector2.up  }; // horário
        Vector2[] ccw = new[] { Vector2.right, Vector2.up,   Vector2.left, Vector2.down }; // anti-horário
        var order = lookClockwise ? cw : ccw;

        // acha índice inicial (direção do array mais próxima da direção atual)
        int startIdx = 0;
        float bestDot = -999f;
        for (int i = 0; i < 4; i++)
        {
            float d = Vector2.Dot(order[i], startDir);
            if (d > bestDot) { bestDot = d; startIdx = i; }
        }

        // sequência: inicial -> +1 -> +2 -> +3 -> inicial (todos módulo 4)
        for (int k = 0; k < 4; k++)
            yield return LookStep(order[(startIdx + k) & 3]);
        yield return LookStep(order[startIdx]); // voltar para a inicial

        // limpa overrides
        vision?.SetFacingOverride(null);
        enemyAnim?.SetFacingOverride(null);

        // avança imediatamente
        waitTimer = 0f;
        if (pingPong)
        {
            if (index == waypoints.Length - 1) dirSign = -1;
            else if (index == 0) dirSign = 1;
            index += dirSign;
        }
        else index = (index + 1) % waypoints.Length;

        lookingRoutineRunning = false;
    }

    IEnumerator LookStep(Vector2 dir)
    {
        vision?.SetFacingOverride(dir);
        enemyAnim?.SetFacingOverride(dir);

        if (rotateTransform)
        {
            float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            rb.MoveRotation(ang);
        }

        float t = 0f;
        while (t < lookStepDuration)
        {
            rb.linearVelocity = Vector2.zero; // parado durante o step
            t += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
    }
}
