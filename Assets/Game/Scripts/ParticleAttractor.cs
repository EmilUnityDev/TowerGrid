using UnityEngine;

public class ParticleAttractor : MonoBehaviour {
    // Статический доступ, чтобы любой спавнер мог мгновенно найти точку
    public static Transform TargetTransform { get; private set; }

    private void Awake() {
        // Регистрируем себя как главную цель
        TargetTransform = this.transform;
    }

    private void OnDestroy() {
        if (TargetTransform == this.transform) {
            TargetTransform = null;
        }
    }
}