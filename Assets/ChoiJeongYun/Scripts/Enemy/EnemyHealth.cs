using System;
using UnityEngine;

namespace ChoiJeongYun.Scripts.Enemy
{
    public class EnemyHealth : MonoBehaviour
    {
        public event Action OnDead;
        public event Action OnDamageThresholdReached;

        [SerializeField] private int maxHealth;
        [SerializeField] private int currentHealth;
        [SerializeField, Range(0f, 1f)] private float retreatThresholdPercent = 0.3f;

        private bool thresholdReached;

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        public void TakeDamage(int damage)
        {
            if (currentHealth <= 0) return;

            currentHealth -= damage;

            if (!thresholdReached && currentHealth <= maxHealth * (1f - retreatThresholdPercent))
            {
                thresholdReached = true;
                OnDamageThresholdReached?.Invoke();
            }

            if (currentHealth <= 0)
            {
                OnDead?.Invoke();
            }
        }

    }
}