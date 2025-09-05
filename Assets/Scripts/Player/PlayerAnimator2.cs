using UnityEngine;

[DisallowMultipleComponent]
[AddComponentMenu("Player/PlayerAnimator2 (root -> usa gfx)")]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerAnimator2 : MonoBehaviour
{
    [SerializeField] float idleThreshold = 0.05f;

    // opcional: arraste aqui o filho visual (se não arrastar, eu procuro sozinho)
    [SerializeField] Transform gfx;

    Rigidbody2D rb;
    Animator anim;              // no filho (gfx)
    SpriteRenderer sr;          // no filho (gfx)
    Vector2 lastDir = Vector2.down;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // se não tiver referência, tenta achar automaticamente um filho com Animator/SpriteRenderer
        if (gfx == null)
        {
            anim = GetComponentInChildren<Animator>(true);
            if (anim != null) gfx = anim.transform;
        }
        else
        {
            anim = gfx.GetComponentInChildren<Animator>(true);
        }

        if (anim == null)
        {
            Debug.LogError("[PlayerAnimator2] Não encontrei Animator no filho 'gfx'. " +
                           "Coloque o Animator no objeto visual ou arraste o 'gfx' no inspector.", this);
            enabled = false;
            return;
        }

        sr = anim.GetComponentInChildren<SpriteRenderer>(true);
        if (sr == null)
        {
            Debug.LogError("[PlayerAnimator2] Não encontrei SpriteRenderer no filho 'gfx'.", this);
            enabled = false;
            return;
        }
    }

    void Update()
    {
        Vector2 v = rb.linearVelocity;
        bool moving = v.sqrMagnitude > idleThreshold * idleThreshold;

        if (moving) lastDir = v.normalized;

        anim.SetBool("IsMoving", moving);
        anim.SetFloat("Dir", ToDirIndex(lastDir)); // 0=Down, 1=Left, 2=Up, 3=Right

        // espelha só o gráfico (não mexe no root/física)
        if (lastDir.x < -0.01f)      sr.flipX = true;   // esquerda
        else if (lastDir.x > 0.01f)  sr.flipX = false;  // direita
        // cima/baixo mantém o flip anterior
    }

    // 0=Down, 1=Left, 2=Up, 3=Right
    float ToDirIndex(Vector2 dir)
    {
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            return dir.x < 0 ? 1f : 3f;  // Left : Right
        else
            return dir.y > 0 ? 2f : 0f;  // Up : Down
    }
}
