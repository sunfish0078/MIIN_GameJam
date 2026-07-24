using UnityEngine;

// 촬영 판정(레티클 범위 vs 대상 콜라이더)에 공통으로 쓰는 계산들.
// PhotoDamage(몬스터 데미지), PhotoItemDetector(아이템 인식) 양쪽에서 재사용.
public static class CaptureOverlapMath
{
    public static Vector3 ScreenPointToWorldPoint(Camera cam, Vector2 screenPoint)
    {
        float depth = -cam.transform.position.z;
        Vector3 screenPos = new Vector3(screenPoint.x, screenPoint.y, depth);
        return cam.ScreenToWorldPoint(screenPos);
    }

    public static float ScreenRadiusToWorldRadius(Camera cam, float screenPixelRadius)
    {
        float worldUnitsPerPixel = (cam.orthographicSize * 2f) / Screen.height;
        return screenPixelRadius * worldUnitsPerPixel;
    }

    // 캡처 원(레티클) 넓이 대비 대상 콜라이더가 차지하는 비율(0~1).
    public static float GetOverlapRatio(Vector3 captureCenter, float captureRadius, Collider2D targetCollider)
    {
        Bounds bounds = targetCollider.bounds;
        float targetRadius = (bounds.extents.x + bounds.extents.y) * 0.5f;
        float distance = Vector2.Distance(captureCenter, bounds.center);

        float overlapArea = CircleIntersectionArea(captureRadius, targetRadius, distance);
        float captureArea = Mathf.PI * captureRadius * captureRadius;

        return captureArea > 0f ? Mathf.Clamp01(overlapArea / captureArea) : 0f;
    }

    // 대상(아이템)이 캡처 원 안에 완전히 담겼는지(=대상 원 전체가 캡처 원 내부에 있는지).
    public static bool IsFullyContained(Vector3 captureCenter, float captureRadius, Collider2D targetCollider)
    {
        Bounds bounds = targetCollider.bounds;
        float targetRadius = (bounds.extents.x + bounds.extents.y) * 0.5f;
        float distance = Vector2.Distance(captureCenter, bounds.center);

        return distance + targetRadius <= captureRadius;
    }

    // 반지름 r1, r2인 두 원이 중심거리 d만큼 떨어져 있을 때 겹치는 넓이 (표준 원-원 교차 넓이 공식)
    public static float CircleIntersectionArea(float r1, float r2, float d)
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
}
