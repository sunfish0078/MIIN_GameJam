using UnityEngine;
using UnityEngine.Events;

namespace ChoiJeongYun.Scripts.Enemy
{
    public class AbstractEnemy : MonoBehaviour, IDamageable
    {
        public UnityEvent OnDeadEvent;
        public UnityEvent OnHitEvent;

        [SerializeField] private AudioClip deathSound;

        public EnemyMovement MovementCompo {get; private set;}
        public EnemyAnimator AnimatorCompo {get; private set;}
        public EnemyHealth  HealthCompo {get; private set;}
        
        public bool IsDead {get; private set;}

        protected virtual void Awake()
        {
            MovementCompo = GetComponentInChildren<EnemyMovement>();
            AnimatorCompo = GetComponentInChildren<EnemyAnimator>();
            HealthCompo = GetComponentInChildren<EnemyHealth>();

            HealthCompo.OnDead += HandleOnDead;
            HealthCompo.OnDamageThresholdReached += MovementCompo.BeginRetreatAndApproach;
            OnHitEvent.AddListener(MovementCompo.HandleHit);
        }

        private void HandleOnDead()
        {
            IsDead = true;

            if (SoundManager.Instance != null)
                SoundManager.Instance.PlaySFX(deathSound);

            OnDeadEvent?.Invoke();
        }

        public virtual void TakeDamage(int damage)
        {
            if (IsDead) return;

            HealthCompo.TakeDamage(damage);
            
            if (IsDead) return;

            OnHitEvent?.Invoke();
        }
    }
}