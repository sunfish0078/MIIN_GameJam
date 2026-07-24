using UnityEngine;

namespace ChoiJeongYun.Scripts.Feedback
{
    // 메인 몬스터가 죽을 때 살점이 터지는 듯한 파티클 연출.
    // 별도 아트 애셋 없이 코드로 파티클 시스템을 직접 구성함.
    public class DeathBurstFeedback : AbstractFeedback
    {
        [SerializeField] private SpriteRenderer targetRenderer;

        [SerializeField] private int particleCount = 24;
        [SerializeField] private float burstSpeed = 1f;
        [SerializeField] private float particleLifetime = 1.5f;
        [SerializeField] private float particleSize = 0.06f;
        [SerializeField] private Color particleColor = new Color(0.55f, 0.05f, 0.05f);

        private ParticleSystem burstParticles;

        private void Awake()
        {
            burstParticles = gameObject.AddComponent<ParticleSystem>();

            ParticleSystem.MainModule main = burstParticles.main;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = particleLifetime;
            main.startSpeed = burstSpeed;
            main.startSize = particleSize;
            main.startColor = particleColor;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            ParticleSystem.EmissionModule emission = burstParticles.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)particleCount) });

            ParticleSystem.ShapeModule shape = burstParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.05f;

            // 갑자기 팝 하고 없어지는 대신, 서서히 투명해지면서 사라지게
            ParticleSystem.ColorOverLifetimeModule colorOverLifetime = burstParticles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient fadeGradient = new Gradient();
            fadeGradient.SetKeys(
                new[] { new GradientColorKey(particleColor, 0f), new GradientColorKey(particleColor, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.6f), new GradientAlphaKey(0f, 1f) });
            colorOverLifetime.color = fadeGradient;

            ParticleSystemRenderer psRenderer = burstParticles.GetComponent<ParticleSystemRenderer>();
            psRenderer.material = new Material(Shader.Find("Sprites/Default"));

            if (targetRenderer != null)
            {
                psRenderer.sortingLayerID = targetRenderer.sortingLayerID;
                psRenderer.sortingOrder = targetRenderer.sortingOrder + 1;
            }
        }

        public override void CreateFeedback()
        {
            if (targetRenderer != null)
                targetRenderer.enabled = false;

            burstParticles.Play();
        }

        public override void FinishFeedback()
        {
            burstParticles.Stop();
        }
    }
}
