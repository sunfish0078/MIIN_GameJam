using Unity.Cinemachine;
using UnityEngine;

[System.Serializable]
public struct CCTVCameraEntry
{

    public CinemachineCamera camera;
    public string roomName;
}
public class CCTVSceneCameras : MonoBehaviour
{
    public CCTVCameraEntry[] cctvCameras;
}
