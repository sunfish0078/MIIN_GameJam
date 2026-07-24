using System.Collections;
using ChoiJeongYun.Scripts.Feedback;
using UnityEngine;

namespace ChoiJeongYun.Scripts.Anomaly
{
    public class RoomAnomaly : MonoBehaviour, IPhotographable
    {
        [SerializeField] private FeedbackPlayer hitFeedback;
        [SerializeField] private float destroyDelay = 0.3f;
        [SerializeField] private float fadeOutDuration = 0.4f;

        private Collider2D col;
        private SpriteRenderer spriteRenderer;
        private bool isDead;

        private void Awake()
        {
            col = GetComponentInChildren<Collider2D>();
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        public void OnPhotographed(Texture2D snapshot)
        {
            // 카메라 전체에 다 담겨야만 촬영이 인정되는 판정이라(PhotoItemDetector.IsFullyContained),
            // 그 자체로 난이도가 있어서 한 방에 없어져도 됨 — 체력 없이 바로 사망 처리.
            if (isDead) return;
            isDead = true;

            if (col != null)
                col.enabled = false;

            if (hitFeedback != null)
                hitFeedback.PlayFeedback();

            StartCoroutine(DestroyAfterDelay());
        }

        private IEnumerator DestroyAfterDelay()
        {
            yield return new WaitForSeconds(destroyDelay);

            if (spriteRenderer != null)
            {
                float elapsed = 0f;
                Color c = spriteRenderer.color;
                while (elapsed < fadeOutDuration)
                {
                    elapsed += Time.deltaTime;
                    c.a = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
                    spriteRenderer.color = c;
                    yield return null;
                }
            }

            Destroy(gameObject);
        }
    }
}
