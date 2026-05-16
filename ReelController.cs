using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ReelController — Drives the visual spinning animation for a single reel.
///
/// Architecture:
///   • Each reel has a vertical strip of symbol Images parented under a RectTransform.
///   • Spinning is simulated by continuously scrolling the strip downward.
///   • On stop, the strip snaps to the pre-determined target symbol using
///     a smooth deceleration curve (easing out).
///
/// The reel does NOT choose its own outcome — that responsibility belongs to
/// SlotMachineManager → RNGService (separation of concerns).
/// </summary>
public class ReelController : MonoBehaviour
{
    // ──────────────────────────────────────────────────────────────
    // Constants
    // ──────────────────────────────────────────────────────────────

    /// <summary>Duration of the deceleration phase in seconds (used by manager for timing).</summary>
    public const float DECELERATION_DURATION = 0.6f;

    private const float SYMBOL_HEIGHT    = 150f;   // px — must match prefab layout
    private const float SPIN_SPEED       = 2400f;  // px/sec at full spin
    private const float MIN_SPIN_ROTATIONS = 2;    // minimum full loops before stopping

    // ──────────────────────────────────────────────────────────────
    // Inspector References
    // ──────────────────────────────────────────────────────────────

    [Header("Reel Layout")]
    [Tooltip("The scrollable container that holds repeated symbol images")]
    [SerializeField] private RectTransform symbolStrip;

    [Tooltip("Array of Image components on the strip — order matches SymbolType enum")]
    [SerializeField] private Image[] symbolImages;

    [Tooltip("Sprites indexed by SymbolType (0=Seven, 1=Cherry, 2=Bell, 3=Bar, 4=Wild)")]
    [SerializeField] private Sprite[] symbolSprites;

    [Header("Visual Feedback")]
    [SerializeField] private Image reelHighlight;  // glows on win

    // ──────────────────────────────────────────────────────────────
    // Private State
    // ──────────────────────────────────────────────────────────────

    private int   _targetSymbolIndex;
    private bool  _isSpinning;
    private float _currentScrollY;

    // Total height of the symbol strip (loopable)
    private float StripHeight => symbolImages.Length * SYMBOL_HEIGHT;

    // ──────────────────────────────────────────────────────────────
    // Public API
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Begins the spin animation for this reel.
    /// </summary>
    /// <param name="targetSymbol">Final symbol index to land on.</param>
    /// <param name="startDelay">Seconds before this reel starts moving.</param>
    /// <param name="stopDelay">Seconds before this reel begins decelerating.</param>
    public void StartSpin(int targetSymbol, float startDelay, float stopDelay)
    {
        _targetSymbolIndex = targetSymbol;
        StartCoroutine(SpinRoutine(startDelay, stopDelay));
    }

    /// <summary>Activates the win-highlight glow on this reel.</summary>
    public void ShowWinHighlight(bool active)
    {
        if (reelHighlight != null)
            reelHighlight.enabled = active;
    }

    // ──────────────────────────────────────────────────────────────
    // Spin Coroutine
    // ──────────────────────────────────────────────────────────────

    private IEnumerator SpinRoutine(float startDelay, float stopDelay)
    {
        // ── Phase 0: Wait before starting ──
        yield return new WaitForSeconds(startDelay);

        _isSpinning = true;

        // ── Phase 1: Acceleration (first 0.3 seconds) ──
        float accelTime = 0f;
        float accelDuration = 0.3f;
        while (accelTime < accelDuration)
        {
            float t = accelTime / accelDuration;
            float speed = Mathf.Lerp(0, SPIN_SPEED, EaseInQuad(t));
            ScrollStrip(speed * Time.deltaTime);
            accelTime += Time.deltaTime;
            yield return null;
        }

        // ── Phase 2: Full speed spin ──
        float spinTime = 0f;
        float spinDuration = stopDelay - startDelay - accelDuration;
        spinDuration = Mathf.Max(spinDuration, 0.2f); // guarantee at least a moment at full speed

        while (spinTime < spinDuration)
        {
            ScrollStrip(SPIN_SPEED * Time.deltaTime);
            spinTime += Time.deltaTime;
            yield return null;
        }

        // ── Phase 3: Deceleration → snap to target ──
        float targetY = CalculateTargetScrollY(_targetSymbolIndex);

        // Ensure we overshoot by at least one full loop so the snap looks like it
        // "comes from above" rather than jumping backwards
        float currentNorm = _currentScrollY % StripHeight;
        float neededY     = targetY;

        if (neededY <= currentNorm)
            neededY += StripHeight; // add one full loop

        float startY   = _currentScrollY;
        float endY     = _currentScrollY - currentNorm + neededY;

        float decelTime = 0f;
        while (decelTime < DECELERATION_DURATION)
        {
            float t = decelTime / DECELERATION_DURATION;
            float newY = Mathf.Lerp(startY, endY, EaseOutQuart(t));
            SetStripY(newY);
            decelTime += Time.deltaTime;
            yield return null;
        }

        // Hard snap to final position
        SetStripY(endY);
        _isSpinning = false;
    }

    // ──────────────────────────────────────────────────────────────
    // Strip Scrolling Helpers
    // ──────────────────────────────────────────────────────────────

    private void ScrollStrip(float delta)
    {
        _currentScrollY += delta;
        SetStripY(_currentScrollY);
    }

    private void SetStripY(float y)
    {
        _currentScrollY = y;
        // Wrap within strip bounds to avoid float overflow on long sessions
        float wrapped = y % StripHeight;
        symbolStrip.anchoredPosition = new Vector2(
            symbolStrip.anchoredPosition.x,
            -wrapped
        );
    }

    /// <summary>
    /// Maps a symbol index to the Y position on the strip where that symbol
    /// sits centred in the reel window.
    /// </summary>
    private float CalculateTargetScrollY(int symbolIndex)
    {
        // Symbol 0 is at top of strip; each symbol occupies SYMBOL_HEIGHT px.
        return symbolIndex * SYMBOL_HEIGHT;
    }

    // ──────────────────────────────────────────────────────────────
    // Easing Functions
    // ──────────────────────────────────────────────────────────────

    private static float EaseInQuad(float t)  => t * t;
    private static float EaseOutQuart(float t) => 1f - Mathf.Pow(1f - t, 4f);

    // ──────────────────────────────────────────────────────────────
    // Initialisation
    // ──────────────────────────────────────────────────────────────

    private void Awake()
    {
        // Populate the strip with sprites in correct order
        for (int i = 0; i < symbolImages.Length && i < symbolSprites.Length; i++)
        {
            symbolImages[i].sprite = symbolSprites[i % symbolSprites.Length];
        }

        if (reelHighlight != null)
            reelHighlight.enabled = false;
    }
}
