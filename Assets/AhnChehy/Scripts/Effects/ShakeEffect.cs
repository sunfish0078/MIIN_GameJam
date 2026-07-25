using DG.Tweening;
using UnityEngine;

public class ShakeEffect : MonoBehaviour
{
    [Header("흔들림 주기")]
    [SerializeField] private float minInterval = 3f; // 흔들림 사이 최소 대기 시간(초)
    [SerializeField] private float maxInterval = 6f; // 흔들림 사이 최대 대기 시간(초) - 랜덤이라 매번 똑같지 않게
 
    [Header("흔들림 강도")]
    [SerializeField] private float wobbleAngle = 4f;     // 좌우로 얼마나 기울어질지 (각도, 작을수록 은은함)
    [SerializeField] private int vibrato = 6;             // 흔들리는 동안 좌우 왕복 횟수
    [SerializeField] private float wobbleDuration = 0.6f; // 흔들림 한 번의 총 지속 시간
 
    private RectTransform rectTransform;
 
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }
 
    private void Start()
    {
        ScheduleNextWobble();
    }
 
    /// <summary>
    /// 다음 흔들림까지 랜덤한 시간만큼 기다렸다가, 흔들리고, 다시 스스로를 예약함(반복).
    /// </summary>
    private void ScheduleNextWobble()
    {
        float waitTime = Random.Range(minInterval, maxInterval);
 
        DOVirtual.DelayedCall(waitTime, () =>
        {
            PlayWobble();
            ScheduleNextWobble(); // 흔들고 나서 다음 흔들림을 다시 예약 (계속 반복)
        });
    }
 
    /// <summary>
    /// 좌우로 살짝 흔들리는 회전 애니메이션 한 번 재생.
    /// </summary>
    private void PlayWobble()
    {
        rectTransform.DOPunchRotation(new Vector3(0f, 0f, wobbleAngle), wobbleDuration, vibrato, 0.5f);
    }
}

