using ChoiJeongYun.Scripts.Enemy;
using UnityEngine;
using UnityEngine.InputSystem;

public class PhotoDamage : MonoBehaviour
{
    [SerializeField] private Camera cctvCamera;
    [SerializeField] private Photo photo;
    [SerializeField] private FlashEffect flashEffect;
    [SerializeField] private float minDamage = 1f;
    [SerializeField] private float maxDamage = 15f;
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
        Vector3 worldPoint = ScreenPointToWorldPoint(screenPoint);

        float captureRadius = ScreenRadiusToWorldRadius(photo.GetCurrentWidth() * 0.5f);

        Collider2D[] colliders = Physics2D.OverlapCircleAll(worldPoint, captureRadius);

        foreach (Collider2D col in colliders)
        {
            if (!col.CompareTag(enemyTag))
                continue;

            if (col.TryGetComponent(out IDamageable damageable))
            {
                float overlapRatio = GetOverlapRatio(worldPoint, captureRadius, col);
                float damage = Mathf.Lerp(minDamage, maxDamage, overlapRatio);
                damageable.TakeDamage(damage);
            }
        }
    }
    
    private float GetOverlapRatio(Vector3 captureCenter, float captureRadius, Collider2D enemyCollider)
    {
        Bounds bounds = enemyCollider.bounds;
        float enemyRadius = (bounds.extents.x + bounds.extents.y) * 0.5f;
        float distance = Vector2.Distance(captureCenter, bounds.center);

        float overlapArea = CircleIntersectionArea(captureRadius, enemyRadius, distance);
        float captureArea = Mathf.PI * captureRadius * captureRadius;

        return captureArea > 0f ? Mathf.Clamp01(overlapArea / captureArea) : 0f;
    }
    
    private float CircleIntersectionArea(float r1, float r2, float d)
    {
        if (d >= r1 + r2)
            return 0f;

        if (d <= Mathf.Abs(r1 - r2))
        {
            float rMin = Mathf.Min(r1, r2);
            return Mathf.PI * rMin * rMin;
        }

        float d1 = (d * d - r2 * r2 + r1 * r1) / (2f * d);
        float d2 = d - d1;

        float area1 = r1 * r1 * Mathf.Acos(Mathf.Clamp(d1 / r1, -1f, 1f)) - d1 * Mathf.Sqrt(Mathf.Max(r1 * r1 - d1 * d1, 0f));
        float area2 = r2 * r2 * Mathf.Acos(Mathf.Clamp(d2 / r2, -1f, 1f)) - d2 * Mathf.Sqrt(Mathf.Max(r2 * r2 - d2 * d2, 0f));

        return area1 + area2;
    }

    private Vector3 ScreenPointToWorldPoint(Vector2 screenPoint)
    {
        float depth = -cctvCamera.transform.position.z;
        Vector3 screenPos = new Vector3(screenPoint.x, screenPoint.y, depth);
        return cctvCamera.ScreenToWorldPoint(screenPos);
    }

    private float ScreenRadiusToWorldRadius(float screenPixelRadius)
    {
        float worldUnitsPerPixel = (cctvCamera.orthographicSize * 2f) / Screen.height;
        return screenPixelRadius * worldUnitsPerPixel;
    }
}
