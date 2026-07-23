using System.Collections;
using UnityEngine;

namespace ChoiJeongYun.Scripts.Feedback
{
    public class BlinkFeedback : AbstractFeedback
    {
        [SerializeField] private SpriteRenderer targetRenderer;
        [SerializeField] private float blinkAmount = 0.4f;
        [SerializeField] private float blinkDuration = 0.3f;

        private MaterialPropertyBlock _mpb;

        private readonly int _blinkHash = Shader.PropertyToID("_BlinkValue");
        private Coroutine _blinkCoroutine = null;

        private void Awake()
        {
            _mpb = new MaterialPropertyBlock();
            targetRenderer.GetPropertyBlock(_mpb);
        }

        public override void CreateFeedback()
        {
            if (_blinkCoroutine != null)
                StopCoroutine(_blinkCoroutine);

            _blinkCoroutine = StartCoroutine(BlinkCoroutine());
        }

        private IEnumerator BlinkCoroutine()
        {
            SetBlink(blinkAmount);

            float elapsed = 0f;
            while (elapsed < blinkDuration)
            {
                elapsed += Time.deltaTime;
                SetBlink(Mathf.Lerp(blinkAmount, 0f, elapsed / blinkDuration));
                yield return null;
            }

            SetBlink(0f);
        }

        private void SetBlink(float value)
        {
            _mpb.SetFloat(_blinkHash, value);
            targetRenderer.SetPropertyBlock(_mpb);
        }

        public override void FinishFeedback()
        {
            if (_blinkCoroutine != null)
                StopCoroutine(_blinkCoroutine);

            SetBlink(0f);
        }
    }
}
