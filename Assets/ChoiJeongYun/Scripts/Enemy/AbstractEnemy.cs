using UnityEngine;
using UnityEngine.Events;

namespace ChoiJeongYun.Scripts.Enemy
{
    public class AbstractEnemy : MonoBehaviour, IDamageable
    {
        public UnityEvent OnDeadEvent;
        public UnityEvent OnHitEvent;
        
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
            OnDeadEvent?.Invoke();
        }

        public virtual void TakeDamage(int damage)
        {
            if (IsDead) return;

            HealthCompo.TakeDamage(damage);

            // 이번 타격으로 죽었으면 피격 반응(블링크/도망)은 굳이 재생 안 함 — 사망 연출과 겹쳐서 꼬임
            if (IsDead) return;

            OnHitEvent?.Invoke();
        }
    }
}