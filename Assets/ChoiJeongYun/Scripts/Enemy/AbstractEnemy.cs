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
            OnHitEvent?.Invoke();
        }
    }
}