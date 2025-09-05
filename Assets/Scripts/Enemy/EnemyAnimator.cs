using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class EnemyAnimator : MonoBehaviour
{
    [Header("Animation Speeds")]
    [SerializeField] float patrolAnimSpeed = 1.0f;
    [SerializeField] float chaseAnimSpeed  = 1.6f;

    [Header("Tuning")]
    [Tooltip("Velocidade mínima para considerar 'em movimento'.")]
    [SerializeField] float idleThreshold = 0.05f;

    [Tooltip("Tempo mínimo entre mudanças de direção (anti-flicker).")]
    [SerializeField] float dirMinSwitchInterval = 0.08f;

    [Tooltip("Se > 0, suaviza a direção mostrada (segundos de suavização).")]
    [SerializeField] float dirSmoothTime = 0.10f;

    Rigidbody2D rb;
    Animator anim;

    Vector2 lastDir = Vector2.down;      // direção “parado olhando”
    Vector2 lastPos;
    float lastDirSwitchTime;

    // Suavização opcional
    Vector2 dirVelSmooth;
    Vector2 smoothedDir = Vector2.right;

    // === OVERRIDE DE DIREÇÃO (usado pelo Patrol ao 'olhar 4x') ===
    Vector2? facingOverride = null;
    public void SetFacingOverride(Vector2? dir) { facingOverride = dir; }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        anim.speed = patrolAnimSpeed;
        lastPos = rb.position;
        smoothedDir = Vector2.right;
    }

    void Update()
    {
        // 1) velocidade “oficial”
        Vector2 v = rb.linearVelocity;

        // 2) fallback por delta de posição (caso o rb.linearVelocity esteja zerado no frame)
        if (v.sqrMagnitude < 0.0001f)
        {
            Vector2 cur = rb.position;
            v = (cur - lastPos) / Mathf.Max(Time.deltaTime, 0.0001f);
            lastPos = cur;
        }

        // 3) determina se está se movendo
        bool moving = v.sqrMagnitude > idleThreshold * idleThreshold;

        // 4) escolhe direção base
        Vector2 baseDir = moving ? v.normalized : (facingOverride ?? lastDir);

        // 5) suaviza direção para evitar flip rápido (opcional)
        Vector2 shownDir = baseDir;
        if (dirSmoothTime > 0f)
        {
            smoothedDir = Vector2.SmoothDamp(smoothedDir, baseDir, ref dirVelSmooth, dirSmoothTime);
            if (smoothedDir.sqrMagnitude > 0.0001f) shownDir = smoothedDir.normalized;
        }

        // 6) atualiza lastDir quando de fato está se movendo
        if (moving) lastDir = shownDir;

        // 7) aplica nos parâmetros com anti-flicker na direção
        anim.SetBool("IsMoving", moving);

        float newDirIdx = ToDirIndex(shownDir); // 0 F, 1 L, 2 B, 3 R
        float curDirIdx = anim.GetFloat("Dir");

        if (Time.time - lastDirSwitchTime >= dirMinSwitchInterval || Mathf.Approximately(newDirIdx, curDirIdx))
        {
            anim.SetFloat("Dir", newDirIdx);
            lastDirSwitchTime = Time.time;
        }
    }

    public void SetChase(bool on) => anim.speed = on ? chaseAnimSpeed : patrolAnimSpeed;

    // Mapeia vetor para 4 direções cardeais (0=Front, 1=Left, 2=Back, 3=Right)
    float ToDirIndex(Vector2 d)
    {
        if (Mathf.Abs(d.x) > Mathf.Abs(d.y))
            return d.x < 0 ? 1f : 3f; // Left / Right
        else
            return d.y > 0 ? 2f : 0f; // Back / Front
    }
}
