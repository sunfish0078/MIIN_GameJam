using ChoiJeongYun.Scripts.Enemy;
using UnityEngine;
using UnityEngine.InputSystem;

public class PhotoDamage : MonoBehaviour
{
    [SerializeField] private Camera cctvCamera;
    [SerializeField] private Photo photo;
    [SerializeField] private FlashEffect flashEffect;
    [SerializeField] private int minDamage = 1;
    [SerializeField] private int maxDamage = 15;
    [SerializeField] private string enemyTag = "enemy";

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
            if (!col.CompareTag(enemyTag))
                continue;

            if (col.TryGetComponent(out IDamageable damageable))
            {
                float overlapRatio = CaptureOverlapMath.GetOverlapRatio(worldPoint, captureRadius, col);
                int damage = Mathf.RoundToInt(Mathf.Lerp(minDamage, maxDamage, overlapRatio));
                damageable.TakeDamage(damage);
            }
        }
    }
}
