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

        // 남은 체력 기준으로 retreatThresholdPercent(기본 30%)씩 깎일 때마다 반복 발동
        // (예: 70% → 40% → 10%, maxHealth 대비 자연스럽게 3번 정도)
        private int thresholdsCrossed;

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        public void TakeDamage(int damage)
        {
            if (currentHealth <= 0) return;

            currentHealth -= damage;

            if (currentHealth <= 0)
            {
                // 이 타격으로 죽었으면 도주 트리거는 굳이 안 함 — 사망 연출과 겹쳐서 꼬임 (AbstractEnemy.TakeDamage와 동일한 이유)
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