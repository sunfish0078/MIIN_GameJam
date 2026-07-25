using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PhotoTransitionEffect : MonoBehaviour
{
    public static PhotoTransitionEffect Instance { get; private set; }
    
    public static bool IsPlaying { get; private set; }
    
    public static void SetInputLocked(bool locked)
    {
        IsPlaying = locked;
    }

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
    [SerializeField] private AudioClip transitionSound;

    [Header("Fade Panel")]
    [SerializeField] private Image blackPanel;
    [SerializeField] private float fadeDuration = 0.5f;

    private Vector2 baseSize; 

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

    public void PlayTransition(Texture2D snapshot, RoomType targetRoomType)
    {
        StartCoroutine(TransitionRoutine(snapshot, targetRoomType));
    }

    private IEnumerator TransitionRoutine(Texture2D snapshot, RoomType targetRoomType)
    {
        IsPlaying = true;
        
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

            float popElapsed = 0f;
            while (popElapsed < popDuration)
            {
                popElapsed += Time.deltaTime;
                float popT = popCurve.Evaluate(popElapsed / popDuration);
                frameRect.localScale = Vector3.one * (startScale * popT);
                yield return null;
            }
            frameRect.localScale = Vector3.one * startScale;
            
            yield return new WaitForSeconds(postPopDelay);
            
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySFX(transitionSound);

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

            if (CCTVController.Instance != null)
                CCTVController.Instance.ShowControlRoom();

            MapManager.Instance.SwitchToMap(targetRoomType);
            yield return null;

            frameRect.gameObject.SetActive(false);
            MapManager.Instance.SetCurrentSnapshot(snapshot);

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
