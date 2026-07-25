using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    [Header("씬 전환 페이드 (전체화면 검은 Image, 캔버스 맨 위)")]
    [SerializeField] private Image sceneFadeImage;
    [SerializeField] private float sceneFadeDuration = 0.5f;

    // 설정을 열기 직전에 메인 메뉴가 실제로 켜져 있었을 때만 닫을 때 다시 켜줌
    // (ARoom에서는 mainMenuPanel이 이미 꺼져있으므로 안 켜져야 함)
    private bool wasMainMenuActiveBeforeSettings;

    // 페이드 중 버튼 연타로 씬 전환이 중복 실행되는 것 방지
    private bool isTransitioning;

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
        {
            DontDestroyOnLoad(mainCanvasRoot);

            // ARoom의 CCTV 캔버스 등 다른 캔버스보다 항상 위에 그려지게 강제
            Canvas canvas = mainCanvasRoot.GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.overrideSorting = true;
                canvas.sortingOrder = 1000;
            }
        }
    }

    void Start()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (gameClearPanel != null) gameClearPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);

        // 게임 처음 켰을 때도 검은 화면에서 서서히 밝아지게
        if (sceneFadeImage != null)
        {
            sceneFadeImage.gameObject.SetActive(true);
            SetFadeAlpha(1f);
            StartCoroutine(FadeInThenHide());
        }
    }

    private IEnumerator FadeInThenHide()
    {
        yield return Fade(1f, 0f, sceneFadeDuration);

        // 알파 0이어도 오브젝트가 켜져있으면 레이캐스트를 계속 가로채서 다른 버튼이 안 눌리게 됨 → 꺼둠
        sceneFadeImage.gameObject.SetActive(false);
    }

    void Update()
    {
        bool settingsOpen = settingsPanel != null && settingsPanel.activeSelf;

        if (settingsOpen)
        {
            // 설정 켜진 동안엔 ESC로 닫는 것만 처리. 우리가 방금 걸어둔 입력 잠금(IsPlaying) 때문에
            // 아래쪽 조기 리턴에 걸려서 못 닫히는 일이 없게 여기서 먼저 처리하고 끝냄.
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                ExitSettingsOnClick();

            return;
        }

        if (PhotoTransitionEffect.IsPlaying)
            return;

        // 사망/클리어 화면이 떠있는 동안엔 설정을 못 열게 함
        if ((gameOverPanel != null && gameOverPanel.activeSelf) || (gameClearPanel != null && gameClearPanel.activeSelf))
            return;

        // ARoom에는 설정을 여는 버튼이 없으니, ESC가 열기를 담당
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            SettingsOnClick();
    }

    public void ExitSettingsOnClick()
    {
        if (wasMainMenuActiveBeforeSettings && mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        if (settingsPanel != null) settingsPanel.SetActive(false);

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayUIClick();

        // 설정 여는 동안 걸어뒀던 일시정지/입력잠금 해제
        Time.timeScale = 1f;
        PhotoTransitionEffect.SetInputLocked(false);
    }
    public void StartOnClick()
    {
        if (isTransitioning) return;

        // 캔버스가 씬을 넘어 계속 남아있으므로, ARoom으로 가기 전에 메뉴 화면을 꺼둬야 게임 화면 위에 안 남음
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        StartCoroutine(LoadSceneWithFade("ARoom"));
    }

    private IEnumerator LoadSceneWithFade(string sceneName)
    {
        isTransitioning = true;

        try
        {
            if (sceneFadeImage != null)
            {
                sceneFadeImage.gameObject.SetActive(true);
                yield return Fade(0f, 1f, sceneFadeDuration);
            }

            SceneManager.LoadScene(sceneName);

            yield return null;

            if (sceneFadeImage != null)
                yield return Fade(1f, 0f, sceneFadeDuration);
        }
        finally
        {
            if (sceneFadeImage != null)
                sceneFadeImage.gameObject.SetActive(false);

            isTransitioning = false;
        }
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            SetFadeAlpha(Mathf.Lerp(from, to, elapsed / duration));
            yield return null;
        }

        SetFadeAlpha(to);
    }

    private void SetFadeAlpha(float alpha)
    {
        Color c = sceneFadeImage.color;
        c.a = alpha;
        sceneFadeImage.color = c;
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

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayUIClick();

        // 메인 메뉴(게임 시작 전)에서는 멈출 게 없으니 스킵. 플레이 중(ARoom)일 때만 일시정지.
        if (!wasMainMenuActiveBeforeSettings)
        {
            Time.timeScale = 0f;
            PhotoTransitionEffect.SetInputLocked(true);
        }
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
}
