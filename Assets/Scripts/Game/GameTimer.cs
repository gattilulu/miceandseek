using UnityEngine;
using TMPro;

/// <summary>
/// Timer global decrescente. Atualiza um TMP_Text, chama GameOver ao chegar a zero
/// e permite adicionar tempo (pickups).
/// </summary>
public class GameTimer : MonoBehaviour
{
    public static GameTimer Instance { get; private set; }

    [Header("Config")]
    [SerializeField] float startSeconds = 90f;   // tempo inicial
    [SerializeField] TMP_Text timerText;        // arraste o TimerText aqui
    [SerializeField] float lowTimeThreshold = 10f; // fica vermelho abaixo disso

    float timeLeft;
    bool firedTimeOver;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnEnable()
    {
        timeLeft = Mathf.Max(0f, startSeconds);
        firedTimeOver = false;
        UpdateText();
    }

    void Update()
    {
        // só conta enquanto o jogo está rolando
        if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing) return;

        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            if (!firedTimeOver)
            {
                firedTimeOver = true;
                GameManager.Instance?.GameOver();
            }
        }
        UpdateText();
    }

    public void AddTime(float seconds)
    {
        timeLeft += Mathf.Max(0f, seconds);
        UpdateText();
    }

    void UpdateText()
    {
        if (!timerText) return;

        int secs = Mathf.CeilToInt(timeLeft);
        int m = secs / 60;
        int s = secs % 60;
        timerText.text = $"{m:00}:{s:00}";

        // feedback visual quando o tempo está acabando
        if (timeLeft <= lowTimeThreshold)
           timerText.color = new Color32(255, 0, 0, 255); // cor que vai ficar qnd o tempo tá acabando. agr ta vermelho

        else
            timerText.color = Color.blue; //cor quando o tempo nao ta acabando
    }
}
