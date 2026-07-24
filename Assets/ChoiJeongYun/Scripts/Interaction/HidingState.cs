using UnityEngine;

namespace ChoiJeongYun.Scripts.Interaction
{
    public class HidingState : MonoBehaviour
    {
        public static HidingState Instance { get; private set; }

        public bool IsHidden { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }
        
        public void Hide()
        {
            IsHidden = true;
        }
        
        public void Unhide()
        {
            IsHidden = false;
        }
    }
}
