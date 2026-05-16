using System.Collections;
using UnityEngine;

/// <summary>
/// LeverController — Animates the slot machine's pull-lever.
///
/// The lever is a cosmetic bonus feature:
///   • Pulling it triggers the same spin as the SPIN button.
///   • The lever arm rotates down then springs back up.
///   • Uses a spring-physics approximation (no Rigidbody needed).
///
/// Hook OnLeverPulled to SlotMachineManager.RequestSpin() in the Inspector.
/// </summary>
public class LeverController : MonoBehaviour
{
    [Header("Lever Pivot (the RectTransform to rotate)")]
    [SerializeField] private RectTransform leverArm;

    [Header("Rotation Angles")]
    [SerializeField] private float restAngle      =   0f;
    [SerializeField] private float pulledAngle    = -75f;
    [SerializeField] private float pullDuration   =  0.25f;
    [SerializeField] private float returnDuration =  0.45f;
    [SerializeField] private float overshootAngle =  15f;  // spring overshoot

    [Header("Interaction")]
    [SerializeField] private bool leverEnabled = true;

    private bool _isPulling;

    // ──────────────────────────────────────────────────────────────
    // Public API
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Call from a UI Button's OnClick, or directly via pointer event.
    /// Plays the pull animation and fires OnLeverPulled at the bottom of the stroke.
    /// </summary>
    public void PullLever()
    {
        if (_isPulling || !leverEnabled) return;
        StartCoroutine(LeverRoutine());
    }

    /// <summary>Enables/disables lever interaction (mirrors SPIN button state).</summary>
    public void SetEnabled(bool enabled) => leverEnabled = enabled;

    // ──────────────────────────────────────────────────────────────
    // Events
    // ──────────────────────────────────────────────────────────────

    // Wire this to SlotMachineManager.RequestSpin() in the Inspector
    public UnityEngine.Events.UnityEvent onLeverPulled;

    // ──────────────────────────────────────────────────────────────
    // Coroutine
    // ──────────────────────────────────────────────────────────────

    private IEnumerator LeverRoutine()
    {
        _isPulling = true;

        // ── Phase 1: Pull down ──
        yield return RotateLever(restAngle, pulledAngle, pullDuration, EaseInQuad);

        // ── Fire spin at bottom of stroke ──
        onLeverPulled?.Invoke();

        // ── Phase 2: Spring return with overshoot ──
        float midAngle = restAngle + overshootAngle;
        yield return RotateLever(pulledAngle, midAngle, returnDuration * 0.6f, EaseOutExpo);
        yield return RotateLever(midAngle, restAngle, returnDuration * 0.4f, EaseInOutSine);

        _isPulling = false;
    }

    private IEnumerator RotateLever(float from, float to, float duration, System.Func<float, float> easing)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t     = elapsed / duration;
            float angle = Mathf.Lerp(from, to, easing(t));
            leverArm.localEulerAngles = new Vector3(0f, 0f, angle);
            elapsed += Time.deltaTime;
            yield return null;
        }
        leverArm.localEulerAngles = new Vector3(0f, 0f, to);
    }

    // ──────────────────────────────────────────────────────────────
    // Easing
    // ──────────────────────────────────────────────────────────────

    private static float EaseInQuad(float t)     => t * t;
    private static float EaseOutExpo(float t)    => t >= 1f ? 1f : 1f - Mathf.Pow(2f, -10f * t);
    private static float EaseInOutSine(float t)  => -(Mathf.Cos(Mathf.PI * t) - 1f) / 2f;
}
