using UnityEngine;
using DG.Tweening;

public class FileSlideAnimation : MonoBehaviour
{
   private RectTransform file;

    [Header("File Slide Down Settings")] 
    [SerializeField] private float dropDistance = 300f; // 시작 위치 (마스크 안, 카메라 쪽에서 얼마나 위에서 시작할지)
    [SerializeField] private float slideDuration = 1.1f; // 뽑혀나오는 데 걸리는 시간 (느릴수록 "쭉 뽑히는" 느낌)
    [SerializeField] private Ease slideEase = Ease.InOutSine; // 등속에 가깝게, 갑자기 튀지 않는 이징을 하고 싶어서 가져온 코드!
    private Vector2 fileTargetPos;

    [Header("Stretch Effect for Film")] [SerializeField]
    private float stretchAmountY = 1.08f; // 나오는 도중 세로로 살짝 늘어나는 정도 (1이면 효과 없음)

    [SerializeField] private float settleDuration = 0.25f; // 다 나온 후 원래 비율로 탁 정착하는 시간

    [Header("Settings for both Slide Effects")] [SerializeField] private float startDelay = 0.3f; // 게임 시작 후 몇 초 뒤에 애니메이션 시작할지
    
    [Header("file Slide-In Settings")]
    [SerializeField] private RectTransform photo; // 먼저 왼쪽에서 오른쪽으로 슬라이드될 사진
    [SerializeField] private RectTransform photo2;
    [SerializeField] private RectTransform photo3;
    [SerializeField] private float photoSlideDistance = 400f; // 사진이 왼쪽에서 얼마나 떨어진 곳에서 시작할지
    [SerializeField] private float photoSlideDuration = 0.8f; //slide하는데 걸리는 시간
    [SerializeField] private Ease photoSlideEase = Ease.OutCubic; //감속하며 멈추는 
    
    private Vector2 photoTargetPos, photo2TargetPos, photo3TargetPos;

    private void Start()
    {
       
    }

    private void OnEnable()
    {
        if (file == null)
            file = GetComponent<RectTransform>();
        
        fileTargetPos = file.anchoredPosition;
        file.anchoredPosition = fileTargetPos + new Vector2(0f, dropDistance);
        
        // photo, photo2, photo3도 똑같이 미리 숨겨야 함
        if (photo != null)
        {
            photoTargetPos = photo.anchoredPosition;
            photo.anchoredPosition = photoTargetPos + new Vector2(-photoSlideDistance, 0f);
        }
        if (photo2 != null)
        {
            photo2TargetPos = photo2.anchoredPosition;
            photo2.anchoredPosition = photo2TargetPos + new Vector2(-photoSlideDistance, 0f);
        }
        if (photo3 != null)
        {
            photo3TargetPos = photo3.anchoredPosition;
            photo3.anchoredPosition = photo3TargetPos + new Vector2(-photoSlideDistance, 0f);
        }
        
        PlayIntroAnimation();
    }

    private void PlayPhotoSlideIn()
    {
        if (photo == null) //error안전장치 코드
        {
            PlayIntroAnimation();
            return;
        }

        photo.DOAnchorPos(photoTargetPos, photoSlideDuration)
            .SetEase(photoSlideEase) //어떤 느낌으로 움직일지 = Ease.OutCubic사용해서 감속하며 멈춤
            .SetDelay(startDelay) //몇 초 기다렸다 시작할지
            .OnComplete(PlayPhotoSlideIn2); //끝난후 필름 내려온다
    }

    private void PlayPhotoSlideIn2()
    {

        photo2.DOAnchorPos(photoTargetPos, photoSlideDuration)
            .SetEase(photoSlideEase) //어떤 느낌으로 움직일지 = Ease.OutCubic사용해서 감속하며 멈춤
            .SetDelay(startDelay) //몇 초 기다렸다 시작할지
            .OnComplete(PlayPhotoSlideIn3); //끝난후 필름 내려온다
    } 
    private void PlayPhotoSlideIn3()
    {
        photo3.DOAnchorPos(photoTargetPos, photoSlideDuration)
            .SetEase(photoSlideEase) //어떤 느낌으로 움직일지 = Ease.OutCubic사용해서 감속하며 멈춤
            .SetDelay(startDelay); //몇 초 기다렸다 시작할지
    }
    
    private void PlayIntroAnimation()
    {
        file.localScale = Vector3.one;

        Sequence sequence = DOTween.Sequence();
        // 뽑혀나오는 동안 세로로 살짝 늘어남 (마스크에 눌려서 밀려나오는 물리감)
        sequence.Append(file.DOScaleY(stretchAmountY, slideDuration * 0.3f).SetEase(Ease.OutQuad));
        // 위치 이동 
        sequence.Join(file.DOAnchorPos(fileTargetPos, slideDuration).SetEase(slideEase));
        // 다 내려오면 늘어났던 비율을 원래대로 바꾼다
        sequence.Append(file.DOScaleY(1f, settleDuration).SetEase(Ease.OutBack)).OnComplete(PlayPhotoSlideIn);
    }
}
