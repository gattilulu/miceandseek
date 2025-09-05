using System.Collections;
using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
    [Header("Movimento")]
    [SerializeField] float moveSpeed = 5f;

    [Tooltip("Multiplicador de velocidade por item carregado. Ex.: 0.6 = cada item reduz p/ 60%")]
    [SerializeField] float carryingMultiplier = 0.6f;

    Rigidbody2D rb;
    Vector2 input;

    // esconderijo (opcional)
    PlayerHide hide;

    // --- Itens principais carregados ---
    public int CarriedMainItems { get; private set; } = 0;

    // Para compat: mantém um getter antigo (true se tiver >=1)
    public bool HasMainItem => CarriedMainItems > 0;

    // ---- Power-up de velocidade (já existia) ----
    float currentSpeedBoost = 0f;        
    Coroutine speedBoostRoutine = null;  
    // --------------------------------------------

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        hide = GetComponent<PlayerHide>(); // opcional
    }

    void Update()
    {
        input = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;
    }

    void FixedUpdate()
    {
        if (hide != null && hide.IsHidden)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // velocidade base + boost
        float baseSpeed = moveSpeed + currentSpeedBoost;

        // aplica penalidade acumulativa por item: multiplier^N
        float penalty = Mathf.Pow(carryingMultiplier, CarriedMainItems);
        float finalSpeed = baseSpeed * penalty;

        rb.linearVelocity = input * finalSpeed;
    }

    // ---------- API p/ itens principais ----------
    public void AddCarry(int amount = 1)
    {
        CarriedMainItems = Mathf.Max(0, CarriedMainItems + amount);
    }

    public void ResetCarry()
    {
        CarriedMainItems = 0;
    }
    // --------------------------------------------

    // -------- API: Power-up de velocidade --------
    public void ActivateSpeedBoost(float boostAmount, float duration)
    {
        if (speedBoostRoutine != null)
            StopCoroutine(speedBoostRoutine);

        speedBoostRoutine = StartCoroutine(SpeedBoostRoutine(boostAmount, duration));
    }

    IEnumerator SpeedBoostRoutine(float boostAmount, float duration)
    {
        currentSpeedBoost = boostAmount;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        currentSpeedBoost = 0f;
        speedBoostRoutine = null;
    }
    // --------------------------------------------
}
