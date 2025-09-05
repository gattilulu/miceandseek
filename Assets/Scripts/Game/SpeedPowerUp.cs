using UnityEngine;

public class SpeedPowerUp : MonoBehaviour
{
    public float speedBoostAmount = 3f;   // quanto soma à velocidade (ex.: +3)
    public float durationSeconds  = 5f;   // duração do efeito

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var pc = other.GetComponent<PlayerController2D>();
        if (pc != null)
        {
            pc.ActivateSpeedBoost(speedBoostAmount, durationSeconds);
        }
        Destroy(gameObject);
    }
}
