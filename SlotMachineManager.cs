using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// SlotMachineManager — Central controller for all slot machine game logic.
/// Coordinates spinning, RNG, win evaluation, and payout calculation.
/// Follows a single-responsibility design: this class orchestrates, delegates details to sub-systems.
/// </summary>
public class SlotMachineManager : MonoBehaviour
{
    // ──────────────────────────────────────────────────────────────
    // Inspector References
    // ──────────────────────────────────────────────────────────────

    [Header("Reel Controllers")]
    [Tooltip("Array of the three reel controllers, ordered Left → Centre → Right")]
    [SerializeField] private ReelController[] reels = new ReelController[3];

    [Header("Game Settings")]
    [SerializeField] private int startingCredits = 100;
    [SerializeField] private int betPerSpin     = 10;

    [Header("Events — hook UI listeners here")]
    public UnityEvent<int>    onCreditsChanged;   // fires after every credit update
    public UnityEvent<string> onSpinResult;        // "WIN", "LOSE", "JACKPOT", "BONUS"
    public UnityEvent         onSpinStarted;
    public UnityEvent         onAllReelsStopped;

    // ──────────────────────────────────────────────────────────────
    // Private State
    // ──────────────────────────────────────────────────────────────

    private int  _credits;
    private bool _isSpinning;

    /// <summary>Current player credit balance (read-only from outside).</summary>
    public int Credits => _credits;

    // ──────────────────────────────────────────────────────────────
    // Unity Lifecycle
    // ──────────────────────────────────────────────────────────────

    private void Awake()
    {
        _credits = startingCredits;
    }

    private void Start()
    {
        onCreditsChanged?.Invoke(_credits);
    }

    // ──────────────────────────────────────────────────────────────
    // Public API
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Called by the SPIN button. Deducts bet, triggers reel animations,
    /// then evaluates the outcome once all reels have settled.
    /// </summary>
    public void RequestSpin()
    {
        if (_isSpinning)
        {
            Debug.Log("[SlotMachine] Spin already in progress — ignoring request.");
            return;
        }

        if (_credits < betPerSpin)
        {
            Debug.Log("[SlotMachine] Not enough credits to spin.");
            onSpinResult?.Invoke("NO_CREDITS");
            return;
        }

        // Deduct bet up-front (house always gets paid first)
        _credits -= betPerSpin;
        onCreditsChanged?.Invoke(_credits);

        _isSpinning = true;
        onSpinStarted?.Invoke();

        StartCoroutine(SpinSequence());
    }

    // ──────────────────────────────────────────────────────────────
    // Spin Sequence Coroutine
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Orchestrates the full spin:
    ///   1. Pre-generate RNG outcomes for all reels.
    ///   2. Stagger reel starts for visual drama.
    ///   3. Wait for all reels to report completion.
    ///   4. Evaluate win condition and award payout.
    /// </summary>
    private IEnumerator SpinSequence()
    {
        // --- 1. RNG: decide final symbols before animation starts ---
        // This ensures fairness — the outcome is determined at spin time,
        // not influenced by animation timing.
        int[] finalSymbols = RNGService.GenerateOutcome(reels.Length);

        // --- 2. Kick off reels with staggered delays (left → mid → right) ---
        for (int i = 0; i < reels.Length; i++)
        {
            float startDelay = i * 0.12f;  // 120 ms stagger
            float stopDelay  = 1.0f + i * 0.45f; // each reel stops progressively later
            reels[i].StartSpin(finalSymbols[i], startDelay, stopDelay);
        }

        // --- 3. Wait until the slowest reel finishes ---
        float totalDuration = 1.0f + (reels.Length - 1) * 0.45f + ReelController.DECELERATION_DURATION;
        yield return new WaitForSeconds(totalDuration + 0.1f); // small buffer

        // --- 4. Evaluate outcome ---
        onAllReelsStopped?.Invoke();
        EvaluateOutcome(finalSymbols);

        _isSpinning = false;
    }

    // ──────────────────────────────────────────────────────────────
    // Win Evaluation
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Checks the landed symbols against the payout table and awards credits.
    /// Win condition: all three reels show the same symbol index.
    /// Bonus: if a WILD symbol is present it substitutes for any symbol.
    /// </summary>
    private void EvaluateOutcome(int[] symbols)
    {
        // Resolve WILDs: replace WILD (index 4) with the most common non-wild symbol
        int[] resolved = WildResolver.Resolve(symbols);

        bool allMatch = resolved[0] == resolved[1] && resolved[1] == resolved[2];

        if (!allMatch)
        {
            Debug.Log($"[SlotMachine] No match → {SymbolName(symbols[0])}, {SymbolName(symbols[1])}, {SymbolName(symbols[2])}");
            onSpinResult?.Invoke("LOSE");
            return;
        }

        // Calculate payout based on matched symbol
        int matchedSymbol = resolved[0];
        int payout        = PayoutTable.GetPayout(matchedSymbol, betPerSpin);

        _credits += payout;
        onCreditsChanged?.Invoke(_credits);

        string resultType = matchedSymbol == (int)SymbolType.Seven ? "JACKPOT" : "WIN";
        Debug.Log($"[SlotMachine] {resultType}! Symbol: {SymbolName(matchedSymbol)}, Payout: {payout}");

        onSpinResult?.Invoke(resultType);
    }

    // ──────────────────────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────────────────────

    private string SymbolName(int index) =>
        System.Enum.IsDefined(typeof(SymbolType), index)
            ? ((SymbolType)index).ToString()
            : $"Symbol[{index}]";
}
