using UnityEngine;
using UnityEngine.EventSystems;

public class TutorialHoverPrompt : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject promptObject;

    private void Awake()
    {
        if (promptObject != null)
            promptObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (promptObject != null)
            promptObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (promptObject != null)
            promptObject.SetActive(false);
    }

    private void OnDisable()
    {
        if (promptObject != null)
            promptObject.SetActive(false);
    }
}
