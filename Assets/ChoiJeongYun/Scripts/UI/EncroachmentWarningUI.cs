using System.Collections;
using TMPro;
using UnityEngine;

namespace ChoiJeongYun.Scripts.Anomaly
{
    public class EncroachmentWarningUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text warningText;

        [Header("Tier 1")]
        [SerializeField, TextArea] private string tier1Message = "잠식이 깊어지고 있습니다";
        [SerializeField] private Color tier1Color = Color.white;

        [Header("Tier 2")]
        [SerializeField, TextArea] private string tier2Message = "잠식이 점점 깊어지고 있습니다..";
        [SerializeField] private Color tier2Color = Color.red;

        [Header("타이밍")]
        [SerializeField] private float fadeInDuration = 0.4f;
        [SerializeField] private float holdDuration = 1.5f;
        [SerializeField] private float fadeOutDuration = 0.6f;

        private Coroutine activeRoutine;

        private void Awake()
        {
            SetAlpha(0f);
        }

        public void ShowTier1()
        {
            Show(tier1Message, tier1Color);
        }

        public void ShowTier2()
        {
            Show(tier2Message, tier2Color);
        }

        private void Show(string message, Color color)
        {
            if (activeRoutine != null)
                StopCoroutine(activeRoutine);

            activeRoutine = StartCoroutine(ShowRoutine(message, color));
        }

        private IEnumerator ShowRoutine(string message, Color color)
        {
            warningText.text = message;
            Color baseColor = new Color(color.r, color.g, color.b, 0f);
            warningText.color = baseColor;

            float elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                SetAlpha(Mathf.Lerp(0f, 1f, elapsed / fadeInDuration), color);
                yield return null;
            }
            SetAlpha(1f, color);

            yield return new WaitForSeconds(holdDuration);

            elapsed = 0f;
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                SetAlpha(Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration), color);
                yield return null;
            }
            SetAlpha(0f, color);

            activeRoutine = null;
        }

        private void SetAlpha(float alpha)
        {
            Color c = warningText.color;
            c.a = alpha;
            warningText.color = c;
        }

        private void SetAlpha(float alpha, Color color)
        {
            warningText.color = new Color(color.r, color.g, color.b, alpha);
        }
    }
}
