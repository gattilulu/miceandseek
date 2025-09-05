using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] float idleThreshold = 0.05f;

    Rigidbody2D rb;
    Animator anim;
    Vector2 lastDir = Vector2.down;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        Vector2 v = rb.linearVelocity;
        bool moving = v.sqrMagnitude > idleThreshold * idleThreshold;

        if (moving) lastDir = v.normalized;

        anim.SetBool("IsMoving", moving);
        anim.SetFloat("Dir", ToDirIndex(lastDir)); // 0/1/2/3
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
