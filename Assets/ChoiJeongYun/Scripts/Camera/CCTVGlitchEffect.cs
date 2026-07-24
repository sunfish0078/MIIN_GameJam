using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CCTVGlitchEffect : MonoBehaviour
{
    [SerializeField] private Image glitchImage;
    [SerializeField] private float peakAlpha = 0.8f;
    [SerializeField] private float duration = 0.15f;

    private static readonly int AlphaId = Shader.PropertyToID("_Alpha");

    private Coroutine glitchRoutine;

    private void Awake()
    {
        SetAlpha(0f);
    }

    public void TriggerGlitch()
    {
        if (glitchRoutine != null)
            StopCoroutine(glitchRoutine);

        glitchRoutine = StartCoroutine(GlitchRoutine());
    }

    private IEnumerator GlitchRoutine()
    {
        SetAlpha(peakAlpha);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            SetAlpha(Mathf.Lerp(peakAlpha, 0f, elapsed / duration));
            yield return null;
        }

        SetAlpha(0f);
    }

    private void SetAlpha(float alpha)
    {
        glitchImage.material.SetFloat(AlphaId, alpha);
    }
}
