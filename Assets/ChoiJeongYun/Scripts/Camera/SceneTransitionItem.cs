using UnityEngine;

public class SceneTransitionItem : MonoBehaviour, IPhotographable
{
    [SerializeField] private RoomType targetRoomType;

    public void OnPhotographed(Texture2D snapshot)
    {
        PhotoTransitionEffect.Instance.PlayTransition(snapshot, targetRoomType);
        Destroy(gameObject);
    }
}
