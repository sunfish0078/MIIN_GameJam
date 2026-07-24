using UnityEngine;

public interface IPhotographable
{
    bool IsMouseNearCenter(Vector3 worldPoint);
    
    void OnPhotographed(Texture2D snapshot);
}
