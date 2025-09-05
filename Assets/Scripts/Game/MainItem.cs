using UnityEngine;

/// <summary>
/// Item principal: ao pegar, incrementa o contador do player,
/// ativa a zona de saída (opcional) e se destrói.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class MainItem : MonoBehaviour
{
    [Tooltip("Ative a saída quando o item for pego.")]
    [SerializeField] GameObject exitZoneToActivate;

    [Tooltip("Som/efeito opcional ao pegar.")]
    [SerializeField] AudioSource pickupSfx;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var pc = other.GetComponent<PlayerController2D>();
        if (pc != null)
        {
            pc.AddCarry(1);
        }

        if (exitZoneToActivate != null)
            exitZoneToActivate.SetActive(true); // libera a saída (na 1ª coleta, por ex.)

        if (pickupSfx) pickupSfx.Play();

        // destrói o item; se quiser, pode adicionar um pequeno atraso p/ tocar sfx por completo
        Destroy(gameObject);
    }
}
