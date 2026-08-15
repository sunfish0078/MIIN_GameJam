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

        // 남은 체력기준 30% 깎일때 마다
        private int thresholdsCrossed;

        private void Awake()
        {
            currentHealth = DevMode.OneHitKillMonsters ? 1 : maxHealth;
        }

        public void TakeDamage(int damage)
        {
            if (currentHealth <= 0) return;

            currentHealth -= damage;

            if (currentHealth <= 0)
            {
                OnDead?.Invoke();
                return;
            }

            int newThresholdsCrossed = Mathf.FloorToInt((1f - (float)currentHealth / maxHealth) / retreatThresholdPercent);
            if (newThresholdsCrossed > thresholdsCrossed)
            {
                thresholdsCrossed = newThresholdsCrossed;
                OnDamageThresholdReached?.Invoke();
            }
        }

    }
}