using UnityEngine;

/// <summary>
/// Quando o Player encosta, adiciona segundos ao GameTimer e se destrói.
/// Deixe o collider como isTrigger.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class TimePickup : MonoBehaviour
{
    [SerializeField] float seconds = 10f;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (GameTimer.Instance != null)
            GameTimer.Instance.AddTime(seconds);

        Destroy(gameObject);
    }
}
