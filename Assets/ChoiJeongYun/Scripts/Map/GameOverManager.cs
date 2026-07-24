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

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
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
                // 숨어서 몬스터가 실패 → 순찰 상태로 복귀
                monster.ResetToPatrol();
                return;
            }

            StartCoroutine(DeathSequence());
        }

        private IEnumerator DeathSequence()
        {
            PhotoTransitionEffect.SetInputLocked(true);

            if (CCTVController.Instance != null)
                CCTVController.Instance.ShowControlRoom();

            if (blackPanel != null)
                yield return Fade(0f, 1f, fadeDuration);

            if (MainMenuUIManager.Instance != null)
                MainMenuUIManager.Instance.ShowGameOverPanel();
        }

        private IEnumerator Fade(float from, float to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                SetBlackAlpha(Mathf.Lerp(from, to, elapsed / duration));
                yield return null;
            }

            SetBlackAlpha(to);
        }

        private void SetBlackAlpha(float alpha)
        {
            Color c = blackPanel.color;
            c.a = alpha;
            blackPanel.color = c;
        }
    }
}
