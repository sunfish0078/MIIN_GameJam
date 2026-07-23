using UnityEngine;
using UnityEngine.InputSystem;

namespace ChoiJeongYun.Scripts.Data
{
    [CreateAssetMenu(fileName = "hash data", menuName = "SO/Hash data", order = 0)]
    public class HashDataSO : ScriptableObject
    {
        [field: SerializeField] public string AssetName { get; private set; }
        [field: SerializeField] public int HashValue { get; private set; }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(AssetName)) return;

            HashValue = Animator.StringToHash(AssetName);
        }
    }
}