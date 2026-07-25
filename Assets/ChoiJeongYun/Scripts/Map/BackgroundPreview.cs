using UnityEngine;

public class BackgroundPreview : MonoBehaviour
{
    [SerializeField] private RoomMapSO previewMap;

    private const int RoomCount = 7;
    private const string ControlRoomObjectName = "ControlRoomBG";

    [ContextMenu("Apply Preview")]
    public void ApplyPreview()
    {
        if (previewMap == null)
            return;

        Sprite[] roomSprites =
        {
            previewMap.room1, previewMap.room2, previewMap.room3, previewMap.room4,
            previewMap.room5, previewMap.room6, previewMap.room7
        };

        for (int i = 0; i < RoomCount; i++)
        {
            SpriteRenderer renderer = FindRenderer($"ARoom{i + 1}BG");
            if (renderer != null)
                renderer.sprite = roomSprites[i];
        }

        SpriteRenderer controlRoomRenderer = FindRenderer(ControlRoomObjectName);
        if (controlRoomRenderer != null)
            controlRoomRenderer.sprite = previewMap.controlRoom;
    }

    private SpriteRenderer FindRenderer(string objectName)
    {
        GameObject obj = GameObject.Find(objectName);
        return obj != null ? obj.GetComponent<SpriteRenderer>() : null;
    }
}
