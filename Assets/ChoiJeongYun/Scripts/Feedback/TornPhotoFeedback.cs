using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ChoiJeongYun.Scripts.Feedback
{
    // A룸으로 복귀할 때: 찍었던 사진이 (원래 폴라로이드처럼) 화면 중앙에 팝으로 튀어나와 하나로 보이다가,
    // 잠깐 뒤 위/아래로 찢어지며 반대로 날아가는 연출.
    // 각 조각은 흰 프레임(부모) + 사진(자식) 구조라, 프레임만 움직이면 사진도 같이 따라감.
    public class TornPhotoFeedback : AbstractFeedback
    {
        [SerializeField] private RectTransform topFrame;
        [SerializeField] private RectTransform bottomFrame;

        // 이 스크립트가 붙은 오브젝트(topFrame/bottomFrame의 부모) 자체를 팝인 스케일에 사용.
        // 단, 이 오브젝트 자체는 SetActive로 껐다 켰다 하지 않음(자기 코루틴이 멈춰버림) -
        // 보이기/숨기기는 topFrame/bottomFrame 개별로 처리.
        private RectTransform photoRoot;

        [Header("Pop-in")]
        [SerializeField] private float popDuration = 0.35f;
        [SerializeField] private AnimationCurve popCurve = new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(0.7f, 1.15f),
            new Keyframe(1f, 1f));
        [SerializeField] private float postPopDelay = 1f;

        [Header("Tear")]
        [SerializeField] private float tearDuration = 0.6f;
        [SerializeField] private float slideDistance = 300f;
        [SerializeField] private float spinDegrees = 45f;

        private Image topFrameImage;
        private RawImage topPhoto;
        private Image bottomFrameImage;
        private RawImage bottomPhoto;
        private Coroutine tearCoroutine;

        private Vector2 topRestPosition;
        private Vector2 bottomRestPosition;

        private void Awake()
        {
            photoRoot = (RectTransform)transform;

            topFrameImage = topFrame.GetComponent<Image>();
            topPhoto = topFrame.GetComponentInChildren<RawImage>(true);
            bottomFrameImage = bottomFrame.GetComponent<Image>();
            bottomPhoto = bottomFrame.GetComponentInChildren<RawImage>(true);

            // 씬에서 맞춰둔 위치(예: Y값)를 기준 위치로 저장. 이후 애니메이션은 전부 이 기준에서 움직임.
            topRestPosition = topFrame.anchoredPosition;
            bottomRestPosition = bottomFrame.anchoredPosition;

            Hide();
        }

        public void SetPhoto(Texture2D snapshot)
        {
            topPhoto.texture = snapshot;
            bottomPhoto.texture = snapshot;
            topPhoto.uvRect = new Rect(0f, 0.5f, 1f, 0.5f);
            bottomPhoto.uvRect = new Rect(0f, 0f, 1f, 0.5f);
        }

        // FeedbackPlayer로 다른 피드백들이랑 같이 재생할 때 쓰는 경로 (완료를 기다리지 않음)
        public override void CreateFeedback()
        {
            if (tearCoroutine != null)
                StopCoroutine(tearCoroutine);

            tearCoroutine = StartCoroutine(PlayRoutine());
        }

        // MapManager처럼 "다 끝날 때까지 기다렸다가 다음 걸로 넘어가야" 할 때 쓰는 경로
        public IEnumerator PlayAndWait(Texture2D snapshot)
        {
            SetPhoto(snapshot);

            if (tearCoroutine != null)
                StopCoroutine(tearCoroutine);

            tearCoroutine = StartCoroutine(PlayRoutine());
            yield return tearCoroutine;
        }

        private IEnumerator PlayRoutine()
        {
            topFrame.gameObject.SetActive(true);
            bottomFrame.gameObject.SetActive(true);

            topFrame.anchoredPosition = topRestPosition;
            bottomFrame.anchoredPosition = bottomRestPosition;
            topFrame.localEulerAngles = Vector3.zero;
            bottomFrame.localEulerAngles = Vector3.zero;
            photoRoot.localScale = Vector3.zero;

            // 1단계: 팝으로 튀어나오기. topFrame/bottomFrame 각자가 아니라 부모(photoRoot)를 통째로
            // 스케일해야 두 조각의 이음매가 어긋나지 않고 하나의 사진처럼 보임.
            float popElapsed = 0f;
            while (popElapsed < popDuration)
            {
                popElapsed += Time.deltaTime;
                float scale = popCurve.Evaluate(popElapsed / popDuration);
                photoRoot.localScale = Vector3.one * scale;
                yield return null;
            }
            photoRoot.localScale = Vector3.one;

            // 잠깐 멈춰서 보여주다가 찢어지기 시작
            yield return new WaitForSeconds(postPopDelay);

            yield return TearRoutine();

            Hide();
        }

        private IEnumerator TearRoutine()
        {
            Vector2 topEnd = topRestPosition + new Vector2(-slideDistance * 0.4f, slideDistance);
            Vector2 bottomEnd = bottomRestPosition + new Vector2(slideDistance * 0.4f, -slideDistance);

            float elapsed = 0f;
            while (elapsed < tearDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / tearDuration;

                topFrame.anchoredPosition = Vector2.Lerp(topRestPosition, topEnd, t);
                bottomFrame.anchoredPosition = Vector2.Lerp(bottomRestPosition, bottomEnd, t);

                topFrame.localEulerAngles = new Vector3(0f, 0f, Mathf.Lerp(0f, spinDegrees, t));
                bottomFrame.localEulerAngles = new Vector3(0f, 0f, Mathf.Lerp(0f, -spinDegrees, t));

                float alpha = Mathf.Lerp(1f, 0f, t);
                SetAlpha(topFrameImage, alpha);
                SetAlpha(topPhoto, alpha);
                SetAlpha(bottomFrameImage, alpha);
                SetAlpha(bottomPhoto, alpha);

                yield return null;
            }
        }

        public override void FinishFeedback()
        {
            if (tearCoroutine != null)
                StopCoroutine(tearCoroutine);

            Hide();
        }

        private void Hide()
        {
            topFrame.gameObject.SetActive(false);
            bottomFrame.gameObject.SetActive(false);

            topFrame.anchoredPosition = topRestPosition;
            bottomFrame.anchoredPosition = bottomRestPosition;
            topFrame.localEulerAngles = Vector3.zero;
            bottomFrame.localEulerAngles = Vector3.zero;
            photoRoot.localScale = Vector3.one;

            SetAlpha(topFrameImage, 1f);
            SetAlpha(topPhoto, 1f);
            SetAlpha(bottomFrameImage, 1f);
            SetAlpha(bottomPhoto, 1f);
        }

        private void SetAlpha(Graphic graphic, float alpha)
        {
            Color c = graphic.color;
            c.a = alpha;
            graphic.color = c;
        }
    }
}
