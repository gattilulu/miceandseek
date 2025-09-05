using UnityEngine;
using UnityEngine.SceneManagement;

public class PressSpaceToContinue : MonoBehaviour
{
    [SerializeField] private string targetScene = "Gameplay";

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.LoadScene(targetScene);
        }
    }
}
