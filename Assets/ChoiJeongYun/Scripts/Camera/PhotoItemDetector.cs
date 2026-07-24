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

        foreach (Collider2D col in colliders)
        {
            if (!col.CompareTag(itemTag) && !col.CompareTag(anomalyTag))
                continue;

            if (!col.OverlapPoint(worldPoint))
                continue;

            if (!CaptureOverlapMath.IsFullyContained(worldPoint, captureRadius, col))
                continue;

            IPhotographable photographable = col.GetComponentInParent<IPhotographable>();
            if (photographable == null)
                continue;

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
