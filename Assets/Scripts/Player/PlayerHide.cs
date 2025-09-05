using UnityEngine;

/// <summary>
/// Permite ao player se esconder ao apertar E, mas só quando estiver dentro de um HidingSpot.
/// Dá feedback visual (alpha) e expõe a propriedade IsHidden para outros sistemas.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class PlayerHide : MonoBehaviour
{
    [Header("Config")]
    [Tooltip("Tecla para esconder/mostrar.")]
    [SerializeField] KeyCode hideKey = KeyCode.E;
    [Tooltip("Alpha do Sprite quando escondido (feedback).")]
    [Range(0.05f, 1f)]
    [SerializeField] float hiddenAlpha = 0.35f;
    [Tooltip("Alpha do Sprite quando visível (normal).")]
    [Range(0.05f, 1f)]
    [SerializeField] float visibleAlpha = 1f;
    [Tooltip("Tag usada pelos objetos de esconderijo.")]
    [SerializeField] string hidingSpotTag = "HidingSpot";

    // Estado público para outros scripts lerem (EnemyVision, PlayerController)
    public bool IsHidden { get; private set; }

    // Internos
    SpriteRenderer[] renderers; // caso o player tenha vários sprites (sombra, acessórios, etc.)
    bool canHideHere = false;   // true enquanto dentro de um HidingSpot

    void Awake()
    {
        // Pega todos os SpriteRenderers do player e filhos para aplicar alpha
        renderers = GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
    }

    void Update()
    {
        // Toggle do escondido só é permitido se estiver numa área de esconderijo
        if (Input.GetKeyDown(hideKey) && canHideHere)
        {
            SetHidden(!IsHidden);
        }
    }

    void SetHidden(bool hidden)
    {
        IsHidden = hidden;

        // Feedback visual simples: ajusta alpha de todos os sprites do player
        float targetAlpha = hidden ? hiddenAlpha : visibleAlpha;
        for (int i = 0; i < renderers.Length; i++)
        {
            var c = renderers[i].color;
            c.a = targetAlpha;
            renderers[i].color = c;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(hidingSpotTag))
            canHideHere = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(hidingSpotTag))
        {
            canHideHere = false;
            // Ao sair do esconderijo, garantir que volta ao estado visível
            if (IsHidden) SetHidden(false);
        }
    }
}
