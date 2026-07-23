using System;
using UnityEngine;

namespace ChoiJeongYun.Scripts.Enemy
{
    public class EnemyAnimationTrigger : MonoBehaviour
    {
        public event Action OnAnimationEnd;

        private void AnimationEndTrigger()
        {
            OnAnimationEnd?.Invoke();
        }
    }
}