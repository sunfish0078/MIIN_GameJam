using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ChoiJeongYun.Scripts.Interaction
{
    public class InteractionPromptUI : MonoBehaviour
    {
        public static InteractionPromptUI Instance { get; private set; }

        [SerializeField] private GameObject promptRoot;
        [SerializeField] private TMP_Text promptText;
        [SerializeField] private Vector2 mouseOffset = new Vector2(20f, -20f);

        private RectTransform _promptRect;
        private RectTransform _parentRect;
        private Canvas _canvas;
        private bool _isShown;
        private Object _currentOwner;

        private void Awake()
        {
            Instance = this;
            
            _promptRect = promptRoot.GetComponent<RectTransform>();
            _parentRect = _promptRect.parent as RectTransform;
            _canvas = _promptRect.GetComponentInParent<Canvas>();

            Hide(null);
        }

        private void Update()
        {
            if (!_isShown || Mouse.current == null || _parentRect == null || _canvas == null)
                return;

            Vector2 mouseScreenPos = Mouse.current.position.ReadValue() + mouseOffset;
            Camera cam = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_parentRect, mouseScreenPos, cam, out Vector2 localPoint))
            {
                _promptRect.anchoredPosition = localPoint;
            }
        }

        public void Show(Object owner, string message)
        {
            if (promptRoot == null || promptText == null)
                return;

            _currentOwner = owner;
            promptText.text = message;
            promptRoot.SetActive(true);
            _isShown = true;
        }

        public void Hide(Object owner)
        {
            if (promptRoot == null)
                return;

            if (_currentOwner != null && _currentOwner != owner)
                return;

            _currentOwner = null;
            promptRoot.SetActive(false);
            _isShown = false;
        }
    }
}
