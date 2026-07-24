using UnityEngine;

public class SceneTransitionItem : MonoBehaviour, IPhotographable
{
    [SerializeField] private RoomType targetRoomType;
    [SerializeField] private float centerRadius = 0.5f;

    public bool IsMouseNearCenter(Vector3 worldPoint)
    {
        return Vector2.Distance(transform.position, worldPoint) <= centerRadius;
    }

    public void OnPhotographed(Texture2D snapshot)
    {
        PhotoTransitionEffect.Instance.PlayTransition(snapshot, targetRoomType);
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, centerRadius);
    }
}
