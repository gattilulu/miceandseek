using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controla o estado global do jogo e gerencia a meta de entrega de itens.
/// </summary>
public enum GameState { Playing, GameOver, Victory }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameState State { get; private set; } = GameState.Playing;

    [Header("UI")]
    [SerializeField] GameObject gameOverUI;
    [SerializeField] GameObject victoryUI;

    [Header("Objetivos")]
    [Tooltip("Se marcado, conta automaticamente quantos MainItem existiam na cena no início.")]
    [SerializeField] bool autoCountRequired = true;

    [Tooltip("Se autoCountRequired = false, usa este valor como quantidade requerida.")]
    [SerializeField] int requiredItemsOverride = 1;

    [Tooltip("Quantidade total de itens requeridos (somente leitura em runtime).")]
    [SerializeField] int requiredItems = 0;

    [Tooltip("Quantos já foram entregues na saída (somente leitura em runtime).")]
    [SerializeField] int deliveredItems = 0;

    // Exposição somente-leitura pro HUD/inspector via propriedade
    public int RequiredItems => Mathf.Max(0, requiredItems);
    public int DeliveredItems => Mathf.Max(0, deliveredItems);

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Define a meta ao iniciar a cena
        if (autoCountRequired)
        {
            // Unity 6: FindObjectsByType (sem pegar inativos por padrão)
#if UNITY_6000_0_OR_NEWER
            requiredItems = Object.FindObjectsByType<MainItem>(
                FindObjectsInactive.Exclude, FindObjectsSortMode.None).Length;
#else
            requiredItems = FindObjectsOfType<MainItem>().Length;
#endif
        }
        else
        {
            requiredItems = Mathf.Max(0, requiredItemsOverride);
        }
    }

    public void GameOver()
    {
        if (State != GameState.Playing) return;
        State = GameState.GameOver;
        if (gameOverUI) gameOverUI.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Victory()
    {
        if (State != GameState.Playing) return;
        State = GameState.Victory;
        if (victoryUI) victoryUI.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // ---------- Depósito / Progresso ----------
    public void DepositItems(int amount)
    {
        if (State != GameState.Playing) return;
        if (amount <= 0) return;

        deliveredItems += amount;

        // Se não houver meta (ex.: cena de teste), 1+ entrega já vence
        int target = Mathf.Max(1, requiredItems);

        if (deliveredItems >= target)
        {
            Victory();
        }
    }

    /// <summary>Retorna true se já entregou tudo e a saída pode “abrir”.</summary>
    public bool IsVictoryReady()
    {
        int target = Mathf.Max(1, requiredItems);
        return deliveredItems >= target;
    }

    /// <summary>Útil para debugar/testar.</summary>
    public void ResetProgress(bool recountSceneItems = false)
    {
        deliveredItems = 0;
        if (recountSceneItems)
        {
#if UNITY_6000_0_OR_NEWER
            requiredItems = Object.FindObjectsByType<MainItem>(
                FindObjectsInactive.Exclude, FindObjectsSortMode.None).Length;
#else
            requiredItems = FindObjectsOfType<MainItem>().Length;
#endif
        }
        State = GameState.Playing;
        if (gameOverUI) gameOverUI.SetActive(false);
        if (victoryUI) gameOverUI.SetActive(false);
        Time.timeScale = 1f;
    }
}
