using DG.Tweening;
using UnityEngine;

public class TutorialUI : MonoBehaviour
{
    [SerializeField] private GameObject[] panels; // 순서대로. 첫 패널은 카메라 이미지 없이, 나머지는 카메라 이미지와 함께 보임
    [SerializeField] private GameObject defaultPanel;
    [SerializeField] private GameObject cameraImage;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private float fadeDuration = 1f;

    private CanvasGroup[] panelGroups;
    private CanvasGroup cameraGroup;
    private int currentIndex;
    private bool isAnimating;

    private void OnEnable()
    {
        cameraGroup = GetOrAddCanvasGroup(cameraImage);
        cameraGroup.DOKill();
        cameraImage.SetActive(false);
        cameraGroup.alpha = 0f;

        panelGroups = new CanvasGroup[panels.Length];
        for (int i = 0; i < panels.Length; i++)
        {
            CanvasGroup group = GetOrAddCanvasGroup(panels[i]);
            group.DOKill();
            panelGroups[i] = group;

            panels[i].SetActive(i == 0);
            group.alpha = i == 0 ? 1f : 0f;
        }

        defaultPanel.SetActive(true);
        currentIndex = 0;
        isAnimating = false;
    }

    private CanvasGroup GetOrAddCanvasGroup(GameObject target)
    {
        CanvasGroup group = target.GetComponent<CanvasGroup>();
        if (group == null)
            group = target.AddComponent<CanvasGroup>();
        return group;
    }

    // 화면(또는 "다음" 버튼)의 OnClick에 연결. 클릭할 때마다 다음 설명으로 넘어가고,
    // 마지막 설명에서 클릭하면 튜토리얼을 닫고 메인 메뉴로 돌아감.
    public void OnClickAdvance()
    {
        if (isAnimating || panels.Length == 0) return;

        isAnimating = true;
        bool isLast = currentIndex >= panels.Length - 1;

        HideCurrent(() =>
        {
            if (isLast)
            {
                defaultPanel.SetActive(false);
                gameObject.SetActive(false);
                if (mainMenu != null) mainMenu.SetActive(true);
                return;
            }

            currentIndex++;
            ShowCurrent(() => isAnimating = false);
        });
    }

    // 즉시 닫고 메인 메뉴로 (X 버튼 등에 연결하고 싶으면 사용)
    public void SkipTutorial()
    {
        if (panelGroups != null)
            foreach (CanvasGroup group in panelGroups)
                group?.DOKill();

        cameraGroup?.DOKill();

        if (panels != null)
            foreach (GameObject panel in panels)
                if (panel != null) panel.SetActive(false);

        if (cameraImage != null) cameraImage.SetActive(false);
        if (defaultPanel != null) defaultPanel.SetActive(false);

        gameObject.SetActive(false);
        if (mainMenu != null) mainMenu.SetActive(true);

        isAnimating = false;
    }

    // 현재 패널 숨기기. 첫 패널(카메라 이미지 없음)이면 패널만, 나머지는 카메라 이미지도 같이.
    private void HideCurrent(System.Action onComplete)
    {
        CanvasGroup group = panelGroups[currentIndex];
        GameObject panel = panels[currentIndex];
        group.DOKill();

        if (currentIndex == 0)
        {
            group.DOFade(0f, fadeDuration).OnComplete(() =>
            {
                panel.SetActive(false);
                onComplete?.Invoke();
            });
            return;
        }

        cameraGroup.DOKill();
        Sequence seq = DOTween.Sequence();
        seq.Append(group.DOFade(0f, fadeDuration));
        seq.Join(cameraGroup.DOFade(0f, fadeDuration));
        seq.OnComplete(() =>
        {
            panel.SetActive(false);
            cameraImage.SetActive(false);
            onComplete?.Invoke();
        });
    }

    // 다음 패널 보이기. 첫 패널이 아니면 패널 페이드인 후 카메라 이미지도 페이드인.
    private void ShowCurrent(System.Action onComplete)
    {
        CanvasGroup group = panelGroups[currentIndex];
        GameObject panel = panels[currentIndex];

        panel.SetActive(true);
        group.DOKill();
        group.alpha = 0f;

        if (currentIndex == 0)
        {
            group.DOFade(1f, fadeDuration).OnComplete(() => onComplete?.Invoke());
            return;
        }

        group.DOFade(1f, fadeDuration).OnComplete(() =>
        {
            cameraImage.SetActive(true);
            cameraGroup.DOKill();
            cameraGroup.alpha = 0f;

            cameraGroup.DOFade(1f, fadeDuration).OnComplete(() => onComplete?.Invoke());
        });
    }
}
