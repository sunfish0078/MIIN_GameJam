using System.Collections;
using ChoiJeongYun.Scripts.Anomaly;
using ChoiJeongYun.Scripts.Enemy;
using ChoiJeongYun.Scripts.Interaction;
using ChoiJeongYun.Scripts.Timer;
using UnityEngine;
using UnityEngine.UI;

namespace ChoiJeongYun.Scripts.Map
{
    public class GameOverManager : MonoBehaviour
    {
        public static GameOverManager Instance { get; private set; }

        [Header("몬스터 접근 경고")]
        [SerializeField] private EncroachmentWarningUI toastUI;
        [SerializeField] private Color approachToastColor = Color.blue;

        [Header("사망 연출")]
        [SerializeField] private Image blackPanel;
        [SerializeField] private float fadeDuration = 0.5f;
        [SerializeField] private AudioClip screamSound;

        [Header("점프스케어 (몬스터 이미지, 화면 꽉 채움)")]
        [SerializeField] private Image jumpscareImage;
        [SerializeField] private float jumpscareHoldDuration = 0.5f;


        [Header("잠식(시간초과) 사망 연출 - 몬스터 점프스케어 없이 방별 이미지로 화면 페이드")]
        [SerializeField] private float encroachmentFadeDuration = 2f;
        [SerializeField] private float encroachmentHoldDuration = 2f;
        [SerializeField] private float glitchJitterAmount = 20f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            // 잠식 타이머가 0이 되면(RoomTimer.cs) 관리실 침입과 동일한 연출로 이어짐
            if (RoomTimer.Instance != null)
                RoomTimer.Instance.OnEncroachmentReached += HandleEncroachmentReached;
        }

        private void HandleEncroachmentReached()
        {
            StartCoroutine(EncroachmentDeathSequence());
        }

        public void HandleFootstepWarning()
        {
            if (toastUI != null)
                toastUI.ShowCustomMessage("오고있어..", approachToastColor);

            if (RoomTimer.Instance != null)
                RoomTimer.Instance.ShowApproachWarning();
        }

        public void HandleMonsterReachedControlRoom(EnemyMovement monster)
        {
            if (RoomTimer.Instance != null)
                RoomTimer.Instance.HideApproachWarning();

            if (HidingState.Instance != null && HidingState.Instance.IsHidden)
            {
                // 숨어서 몬스터가 실패 → 순찰 상태로 복귀. 재생 중이던 (긴) 울음소리는 서서히 줄여서 끔
                if (SoundManager.Instance != null)
                    SoundManager.Instance.FadeOutSFX(1f);

                monster.ResetToPatrol();
                return;
            }

            StartCoroutine(DeathSequence(monster));
        }

        private IEnumerator DeathSequence(EnemyMovement monster)
        {
            // 이 연출이 재생되는 동안 잠식 타이머가 0이 되어 EncroachmentDeathSequence가 동시에
            // 겹쳐 터지는 걸 막기 위해 즉시 정지 (MapManager.HandleMonsterDead와 동일한 이유)
            if (RoomTimer.Instance != null)
                RoomTimer.Instance.StopRoom();

            PhotoTransitionEffect.SetInputLocked(true);

            yield return BeginDeathFade();

            // 1. 몬스터 이미지가 화면에 팍 나타남 (연출 없이 바로) - 쫄보모드면 이 몬스터의 CuteEnemy가 지정한 이미지로 교체
            if (jumpscareImage != null)
            {
                CuteEnemy cute = monster.GetComponentInParent<CuteEnemy>();
                jumpscareImage.sprite = (cute != null && cute.JumpscareOverride != null) ? cute.JumpscareOverride : monster.JumpscareSprite;
                jumpscareImage.transform.localScale = Vector3.one;
                SetImageAlpha(jumpscareImage, 1f);
                jumpscareImage.gameObject.SetActive(true);
            }

            yield return new WaitForSeconds(jumpscareHoldDuration);

            // 2. 검은 화면도 바로 확 (페이드 없이) + 비명 동시 재생 (쫄보모드면 SoundManager가 알아서 공용 cuteSound로 교체)
            if (blackPanel != null)
                SetImageAlpha(blackPanel, 1f);

            AudioClip actualScream = SoundManager.Instance != null ? SoundManager.Instance.ResolveMonsterSFX(screamSound) : screamSound;

            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySFX(actualScream);

            yield return new WaitForSeconds(actualScream != null ? actualScream.length : 0.5f);

            FinishDeathSequence();
        }

        // 몬스터한테 잡힌 경우(점프스케어+비명)와 달리, 시간초과(잠식) 사망은 점프스케어/비명 없이
        // 방마다 지정된 이미지(RoomMapSO.encroachmentDeathImage)로 화면 전체가 조용히 페이드되는 연출
        private IEnumerator EncroachmentDeathSequence()
        {
            PhotoTransitionEffect.SetInputLocked(true);

            yield return BeginDeathFade();

            Sprite deathSprite = MapManager.Instance != null ? MapManager.Instance.GetCurrentEncroachmentDeathImage() : null;

            if (jumpscareImage != null && deathSprite != null)
            {
                jumpscareImage.sprite = deathSprite;
                jumpscareImage.transform.localScale = Vector3.one;
                SetImageAlpha(jumpscareImage, 0f);
                jumpscareImage.gameObject.SetActive(true);

                yield return GlitchRevealImage(jumpscareImage, encroachmentFadeDuration);
            }

            yield return new WaitForSeconds(encroachmentHoldDuration);

            FinishDeathSequence();
        }

        // DeathSequence/EncroachmentDeathSequence 공통 시작부: 관리실로 전환하고 배경음/효과음을 서서히 줄임
        private IEnumerator BeginDeathFade()
        {
            if (CCTVController.Instance != null)
                CCTVController.Instance.ShowControlRoom();

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.FadeOutAmbient(fadeDuration);
                SoundManager.Instance.FadeOutSFX(fadeDuration);
            }

            yield return new WaitForSeconds(fadeDuration);
        }

        // DeathSequence/EncroachmentDeathSequence 공통 마무리: 점프스케어 이미지 끄고 타이머 정지, 게임오버 패널 표시
        private void FinishDeathSequence()
        {
            if (jumpscareImage != null)
                jumpscareImage.gameObject.SetActive(false);

            if (RoomTimer.Instance != null)
                RoomTimer.Instance.StopRoom();

            if (MainMenuUIManager.Instance != null)
                MainMenuUIManager.Instance.ShowGameOverPanel();
        }

        // 알파를 랜덤하게 깜빡이고 좌우로 흔들다가, 시간이 지날수록 흔들림이 잦아들며 완전히 안정되는 지직거림 연출
        private IEnumerator GlitchRevealImage(Image image, float duration)
        {
            RectTransform rect = image.rectTransform;
            Vector2 basePos = rect.anchoredPosition;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float settle = 1f - t;

                float alpha = Mathf.Clamp01(t + Random.Range(-0.35f, 0.35f) * settle);
                if (Random.value < 0.1f * settle)
                    alpha = 0f;

                SetImageAlpha(image, alpha);
                rect.anchoredPosition = basePos + new Vector2(Random.Range(-glitchJitterAmount, glitchJitterAmount) * settle, 0f);

                yield return null;
            }

            rect.anchoredPosition = basePos;
            SetImageAlpha(image, 1f);
        }

        private void SetImageAlpha(Image image, float alpha)
        {
            Color c = image.color;
            c.a = alpha;
            image.color = c;
        }
    }
}
