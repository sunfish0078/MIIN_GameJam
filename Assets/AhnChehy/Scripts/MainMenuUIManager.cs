using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MainMenuUIManager : MonoBehaviour
{
    public static MainMenuUIManager Instance { get; private set; }

    [Header("씬 넘어가도 유지 (Menu.unity의 MAINCanva)")]
    [SerializeField] private GameObject mainCanvasRoot;

    [Header("Buttons (위에서부터 Start / Settings / Exit 순서)")]
    [SerializeField] private RectTransform startButton;
    [SerializeField] private RectTransform settingsButton;
    [SerializeField] private RectTransform exitButton;

    [Header("Panels")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject gameClearPanel;
    [SerializeField] private GameObject tutorialPanel;

    // 설정을 열기 직전에 메인 메뉴가 실제로 켜져 있었을 때만 닫을 때 다시 켜줌
    // (ARoom에서는 mainMenuPanel이 이미 꺼져있으므로 안 켜져야 함)
    private bool wasMainMenuActiveBeforeSettings;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // 스크립트 오브젝트 자신과, 실제 UI가 들어있는 캔버스(별개 루트)를 둘 다 영속시켜야 함
        DontDestroyOnLoad(gameObject);

        if (mainCanvasRoot != null)
            DontDestroyOnLoad(mainCanvasRoot);
    }

    void Start()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (gameClearPanel != null) gameClearPanel.SetActive(false);
        if (tutorialPanel != null) tutorialPanel.SetActive(false);
        
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }

    void Update()
    {
        if (PhotoTransitionEffect.IsPlaying)
            return;

        // 사망/클리어 화면이 떠있는 동안엔 설정을 못 열게 함
        if ((gameOverPanel != null && gameOverPanel.activeSelf) || (gameClearPanel != null && gameClearPanel.activeSelf))
            return;

        // ARoom에는 설정을 여는 버튼이 없으니, ESC가 열기/닫기를 둘 다 담당
        if (Keyboard.current.escapeKey.wasPressedThisFrame && settingsPanel != null)
        {
            if (settingsPanel.activeSelf)
                ExitSettingsOnClick();
            else
                SettingsOnClick();
        }
    }

    public void ExitSettingsOnClick()
    {
        if (wasMainMenuActiveBeforeSettings && mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        if (settingsPanel != null) settingsPanel.SetActive(false);
    }
    public void StartOnClick()
    {
        // 캔버스가 씬을 넘어 계속 남아있으므로, ARoom으로 가기 전에 메뉴 화면을 꺼둬야 게임 화면 위에 안 남음
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        SceneManager.LoadScene("ARoom");
    }

    public void ExitGameOnClick()
    {
        Application.Quit();
    }

    public void SettingsOnClick()
    {
        if (settingsPanel == null) return;

        wasMainMenuActiveBeforeSettings = mainMenuPanel != null && mainMenuPanel.activeSelf;

        if (wasMainMenuActiveBeforeSettings)
            mainMenuPanel.SetActive(false);

        settingsPanel.SetActive(true);
    }

    // GameOverManager(ARoom)가 사망 시점에 호출
    public void ShowGameOverPanel()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    // 클리어 조건이 만들어지면 그쪽에서 호출
    public void ShowGameClearPanel()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (gameClearPanel != null) gameClearPanel.SetActive(true);
    }
    
    public void TutorialOnClick()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (tutorialPanel != null) tutorialPanel.SetActive(true);
    }
}
