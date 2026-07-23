using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class FlashEffect : MonoBehaviour
{
    [SerializeField] private Image flashImage;
    [SerializeField] private float holdDuration;
    [SerializeField] private float fadeOutDuration;
    [SerializeField] private float cooldown;

    public event Action OnCaptured;

    private bool isFlashing = false;

    private void Awake()
    {
        SetAlpha(0f);
    }

    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Flash();
        }
    }

    private void Flash()
    {
        if (isFlashing) return;

        OnCaptured?.Invoke();
        StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        isFlashing = true;
        SetAlpha(1f);
        
        yield return new WaitForSeconds(holdDuration);
        
        float elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            SetAlpha(Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration));
            yield return null;
        }
        
        SetAlpha(0f);
        
        yield return new WaitForSeconds(cooldown);
        isFlashing = false;
    }

    private void SetAlpha(float alpha)
    {
        Color c = flashImage.color;
        c.a = alpha;
        flashImage.color = c;
    }
}
