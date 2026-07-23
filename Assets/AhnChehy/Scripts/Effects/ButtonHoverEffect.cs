using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float popScale = 1.2f;
    private Vector3 originalScale;
    private Quaternion originalRotation;
    void Awake()
    {
        originalScale = transform.localScale;
        originalRotation = transform.rotation;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOKill();
        transform.DOScale(originalScale * popScale, 0.1f).SetUpdate(true);
        transform.DORotateQuaternion(originalRotation, 0.1f).SetUpdate(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOKill();
        transform.DOScale(originalScale, 0.1f).SetUpdate(true);
    }
    void OnDisable()
    {
        transform.DOKill();
        transform.localScale = originalScale;
    }
}

