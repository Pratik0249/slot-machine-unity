using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// JackpotEffect — Orchestrates the full visual celebration for a jackpot win.
/// Plays a screen flash, enables a particle system, and optionally shakes
/// the slot machine frame for maximum dramatic impact.
///
/// Called by UIController.OnSpinResult("JACKPOT").
/// </summary>
public class JackpotEffect : MonoBehaviour
{
    [Header("Screen Flash")]
    [SerializeField] private Image flashOverlay;           // full-screen white/gold image
    [SerializeField] private float flashInDuration  = 0.1f;
    [SerializeField] private float flashHoldDuration = 0.05f;
    [SerializeField] private float flashOutDuration  = 0.4f;

    [Header("Particle System")]
    [SerializeField] private ParticleSystem confettiSystem;

    [Header("Frame Shake")]
    [SerializeField] private RectTransform machineFrame;
    [SerializeField] private float shakeDuration  = 0.6f;
    [SerializeField] private float shakeMagnitude = 18f;

    // ──────────────────────────────────────────────────────────────
    // Public API
    // ──────────────────────────────────────────────────────────────

    public void PlayJackpot()
    {
        StartCoroutine(JackpotRoutine());
    }

    public void StopJackpot()
    {
        StopAllCoroutines();

        if (confettiSystem != null) confettiSystem.Stop();
        if (flashOverlay   != null) flashOverlay.color = Color.clear;
    }

    // ──────────────────────────────────────────────────────────────
    // Coroutine
    // ──────────────────────────────────────────────────────────────

    private IEnumerator JackpotRoutine()
    {
        // Run flash and shake concurrently
        Coroutine shake = StartCoroutine(ShakeFrame());
        yield return StartCoroutine(FlashScreen());

        // Start confetti after the initial flash
        if (confettiSystem != null)
            confettiSystem.Play();

        yield return shake;
    }

    private IEnumerator FlashScreen()
    {
        if (flashOverlay == null) yield break;

        Color goldFlash = new Color(1f, 0.85f, 0f, 0.85f);

        // Flash in
        yield return FadeOverlay(Color.clear, goldFlash, flashInDuration);
        yield return new WaitForSeconds(flashHoldDuration);
        // Flash out
        yield return FadeOverlay(goldFlash, Color.clear, flashOutDuration);
    }

    private IEnumerator FadeOverlay(Color from, Color to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            flashOverlay.color = Color.Lerp(from, to, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        flashOverlay.color = to;
    }

    private IEnumerator ShakeFrame()
    {
        if (machineFrame == null) yield break;

        Vector2 originalPos = machineFrame.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;
            machineFrame.anchoredPosition = originalPos + new Vector2(x, y);
            elapsed += Time.deltaTime;
            yield return null;
        }

        machineFrame.anchoredPosition = originalPos;
    }
}
