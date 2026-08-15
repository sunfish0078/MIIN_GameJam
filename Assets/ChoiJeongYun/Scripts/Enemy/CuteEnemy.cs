using UnityEngine;

namespace ChoiJeongYun.Scripts.Enemy
{
    // "miin" 치트(DevMode.CowardMode)가 켜져 있으면 이 몬스터의 평소 이미지/점프스케어 이미지를
    // 여기서 지정한 귀여운 버전으로 교체. 몬스터 프리팹마다 붙여서 각자 다른 이미지를 지정하면 됨.
    // 사운드는 전부 같은 걸 쓰기로 해서 여기 두지 않고 SoundManager.PlayMonsterSFX에서 공통 처리함.
    public class CuteEnemy : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer targetRenderer;
        [SerializeField] private Sprite cuteImage;
        [SerializeField] private Sprite cuteJS;

        public Sprite JumpscareOverride => DevMode.CowardMode ? cuteJS : null;

        private void Start()
        {
            if (DevMode.CowardMode && cuteImage != null && targetRenderer != null)
                targetRenderer.sprite = cuteImage;
        }
    }
}
