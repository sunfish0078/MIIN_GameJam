using System;
using System.Collections;
using UnityEngine;

namespace ChoiJeongYun.Scripts.Enemy
{
    public class EnemyMovement : MonoBehaviour
    {
        private enum State
        {
            Patrolling,
            Fleeing,
            Retreating,
            AtControlRoom
        }
        
        private Transform[] patrolPoints;

        [Header("CCTV 전환 시 이동")]
        [SerializeField] private int switchesPerMove = 3;

        [Header("피격 시 페이드")]
        [SerializeField] private float hitFeedbackDelay = 0.3f;
        [SerializeField] private float fadeOutDuration = 0.5f;

        [Header("관리실 접근")]
        [SerializeField] private float approachDuration = 10f;
        [SerializeField] private float footstepWarningLead = 1.5f;

        [Header("사운드/연출")]
        [SerializeField] private AudioClip chaseCrySound;
        [SerializeField] private Sprite jumpscareSprite;

        public Sprite JumpscareSprite => jumpscareSprite;
        
        public event Action OnFootstepWarning;
        
        public event Action<EnemyMovement> OnReachedControlRoom;

        private SpriteRenderer spriteRenderer;
        private Collider2D bodyCollider;

        private State state = State.Patrolling;
        private int switchCount;
        private Coroutine activeRoutine;

        private void Awake()
        {
            spriteRenderer = transform.parent.GetComponentInChildren<SpriteRenderer>();
            bodyCollider = transform.parent.GetComponent<Collider2D>();
        }
        
        public void SetPatrolPoints(Transform[] points)
        {
            patrolPoints = points;
        }

        private void OnEnable()
        {
            if (CCTVController.Instance != null)
                CCTVController.Instance.OnCameraSwitched += HandleCameraSwitched;
        }

        private void OnDisable()
        {
            if (CCTVController.Instance != null)
                CCTVController.Instance.OnCameraSwitched -= HandleCameraSwitched;
        }

        private void HandleCameraSwitched()
        {
            if (state != State.Patrolling) return;

            switchCount++;
            if (switchCount >= switchesPerMove)
            {
                switchCount = 0;
                TeleportToDifferentPoint();
            }
        }

        public void HandleHit()
        {
            if (state != State.Patrolling) return;

            if (activeRoutine != null)
                StopCoroutine(activeRoutine);

            activeRoutine = StartCoroutine(FleeRoutine());
        }

        private IEnumerator FleeRoutine()
        {
            state = State.Fleeing;
            
            yield return new WaitForSeconds(hitFeedbackDelay);

            yield return Fade(1f, 0f, fadeOutDuration);

            TeleportToDifferentPoint();

            SetAlpha(1f);
            state = State.Patrolling;
            activeRoutine = null;
        }

        public void BeginRetreatAndApproach()
        {
            if (activeRoutine != null)
                StopCoroutine(activeRoutine);

            activeRoutine = StartCoroutine(RetreatAndApproachRoutine());
        }

        private IEnumerator RetreatAndApproachRoutine()
        {
            state = State.Retreating;
            
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySFX(chaseCrySound);
            
            yield return new WaitForSeconds(hitFeedbackDelay);
            yield return Fade(1f, 0f, fadeOutDuration);

            if (spriteRenderer != null) spriteRenderer.enabled = false;
            if (bodyCollider != null) bodyCollider.enabled = false;

            yield return new WaitForSeconds(approachDuration - footstepWarningLead);

            OnFootstepWarning?.Invoke();

            yield return new WaitForSeconds(footstepWarningLead);

            state = State.AtControlRoom;
            OnReachedControlRoom?.Invoke(this);

            activeRoutine = null;
        }
        
        public void ResetToPatrol()
        {
            if (activeRoutine != null)
                StopCoroutine(activeRoutine);

            state = State.Patrolling;

            SetAlpha(1f);
            if (spriteRenderer != null) spriteRenderer.enabled = true;
            if (bodyCollider != null) bodyCollider.enabled = true;

            TeleportToDifferentPoint();
            activeRoutine = null;
        }

        private void TeleportToDifferentPoint()
        {
            if (patrolPoints == null || patrolPoints.Length < 2) return;

            int index;
            Vector3 currentPos = transform.parent.position;
            do
            {
                index = UnityEngine.Random.Range(0, patrolPoints.Length);
            }
            while (patrolPoints[index].position == currentPos);

            transform.parent.position = patrolPoints[index].position;
        }

        private IEnumerator Fade(float from, float to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                SetAlpha(Mathf.Lerp(from, to, elapsed / duration));
                yield return null;
            }

            SetAlpha(to);
        }

        private void SetAlpha(float alpha)
        {
            if (spriteRenderer == null) return;

            Color c = spriteRenderer.color;
            c.a = alpha;
            spriteRenderer.color = c;
        }
    }
}
