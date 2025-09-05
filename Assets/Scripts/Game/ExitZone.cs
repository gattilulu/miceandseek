using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ExitZone : MonoBehaviour
{
    [Header("Collider que bloqueia a passagem até a vitória")]
    [SerializeField] Collider2D gateBlockingCollider; // coloque aqui um BoxCollider2D NÃO-trigger

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    void Start()
    {
        UpdateGate();
    }

    void Update()
    {
        // simples e seguro pra jam: checa a cada frame
        UpdateGate();
    }

    void UpdateGate()
    {
        if (gateBlockingCollider == null || GameManager.Instance == null) return;
        // bloqueia enquanto ainda não pode vencer
        gateBlockingCollider.enabled = !GameManager.Instance.IsVictoryReady();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var pc = other.GetComponent<PlayerController2D>();
        if (pc == null) return;

        if (pc.CarriedMainItems > 0)
        {
            GameManager.Instance?.DepositItems(pc.CarriedMainItems);
            pc.ResetCarry(); // velocidade volta ao normal
            // sem victory ainda? o gate continua fechado e o player NÃO atravessa.
        }
    }
}
