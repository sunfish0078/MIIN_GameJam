using UnityEngine;
using UnityEngine.InputSystem;

public class Photo : MonoBehaviour
{
    private const float AspectRatio = 1920f / 1080f;

    [SerializeField] private RectTransform rectTrs;
    [SerializeField] private float minWidth = 200;
    [SerializeField] private float maxWidth = 1920;
    [SerializeField] private float scrollSensitivity = 30;

    private float currentWidth;
    private Canvas canvas;
    private RectTransform parentRect;

    private void Awake()
    {
        if (rectTrs == null)
            rectTrs = GetComponent<RectTransform>();

        canvas = rectTrs.GetComponentInParent<Canvas>();
        parentRect = rectTrs.parent as RectTransform;

        currentWidth = Mathf.Clamp(rectTrs.sizeDelta.x, minWidth, maxWidth);
        ApplySize();
    }

    private void Update()
    {
        if (Mouse.current == null || PhotoTransitionEffect.IsPlaying)
            return;

        float scroll = Mouse.current.scroll.ReadValue().y;
        if (!Mathf.Approximately(scroll, 0f))
        {
            currentWidth = Mathf.Clamp(currentWidth + scroll * scrollSensitivity, minWidth, maxWidth);
            ApplySize();
        }

        FollowMouse();
    }

    private void ApplySize()
    {
        rectTrs.sizeDelta = new Vector2(currentWidth, currentWidth / AspectRatio);
    }

    private void FollowMouse()
    {
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, mouseScreenPos, cam, out Vector2 localPoint))
        {
            rectTrs.anchoredPosition = localPoint;
        }
    }
    
    public float GetSizeRatio()
    {
        return Mathf.InverseLerp(minWidth, maxWidth, currentWidth);
    }

    public float GetCurrentWidth()
    {
        return currentWidth;
    }

    public float GetCurrentHeight()
    {
        return currentWidth / AspectRatio;
    }
}
