using DG.Tweening;
using UnityEngine;

public class FilmDropAnimation : MonoBehaviour
{
    private RectTransform film;

    [Header("Slide Settings")] [SerializeField]
    private float dropDistance = 300f; // 시작 위치 (마스크 안, 카메라 쪽에서 얼마나 위에서 시작할지)

    [SerializeField] private float slideDuration = 1.1f; // 뽑혀나오는 데 걸리는 시간 (느릴수록 "쭉 뽑히는" 느낌)
    [SerializeField] private Ease slideEase = Ease.InOutSine; // 등속에 가깝게, 갑자기 튀지 않는 이징을 하고 싶어서 가져온 코드!

    [Header("스트레치 효과 (뽑혀나오는 물리감을 추가하고 싶었음)")] [SerializeField]
    private float stretchAmountY = 1.08f; // 나오는 도중 세로로 살짝 늘어나는 정도 (1이면 효과 없음)

    [SerializeField] private float settleDuration = 0.25f; // 다 나온 후 원래 비율로 탁 정착하는 시간

    [Header("공통")] [SerializeField] private float startDelay = 0.3f; // 게임 시작 후 몇 초 뒤에 애니메이션 시작할지

    private void Awake()
    {
        if (film == null)
            film = GetComponent<RectTransform>();
    }

    private void Start()
    {
        PlayIntroAnimation();
    }

    private void PlayIntroAnimation()
    {
        Vector2 targetPos = film.anchoredPosition; // 원래 배치된 최종 위치
        Vector2 startPos = targetPos + new Vector2(0f, dropDistance); // 마스크 안, 카메라 쪽에서 시작

        // 초기 상태: 마스크 안 숨겨진 위치, 정상 크기 (팝 없이 바로 실제 크기)
        film.anchoredPosition = startPos;
        film.localScale = Vector3.one;

        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(startDelay);

        // 뽑혀나오는 동안 세로로 살짝 늘어남 (마스크에 눌려서 밀려나오는 물리감)
        sequence.Append(film.DOScaleY(stretchAmountY, slideDuration * 0.3f).SetEase(Ease.OutQuad));

        // 위치 이동 
        sequence.Join(film.DOAnchorPos(targetPos, slideDuration).SetEase(slideEase));

        // 다 내려오면 늘어났던 비율을 원래대로 바꾼다
        sequence.Append(film.DOScaleY(1f, settleDuration).SetEase(Ease.OutBack));
    }
}
