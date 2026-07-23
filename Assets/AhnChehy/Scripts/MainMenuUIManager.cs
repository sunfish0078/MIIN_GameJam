using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MainMenuUIManager : MonoBehaviour
{
    [Header("Buttons (위에서부터 Start / Settings / Exit 순서)")]
    [SerializeField] private RectTransform startButton;
    [SerializeField] private RectTransform settingsButton;
    [SerializeField] private RectTransform exitButton;

    [Header("Panels")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject mainMenuPanel;

    void Start()
    {
        if (mainMenuPanel == null) return;
        if (settingsPanel == null) return;
        
        settingsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void ExitSettingsOnClick()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }
    public void StartOnClick()
    {
        //SceneManager.LoadScene("게임 씬 이름");
    }

    public void ExitGameOnClick()
    {
        Application.Quit();
    }

    public void SettingsOnClick()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }
}
