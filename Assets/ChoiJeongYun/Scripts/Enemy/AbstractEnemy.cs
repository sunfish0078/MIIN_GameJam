using UnityEngine;
using UnityEngine.Events;

namespace ChoiJeongYun.Scripts.Enemy
{
    public class AbstractEnemy : MonoBehaviour, IDamageable
    {
        public UnityEvent OnDeadEvent;
        public UnityEvent OnHitEvent;

        [SerializeField] private AudioClip deathSound;
        [SerializeField, Range(0f, 1f)] private float deathSoundVolume = 1f;

        public float DeathSoundLength
        {
            get
            {
                AudioClip clip = SoundManager.Instance != null ? SoundManager.Instance.ResolveMonsterSFX(deathSound) : deathSound;
                return clip != null ? clip.length : 0f;
            }
        }

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
                SoundManager.Instance.PlayMonsterSFX(deathSound, deathSoundVolume);

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