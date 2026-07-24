using UnityEngine;

[CreateAssetMenu(fileName = "Room Map", menuName = "SO/Room Map")]
public class RoomMapSO : ScriptableObject
{
    public RoomType roomType;

    [Header("CCTV 방")]
    public Sprite room1;
    public Sprite room2;
    public Sprite room3;
    public Sprite room4;
    public Sprite room5;
    public Sprite room6;
    public Sprite room7;

    [Header("관리실")]
    public Sprite controlRoom;

    [Header("적")]
    public GameObject monsterPrefab;
}
