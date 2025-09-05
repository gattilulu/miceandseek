using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class TransitionFader : MonoBehaviour
{
    public static TransitionFader Instance { get; private set; }

    [Header("Refs")]
    [SerializeField] private CanvasGroup fadeCanvas; // no Image preto
    [SerializeField] private Canvas fadeCanvasRoot;  // o Canvas (opcional, só p/ setar sorting)

    [Header("Config")]
    [SerializeField, Min(0f)] private float duration = 0.4f;
    [SerializeField] private bool fadeInOnStart = true;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Garantias de inicio TOTALMENTE preto e por cima:
        if (fadeCanvas != null) fadeCanvas.alpha = 1f;
        if (fadeCanvasRoot != null) fadeCanvasRoot.sortingOrder = 9999;
    }

    void Start()
    {
        if (fadeInOnStart && fadeCanvas != null && fadeCanvas.alpha > 0.99f)
            StartCoroutine(Fade(1f, 0f));
    }

    public void FadeAndLoad(string sceneName)
    {
        if (!gameObject.activeInHierarchy) return;
        StartCoroutine(FadeRoutine(sceneName));
    }

    private IEnumerator FadeRoutine(string sceneName)
    {
        // 1) Fecha até preto total ANTES de começar a trocar
        yield return Fade(0f, 1f);

        // 2) Carrega assíncrono mas NÃO ativa a cena ainda
        var op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        // Unity carrega até ~0.9 e espera ativação
        while (op.progress < 0.9f) yield return null;

        // 3) Agora que já está preto, pode ativar a cena
        op.allowSceneActivation = true;

        // 4) Espera concluir a troca
        while (!op.isDone) yield return null;

        // 5) Abre do preto
        yield return Fade(1f, 0f);
    }

    private IEnumerator Fade(float from, float to)
    {
        if (fadeCanvas == null) yield break;

        float t = 0f;
        fadeCanvas.blocksRaycasts = true;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            fadeCanvas.alpha = Mathf.Lerp(from, to, t / Mathf.Max(0.0001f, duration));
            yield return null;
        }
        fadeCanvas.alpha = to;

        // só bloqueia clique quando visível
        fadeCanvas.blocksRaycasts = fadeCanvas.alpha > 0.99f;
    }
}
