using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PhotoTransitionEffect : MonoBehaviour
{
    public static PhotoTransitionEffect Instance { get; private set; }
    
    public static bool IsPlaying { get; private set; }

    [Header("Pop-in")]
    [SerializeField] private float popDuration = 0.35f;
    [SerializeField] private AnimationCurve popCurve = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(0.7f, 1.15f),
        new Keyframe(1f, 1f));
    [SerializeField] private float postPopDelay = 1f;

    [Header("Photo")]
    [SerializeField] private RectTransform frameRect; 
    [SerializeField] private RawImage polaroidImage;  
    [SerializeField] private float startSquareSize = 108f;
    [SerializeField] private float growDuration = 1f;
    [SerializeField] private Vector3 spinDegrees = new Vector3(360f, 480f, 300f);
    [SerializeField] private AnimationCurve growCurve = new AnimationCurve(
        new Keyframe(0f, 0f, 0f, 0f),
        new Keyframe(1f, 1f, 2f, 2f));

    [Header("Fade Panel")]
    [SerializeField] private Image blackPanel;
    [SerializeField] private float fadeDuration = 0.5f;

    private Vector2 baseSize; // frameRect의 원래(고정) 디자인 크기

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        baseSize = frameRect.sizeDelta;

        SetBlackAlpha(0f);
        frameRect.gameObject.SetActive(false);
    }

    public void PlayTransition(Texture2D snapshot, string targetSceneName)
    {
        StartCoroutine(TransitionRoutine(snapshot, targetSceneName));
    }

    private IEnumerator TransitionRoutine(Texture2D snapshot, string targetSceneName)
    {
        IsPlaying = true;

        // 중간에 뭔가 터져도 finally에서 무조건 풀어줌 (안 그러면 영구 입력 잠금)
        try
        {
            polaroidImage.texture = snapshot;
            ApplySquareCrop(snapshot);

            float baseLength = Mathf.Max(baseSize.x, baseSize.y);
            float startScale = startSquareSize / baseLength;
            float targetScale = (Mathf.Max(Screen.width, Screen.height) * 1.2f) / baseLength;

            frameRect.gameObject.SetActive(true);
            frameRect.anchoredPosition = Vector2.zero;
            frameRect.localEulerAngles = Vector3.zero;
            frameRect.localScale = Vector3.zero;

            // 1단계: 팝으로 튀어나오기 (살짝 커졌다 정상 크기로)
            float popElapsed = 0f;
            while (popElapsed < popDuration)
            {
                popElapsed += Time.deltaTime;
                float popT = popCurve.Evaluate(popElapsed / popDuration);
                frameRect.localScale = Vector3.one * (startScale * popT);
                yield return null;
            }
            frameRect.localScale = Vector3.one * startScale;

            // 잠깐 멈췄다가 회전 시작
            yield return new WaitForSeconds(postPopDelay);

            // 2단계: 빙글빙글 돌면서 화면 꽉 찰 때까지 커지기
            float elapsed = 0f;
            while (elapsed < growDuration)
            {
                elapsed += Time.deltaTime;
                float t = growCurve.Evaluate(elapsed / growDuration);
                frameRect.localScale = Vector3.one * Mathf.LerpUnclamped(startScale, targetScale, t);
                frameRect.localEulerAngles = Vector3.LerpUnclamped(spinDegrees, Vector3.zero, t);
                yield return null;
            }

            yield return FadeBlack(0f, 1f, fadeDuration);

            SceneManager.LoadScene(targetSceneName);
            yield return null;

            frameRect.gameObject.SetActive(false);
            Destroy(snapshot);

            yield return FadeBlack(1f, 0f, fadeDuration);
        }
        finally
        {
            IsPlaying = false;

            if (frameRect != null)
                frameRect.gameObject.SetActive(false);

            SetBlackAlpha(0f);
        }
    }

    // 옆(또는 위아래)을 잘라서 정사각형으로 보이게, 텍스처 자체는 안 건드리고 UV만 크롭.
    private void ApplySquareCrop(Texture2D snapshot)
    {
        float aspect = snapshot.width / (float)snapshot.height;

        if (aspect > 1f)
        {
            float uvWidth = 1f / aspect;
            polaroidImage.uvRect = new Rect((1f - uvWidth) * 0.5f, 0f, uvWidth, 1f);
        }
        else
        {
            float uvHeight = aspect;
            polaroidImage.uvRect = new Rect(0f, (1f - uvHeight) * 0.5f, 1f, uvHeight);
        }
    }

    private IEnumerator FadeBlack(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            SetBlackAlpha(Mathf.Lerp(from, to, elapsed / duration));
            yield return null;
        }

        SetBlackAlpha(to);
    }

    private void SetBlackAlpha(float alpha)
    {
        Color c = blackPanel.color;
        c.a = alpha;
        blackPanel.color = c;
    }
}
