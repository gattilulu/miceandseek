using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [Header("Nomes das cenas (edite no Inspector)")]
    [SerializeField] private string menuScene = "Menu";
    [SerializeField] private string gameplayScene = "Gameplay";
    [SerializeField] private string tutorialScene = "Tutorial";
    [SerializeField] private string cutsceneScene = "Cutscene";

    // ---- Métodos sem parâmetro (fáceis de ligar nos botões)
    public void LoadMenu()     => SceneManager.LoadScene(menuScene);
    public void LoadGameplay() => SceneManager.LoadScene(gameplayScene);
    public void LoadTutorial() => SceneManager.LoadScene(tutorialScene);
    public void LoadCutscene() => SceneManager.LoadScene(cutsceneScene);
    public void Reload()       => SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    public void Quit()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }

    // ---- Versão assíncrona opcional (se quiser tela de loading)
    public void LoadSceneAsync(string sceneName) => StartCoroutine(LoadAsync(sceneName));
    private System.Collections.IEnumerator LoadAsync(string sceneName)
    {
        var op = SceneManager.LoadSceneAsync(sceneName);
        while (!op.isDone) yield return null;
    }
}
