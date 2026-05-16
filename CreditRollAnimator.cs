using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// CreditRollAnimator — Animates the credit counter rolling up/down to a new value.
/// Adds satisfying visual feedback when wins are awarded or bets are placed.
///
/// Attach to the credits TextMeshProUGUI GameObject and call RollTo() from UIController.
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class CreditRollAnimator : MonoBehaviour
{
    [SerializeField] [Range(0.3f, 3f)]
    private float rollDuration = 0.8f;  // how long the counting animation takes

    [SerializeField] private string prefix = "CREDITS: ";

    private TextMeshProUGUI _label;
    private int             _displayedValue;
    private Coroutine       _activeRoll;

    private void Awake()
    {
        _label = GetComponent<TextMeshProUGUI>();
    }

    /// <summary>
    /// Animates the displayed number from its current value to <paramref name="targetValue"/>.
    /// Cancels any in-progress animation before starting a new one.
    /// </summary>
    public void RollTo(int targetValue)
    {
        if (_activeRoll != null)
            StopCoroutine(_activeRoll);

        _activeRoll = StartCoroutine(RollRoutine(_displayedValue, targetValue));
    }

    /// <summary>Instantly sets the displayed value without animation.</summary>
    public void SetImmediate(int value)
    {
        if (_activeRoll != null) StopCoroutine(_activeRoll);
        _displayedValue = value;
        UpdateLabel(value);
    }

    // ──────────────────────────────────────────────────────────────
    // Coroutine
    // ──────────────────────────────────────────────────────────────

    private IEnumerator RollRoutine(int from, int to)
    {
        float elapsed = 0f;

        while (elapsed < rollDuration)
        {
            float t   = elapsed / rollDuration;
            float ease = EaseOutCubic(t);
            int   displayed = Mathf.RoundToInt(Mathf.Lerp(from, to, ease));

            UpdateLabel(displayed);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Guarantee exact final value
        _displayedValue = to;
        UpdateLabel(to);
        _activeRoll = null;
    }

    private void UpdateLabel(int value)
    {
        if (_label != null)
            _label.text = $"{prefix}{value}";
    }

    private static float EaseOutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);
}
