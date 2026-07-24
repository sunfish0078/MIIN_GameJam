using UnityEngine;

public class SceneTransitionItem : MonoBehaviour, IPhotographable
{
    [SerializeField] private string targetSceneName;
    [SerializeField] private float centerRadius = 0.5f;

    public bool IsMouseNearCenter(Vector3 worldPoint)
    {
        return Vector2.Distance(transform.position, worldPoint) <= centerRadius;
    }

    public void OnPhotographed(Texture2D snapshot)
    {
        PhotoTransitionEffect.Instance.PlayTransition(snapshot, targetSceneName);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, centerRadius);
    }
}
