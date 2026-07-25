using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace ChoiJeongYun.Scripts.Interaction
{
    public class InteractionHotspot : MonoBehaviour
    {
        [SerializeField] private Camera viewCamera;
        [SerializeField] private Collider2D hotspotCollider;
        [SerializeField] private CinemachineCamera targetCamera;
        [SerializeField] private int activePriority = 30;
        [SerializeField] private CinemachineCamera[] camerasToDeactivate;
        [SerializeField] private int inactivePriority = 0;
        [SerializeField] private string promptMessage = "숨기";
        [SerializeField] private float clickCooldown = 0.3f;
        [SerializeField] private UnityEvent onInteract;
        [SerializeField] private AudioClip interactSound;

        private CinemachineBrain brain;
        private bool isHovered;
        private float lastInteractTime = -Mathf.Infinity;

        private void Awake()
        {
            brain = viewCamera.GetComponent<CinemachineBrain>();
        }

        private void Update()
        {
            if (PhotoTransitionEffect.IsPlaying)
            {
                if (isHovered)
                {
                    InteractionPromptUI.Instance.Hide(this);
                    isHovered = false;
                }
                return;
            }

            if (brain.IsLiveChild(targetCamera))
            {
                if (isHovered)
                {
                    InteractionPromptUI.Instance.Hide(this);
                    isHovered = false;
                }
                return;
            }

            bool nowHovered = CheckHover();
            if (nowHovered)
                InteractionPromptUI.Instance.Show(this, GetDisplayMessage());
            else
                InteractionPromptUI.Instance.Hide(this);

            isHovered = nowHovered;

            if (isHovered && Mouse.current.leftButton.wasPressedThisFrame && Time.time - lastInteractTime >= clickCooldown)
            {
                lastInteractTime = Time.time;

                targetCamera.Priority = activePriority;

                foreach (CinemachineCamera cam in camerasToDeactivate)
                {
                    if (cam != null)
                        cam.Priority = inactivePriority;
                }

                if (SoundManager.Instance != null)
                    SoundManager.Instance.PlaySFX(interactSound);

                onInteract?.Invoke();
            }
        }

        private string GetDisplayMessage()
        {
            float remaining = clickCooldown - (Time.time - lastInteractTime);
            return remaining > 0f ? $"{promptMessage} - {remaining:F1}s" : promptMessage;
        }

        private bool CheckHover()
        {
            Vector2 screenPoint = Mouse.current.position.ReadValue();
            Vector2 worldPoint = viewCamera.ScreenToWorldPoint(screenPoint);
            return hotspotCollider.OverlapPoint(worldPoint);
        }

        private void OnDisable()
        {
            if (isHovered && InteractionPromptUI.Instance != null)
                InteractionPromptUI.Instance.Hide(this);

            isHovered = false;
        }
    }
}
