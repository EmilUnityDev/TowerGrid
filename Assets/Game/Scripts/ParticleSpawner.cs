using Coffee.UIExtensions;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleSpawner : MonoBehaviour {
    [Header("Настройки полета")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float acceleration = 15f;

    [Header("Настройки дуги")]
    [SerializeField] private float curveForce = 10f; 
    [Range(0f, 1f)]
    [SerializeField] private float curveRandomness = 0.5f; 

    [SerializeField] private ParticleSystem particleSys;
    private ParticleSystem.Particle[] particles;
    private Transform targetDestination;
    private bool isAttracting = false;

    private void Start() {
        if (particleSys == null) particleSys = GetComponent<ParticleSystem>();

        
        if (ParticleAttractor.TargetTransform != null) {
            targetDestination = ParticleAttractor.TargetTransform;
        } else {
            Debug.LogWarning($"[ParticleSpawner] На сцене не найден ParticleAttractor для объекта {gameObject.name}!");
        }
    }

    public void SpawnAndAttract() {
        if (targetDestination == null) return;

        transform.SetParent(null);

        if (particles == null || particles.Length < particleSys.main.maxParticles) {
            particles = new ParticleSystem.Particle[particleSys.main.maxParticles];
        }

        isAttracting = true;
        particleSys.Play();

        var main = particleSys.main;
        Destroy(gameObject, main.duration + main.startLifetime.constantMax);
    }

    private void LateUpdate() {
        if (!isAttracting || targetDestination == null || !particleSys.isPlaying) return;

        int aliveCount = particleSys.GetParticles(particles);
        Vector3 targetPos = targetDestination.position;

        if (particleSys.main.simulationSpace == ParticleSystemSimulationSpace.Local) {
            targetPos = transform.InverseTransformPoint(targetPos);
        }

        for (int i = 0; i < aliveCount; i++) {
            Vector3 particlePos = particles[i].position;
            float distance = Vector3.Distance(targetPos, particlePos);

            
            Vector3 directionToTarget = (targetPos - particlePos).normalized;

            
            var prevRandomState = Random.state;
            Random.InitState(unchecked((int)particles[i].randomSeed));

           
            Vector3 perpendicular = Vector3.Cross(directionToTarget, Vector3.forward).normalized;
            if (Random.value > 0.5f) perpendicular = -perpendicular;

            
            float individualCurveForce = curveForce * Random.Range(1f - curveRandomness, 1f + curveRandomness);

            
            Random.state = prevRandomState;
            float curveFade = Mathf.Clamp01(distance / 2f);
            Vector3 curveVelocity = perpendicular * individualCurveForce * curveFade;

            
            float currentSpeed = speed + (acceleration / (distance + 0.1f));
            Vector3 targetVelocity = directionToTarget * currentSpeed;

           
            Vector3 finalVelocity = targetVelocity + curveVelocity;
            particles[i].velocity = Vector3.Lerp(particles[i].velocity, finalVelocity, Time.deltaTime * 10f);

         
            if (distance < 0.3f) {
                particles[i].remainingLifetime = 0f;
            }
        }

        particleSys.SetParticles(particles, aliveCount);
    }
}