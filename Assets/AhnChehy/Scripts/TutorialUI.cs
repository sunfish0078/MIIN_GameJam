using System;
using DG.Tweening;
using UnityEngine;

public class TutorialUI : MonoBehaviour
{
    [SerializeField] private GameObject tutorialPanel0;
    [SerializeField] private GameObject tutorialPanel1;
    [SerializeField] private GameObject tutorialPanel2;
    [SerializeField] private GameObject defaultPanel;
    [SerializeField] private GameObject cameraImage;
    [SerializeField] private GameObject mainMenu;

    [Header("Fade 설정")] 
    [SerializeField] private float visibleDuration = 5f;        // 기본 보여지는 시간
    [SerializeField] private float visibleDurationPanel1 = 7f; // Panel1 보여지는 시간
    [SerializeField] private float fadeDuration = 1f;           // 페이드 인/아웃 걸리는 시간

    private CanvasGroup panel0CanvasGroup;
    private CanvasGroup panel1CanvasGroup;
    private CanvasGroup panel2CanvasGroup;
    private CanvasGroup cameraCanvasGroup;

    private void OnEnable()
    {
        // 이전 실행 중이던 모든 DOTween 정지
        DOTween.Kill(transform);

        panel0CanvasGroup = GetOrAddCanvasGroup(tutorialPanel0);
        panel1CanvasGroup = GetOrAddCanvasGroup(tutorialPanel1);
        panel2CanvasGroup = GetOrAddCanvasGroup(tutorialPanel2);
        cameraCanvasGroup = GetOrAddCanvasGroup(cameraImage);

        // 패널 및 CanvasGroup 알파 초기화
        tutorialPanel0.SetActive(true);
        tutorialPanel1.SetActive(false);
        tutorialPanel2.SetActive(false);
        defaultPanel.SetActive(true);
        cameraImage.SetActive(false);

        panel0CanvasGroup.alpha = 1f;
        panel1CanvasGroup.alpha = 0f;
        panel2CanvasGroup.alpha = 0f;
        cameraCanvasGroup.alpha = 0f;

        PlayFadeOutSequencePanel0();
    }

    private CanvasGroup GetOrAddCanvasGroup(GameObject target)
    {
        CanvasGroup group = target.GetComponent<CanvasGroup>();
        if (group == null)
            group = target.AddComponent<CanvasGroup>();
        return group;
    }

    // --- Panel 0 ---
    private void PlayFadeOutSequencePanel0()
    {
        panel0CanvasGroup.DOKill();
        panel0CanvasGroup.DOFade(0f, fadeDuration)
            .SetDelay(visibleDuration)
            .OnComplete(() =>
            {
                tutorialPanel0.SetActive(false);
                PlayFadeInPanel1();
            });
    }

    // --- Panel 1 ---
    private void PlayFadeInPanel1()
    {
        tutorialPanel1.SetActive(true);
        panel1CanvasGroup.DOKill();
        panel1CanvasGroup.alpha = 0f;

        panel1CanvasGroup.DOFade(1f, fadeDuration)
            .OnComplete(() =>
            {
                // panel1 페이드인 후 cameraImage 페이드인
                CameraImageFadeIn(() => 
                {
                    PlayFadeOutSequencePanel1();
                });
            });
    }

    private void PlayFadeOutSequencePanel1()
    {
        panel1CanvasGroup.DOKill();
        cameraCanvasGroup.DOKill();

        // panel1과 cameraImage를 같이 페이드아웃
        Sequence seq = DOTween.Sequence();
        seq.AppendInterval(visibleDurationPanel1);
        seq.Append(panel1CanvasGroup.DOFade(0f, fadeDuration));
        seq.Join(cameraCanvasGroup.DOFade(0f, fadeDuration));
        seq.OnComplete(() =>
        {
            cameraImage.SetActive(false);
            tutorialPanel1.SetActive(false);
            PlayFadeInPanel2(); 
        });
    }

    // --- Panel 2 ---
    private void PlayFadeInPanel2()
    {
        tutorialPanel2.SetActive(true);
        panel2CanvasGroup.DOKill();
        panel2CanvasGroup.alpha = 0f;

        panel2CanvasGroup.DOFade(1f, fadeDuration)
            .OnComplete(() =>
            {
                CameraImageFadeIn(() =>
                {
                    PlayFadeOutSequencePanel2(); 
                });
            });
    }

    private void PlayFadeOutSequencePanel2()
    {
        panel2CanvasGroup.DOKill();
        cameraCanvasGroup.DOKill();

        Sequence seq = DOTween.Sequence();
        seq.AppendInterval(visibleDuration);
        seq.Append(panel2CanvasGroup.DOFade(0f, fadeDuration));
        seq.Join(cameraCanvasGroup.DOFade(0f, fadeDuration));
        seq.OnComplete(() =>
        {
            cameraImage.SetActive(false);
            tutorialPanel2.SetActive(false);
            defaultPanel.SetActive(false);

            gameObject.SetActive(false); 
            mainMenu.SetActive(true);
        });
    }

    // --- Common ---
    private void CameraImageFadeIn(Action onComplete)
    {
        cameraImage.SetActive(true);
        cameraCanvasGroup.DOKill();
        cameraCanvasGroup.alpha = 0f;

        cameraCanvasGroup.DOFade(1f, fadeDuration)
            .OnComplete(() => onComplete?.Invoke()); 
    }
}