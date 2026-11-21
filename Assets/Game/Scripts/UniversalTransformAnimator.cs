using System.Collections;
using UnityEngine;

public class UniversalTransformAnimator : MonoBehaviour {
    private Transform _target;

    [Header("Scale")]
    [SerializeField] private bool useScale = true;
    [SerializeField] private Vector3 scaleFrom = Vector3.one;
    [SerializeField] private Vector3 scaleTo = Vector3.one * 1.2f;
    [SerializeField] private float scaleDuration = 1f;
    [SerializeField] private EasingType scaleEasing = EasingType.SmoothStep;


    [Header("Rotation")]
    [SerializeField] private bool useRotation = true;

    public enum RotationMode {
        Linear,
        Swing
    }

    [SerializeField] private RotationMode rotationMode = RotationMode.Swing;
    [SerializeField] private float rotationDuration = 1f;
    [SerializeField] private EasingType rotationEasing = EasingType.SmoothStep;

    [Header("Linear Rotation")]
    [SerializeField] private float linearRotationAngle = 180f;

    [Header("Swing Rotation")]
    [SerializeField] private float swingAngle = 15f;


    public enum EasingType {
        Linear,
        EaseIn,
        EaseOut,
        EaseInOut,
        SmoothStep,
        EaseOutBack
    }

    private Coroutine _scaleRoutine;
    private Coroutine _rotationRoutine;

    private Vector3 _startScale;
    private Quaternion _startRotation;

    private void Awake() {
        _target = transform.childCount > 0 ? transform.GetChild(0) : transform;

        _startScale = _target.localScale;
        _startRotation = _target.localRotation;
    }

    private void OnEnable() {
        if (useScale)
            _scaleRoutine = StartCoroutine(ScaleLoop());

        if (useRotation) {
            _rotationRoutine = rotationMode == RotationMode.Linear
                ? StartCoroutine(LinearRotation())
                : StartCoroutine(SwingRotation());
        }
    }

    private void OnDisable() {
        if (_scaleRoutine != null)
            StopCoroutine(_scaleRoutine);

        if (_rotationRoutine != null)
            StopCoroutine(_rotationRoutine);

        _target.localScale = _startScale;
        _target.localRotation = _startRotation;
    }

    private IEnumerator ScaleLoop() {
        Vector3 from = Multiply(_startScale, scaleFrom);
        Vector3 to = Multiply(_startScale, scaleTo);

        while (true) {
            yield return ScaleStep(from, to);
            yield return ScaleStep(to, from);
        }
    }

    private IEnumerator ScaleStep(Vector3 from, Vector3 to) {
        float time = 0f;

        while (time < scaleDuration) {
            time += Time.deltaTime;
            float t = ApplyEasing(time / scaleDuration, scaleEasing);

            _target.localScale = Vector3.Lerp(from, to, t);
            yield return null;
        }
    }


    private IEnumerator LinearRotation() {
        while (true) {
            float time = 0f;

            while (time < rotationDuration) {
                time += Time.deltaTime;
                float t = ApplyEasing(time / rotationDuration, rotationEasing);

                _target.localRotation =
                    _startRotation *
                    Quaternion.Euler(0f, 0f, linearRotationAngle * t);

                yield return null;
            }

            // мгновенный сброс
            _target.localRotation = _startRotation;
        }
    }

    private IEnumerator SwingRotation() {
        while (true) {
            yield return SwingStep(0f, +swingAngle);
            yield return SwingStep(+swingAngle, 0f);
            yield return SwingStep(0f, -swingAngle);
            yield return SwingStep(-swingAngle, 0f);
        }
    }

    private IEnumerator SwingStep(float from, float to) {
        float time = 0f;

        while (time < rotationDuration) {
            time += Time.deltaTime;
            float t = ApplyEasing(time / rotationDuration, rotationEasing);

            float angle = Mathf.Lerp(from, to, t);
            _target.localRotation =
                _startRotation *
                Quaternion.Euler(0f, 0f, angle);

            yield return null;
        }
    }


    private static Vector3 Multiply(Vector3 a, Vector3 b) {
        return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
    }

    private float ApplyEasing(float t, EasingType easing) {
        t = Mathf.Clamp01(t);

        switch (easing) {
            case EasingType.Linear:
                return t;

            case EasingType.EaseIn:
                return t * t;

            case EasingType.EaseOut:
                return 1f - Mathf.Pow(1f - t, 2f);

            case EasingType.EaseInOut:
                return t < 0.5f
                    ? 2f * t * t
                    : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;

            case EasingType.SmoothStep:
                return t * t * (3f - 2f * t);

            case EasingType.EaseOutBack:
                const float c1 = 1.70158f;
                const float c3 = c1 + 1f;
                return 1f + c3 * Mathf.Pow(t - 1f, 3f)
                             + c1 * Mathf.Pow(t - 1f, 2f);

            default:
                return t;
        }
    }
}