using TMPro;
using UnityEngine;

public class CCTVRoomLabel : MonoBehaviour
{
    [SerializeField] private TMP_Text roomNameText;

    private void Update()
    {
        if (CCTVController.Instance == null) return;

        roomNameText.text = CCTVController.Instance.GetCurrentRoomName();
    }
}
