using UnityEngine;
using UnityEngine.InputSystem;

public class PhotoItemDetector : MonoBehaviour
{
    [SerializeField] private Camera cctvCamera;
    [SerializeField] private Photo photo;
    [SerializeField] private FlashEffect flashEffect;
    [SerializeField] private string itemTag = "item";
    [SerializeField] private string anomalyTag = "Phenomenon";

    private void OnEnable()
    {
        flashEffect.OnCaptured += PerformCapture;
    }

    private void OnDisable()
    {
        flashEffect.OnCaptured -= PerformCapture;
    }

    private void PerformCapture()
    {
        Vector2 screenPoint = Mouse.current.position.ReadValue();
        Vector3 worldPoint = CaptureOverlapMath.ScreenPointToWorldPoint(cctvCamera, screenPoint);
        float captureRadius = CaptureOverlapMath.ScreenRadiusToWorldRadius(cctvCamera, photo.GetCurrentWidth() * 0.5f);

        Collider2D[] colliders = Physics2D.OverlapCircleAll(worldPoint, captureRadius);
        Debug.Log($"[PhotoItemDetector] 촬영 범위 안 콜라이더 {colliders.Length}개 감지됨");

        foreach (Collider2D col in colliders)
        {
            if (!col.CompareTag(itemTag) && !col.CompareTag(anomalyTag))
                continue;

            if (!col.OverlapPoint(worldPoint))
            {
                Debug.Log($"[PhotoItemDetector] '{col.name}' 마우스가 콜라이더 안에 없음");
                continue;
            }

            if (!CaptureOverlapMath.IsFullyContained(worldPoint, captureRadius, col))
            {
                Debug.Log($"[PhotoItemDetector] '{col.name}' 촬영 범위에 완전히 안 담김");
                continue;
            }

            // 콜라이더가 있는 오브젝트 자신뿐 아니라 부모 쪽에 IPhotographable이 있어도 인정
            // (콜라이더는 자식 스프라이트/손 오브젝트에, 로직 스크립트는 루트에 있는 구조도 흔함)
            IPhotographable photographable = col.GetComponentInParent<IPhotographable>();
            if (photographable == null)
            {
                Debug.Log($"[PhotoItemDetector] '{col.name}'와 그 부모 어디에도 IPhotographable 컴포넌트가 없음");
                continue;
            }

            float width = photo.GetCurrentWidth();
            float height = photo.GetCurrentHeight();
            Rect captureScreenRect = new Rect(screenPoint.x - width * 0.5f, screenPoint.y - height * 0.5f, width, height);

            Texture2D snapshot = CaptureWorldOnly(captureScreenRect);
            photographable.OnPhotographed(snapshot);
        }
    }
    
    private Texture2D CaptureWorldOnly(Rect screenRect)
    {
        int rtWidth = Screen.width;
        int rtHeight = Screen.height;

        RenderTexture rt = RenderTexture.GetTemporary(rtWidth, rtHeight, 24);
        RenderTexture prevTargetTexture = cctvCamera.targetTexture;
        RenderTexture prevActive = RenderTexture.active;

        cctvCamera.targetTexture = rt;
        cctvCamera.Render();
        RenderTexture.active = rt;

        int x = Mathf.Clamp(Mathf.RoundToInt(screenRect.x), 0, rtWidth - 1);
        int y = Mathf.Clamp(Mathf.RoundToInt(screenRect.y), 0, rtHeight - 1);
        int w = Mathf.Clamp(Mathf.RoundToInt(screenRect.width), 1, rtWidth - x);
        int h = Mathf.Clamp(Mathf.RoundToInt(screenRect.height), 1, rtHeight - y);

        Texture2D snapshot = new Texture2D(w, h, TextureFormat.RGB24, false);
        snapshot.ReadPixels(new Rect(x, y, w, h), 0, 0);
        snapshot.Apply();

        cctvCamera.targetTexture = prevTargetTexture;
        RenderTexture.active = prevActive;
        RenderTexture.ReleaseTemporary(rt);

        return snapshot;
    }
}
