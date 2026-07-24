using UnityEngine;

public interface IPhotographable
{
    bool IsMouseNearCenter(Vector3 worldPoint);

    // snapshot: 촬영 순간 레티클 범위만 잘라서 찍은 스크린샷 (UI 제외, 게임 화면만)
    void OnPhotographed(Texture2D snapshot);
}
