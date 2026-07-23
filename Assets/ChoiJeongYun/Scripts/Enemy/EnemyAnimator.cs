using UnityEngine;

namespace ChoiJeongYun.Scripts.Enemy
{
    [RequireComponent(typeof(Animator))]
    public class EnemyAnimator : MonoBehaviour
    {
        public Animator Animator { get; private set; }

        protected virtual void Awake()
        {
            Animator = GetComponent<Animator>();
        }
        
        public void SetBool(int hash, bool value) => Animator.SetBool(hash, value);
        public void SetFloat(int hash, float value) => Animator.SetFloat(hash, value);
    }
}