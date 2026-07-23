using UnityEngine;
using UnityEngine.InputSystem;

// 스크롤로 촬영 범위(레티클) 크기를 조절하고, 마우스를 따라다니게 한다.
// 최대로 올리면 1920x1080(16:9) 풀스크린 크기가 됨. GetSizeRatio()는 명중 시 데미지 배율 계산용으로 열어둠.
public class Photo : MonoBehaviour
{
    private const float AspectRatio = 1920f / 1080f;

    [SerializeField] private RectTransform rectTrs;
    [SerializeField] private float minWidth;
    [SerializeField] private float maxWidth;
    [SerializeField] private float scrollSensitivity;

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
        if (Mouse.current == null)
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
}
