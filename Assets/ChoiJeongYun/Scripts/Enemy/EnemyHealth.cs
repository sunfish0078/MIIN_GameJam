using System;
using UnityEngine;

namespace ChoiJeongYun.Scripts.Enemy
{
    public class EnemyHealth : MonoBehaviour
    {
        public event Action OnDead;
        
        [SerializeField] private int maxHealth;
        [SerializeField] private int currentHealth;

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        public void TakeDamage(int damage)
        {
            if (currentHealth <= 0) return;
            
            currentHealth -= damage;
            Debug.Log("입은 데미지: " +  damage);
            
            if (currentHealth <= 0)
            {
                OnDead?.Invoke();
            }
        }
        
    }
}