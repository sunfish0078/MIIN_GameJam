using UnityEngine;
using UnityEngine.InputSystem;

// 촬영 시 아이템(이상현상)이 레티클 안에 완전히 담기면(=레티클 중앙이 아이템 중앙에 근접하고,
// 아이템 전체가 레티클 범위 안에 들어오면) 인식된 것으로 판정한다.
public class PhotoItemDetector : MonoBehaviour
{
    [SerializeField] private Camera cctvCamera;
    [SerializeField] private Photo photo;
    [SerializeField] private FlashEffect flashEffect;
    [SerializeField] private string itemTag = "item";

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

        foreach (Collider2D col in colliders)
        {
            if (!col.CompareTag(itemTag))
                continue;

            if (!CaptureOverlapMath.IsFullyContained(worldPoint, captureRadius, col))
                continue;

            if (col.TryGetComponent(out IPhotographable photographable))
            {
                if (!photographable.IsMouseNearCenter(worldPoint))
                    continue;

                float width = photo.GetCurrentWidth();
                float height = photo.GetCurrentHeight();
                Rect captureScreenRect = new Rect(screenPoint.x - width * 0.5f, screenPoint.y - height * 0.5f, width, height);

                Texture2D snapshot = CaptureWorldOnly(captureScreenRect);
                photographable.OnPhotographed(snapshot);
            }
        }
    }

    // cctvCamera만 오프스크린으로 즉시 렌더링해서 크롭한다. UI 캔버스(플래시, 프롬프트 등)는
    // 카메라 렌더링이 아니라 별도 합성이라 여기 안 찍힘 -> 지연 없이 바로 찍어도 하얗게 안 날아감.
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
