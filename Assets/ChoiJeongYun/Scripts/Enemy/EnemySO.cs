using UnityEngine;

namespace ChoiJeongYun.Scripts.Enemy
{
    [CreateAssetMenu(fileName = "Enemy data", menuName = "SO/Enemy data", order = 0)]
    public class EnemySO : ScriptableObject
    {
        public string EnemyName;
    }
}