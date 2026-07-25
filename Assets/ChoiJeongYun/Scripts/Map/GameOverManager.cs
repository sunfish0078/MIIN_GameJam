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
            StartCoroutine(DeathSequence(null));
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
            PhotoTransitionEffect.SetInputLocked(true);

            if (CCTVController.Instance != null)
                CCTVController.Instance.ShowControlRoom();

            // 뒤에 깔리던 환풍기 소리 등은 서서히 줄어들며 멈춤
            if (SoundManager.Instance != null)
                SoundManager.Instance.FadeOutAmbient(fadeDuration);

            // 1. 몬스터 이미지가 화면에 팍 나타남 (연출 없이 바로)
            if (jumpscareImage != null)
            {
                jumpscareImage.sprite = monster != null ? monster.JumpscareSprite : null;
                jumpscareImage.transform.localScale = Vector3.one;
                jumpscareImage.gameObject.SetActive(true);
            }

            yield return new WaitForSeconds(jumpscareHoldDuration);

            // 2. 검은 화면도 바로 확 (페이드 없이) + 비명 동시 재생
            if (blackPanel != null)
                SetBlackAlpha(1f);

            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySFX(screamSound);

            yield return new WaitForSeconds(screamSound != null ? screamSound.length : 0.5f);

            // 4. 엔딩 화면
            if (jumpscareImage != null)
                jumpscareImage.gameObject.SetActive(false);

            if (RoomTimer.Instance != null)
                RoomTimer.Instance.StopRoom();

            if (MainMenuUIManager.Instance != null)
                MainMenuUIManager.Instance.ShowGameOverPanel();
        }

        private void SetBlackAlpha(float alpha)
        {
            Color c = blackPanel.color;
            c.a = alpha;
            blackPanel.color = c;
        }
    }
}
