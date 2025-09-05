using UnityEngine;
using UnityEngine.UI;

public class OpenPanel : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject creditsPanel;

    [Header("Tutorial Images")]
    public Image[] tutorialImages; 
    private int currentIndex = 0;

    private void Start()
    {
        ShowImage(currentIndex); 
    }

    public void ClosePanel()
    {
        creditsPanel.SetActive(false);
    }

    public void NextImage()
    {
        currentIndex++;
        if (currentIndex >= tutorialImages.Length) 
            currentIndex = 0; 
        ShowImage(currentIndex);
    }

    public void PreviousImage()
    {
        currentIndex--;
        if (currentIndex < 0) 
            currentIndex = tutorialImages.Length - 1; 
        ShowImage(currentIndex);
    }

    private void ShowImage(int index)
    {
        for (int i = 0; i < tutorialImages.Length; i++)
        {
            tutorialImages[i].gameObject.SetActive(i == index);
        }
    }
}
