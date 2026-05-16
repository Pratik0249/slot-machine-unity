using UnityEngine;

/// <summary>
/// RNGService — Stateless utility that generates random slot outcomes.
///
/// Uses Unity's Random (seeded per session via Time.time at boot) with a
/// weighted probability table so the house edge is configurable without
/// changing win-check logic. All weights are inspectable and auditable here.
///
/// Design notes:
///   • Static class → no MonoBehaviour overhead, callable from anywhere.
///   • Weights are additive; increase Cherry weight to raise hit frequency.
///   • Wild symbol has deliberately low weight — it's a bonus, not a crutch.
/// </summary>
public static class RNGService
{
    // ──────────────────────────────────────────────────────────────
    // Symbol Probability Weights
    // Higher weight = more likely to appear on a single reel stop.
    // Total = 100 for easy mental math (each unit ≈ 1% of single-reel hits).
    // ──────────────────────────────────────────────────────────────
    private static readonly int[] SymbolWeights =
    {
        5,   // SymbolType.Seven  — rare, jackpot value
        40,  // SymbolType.Cherry — very common, small win
        25,  // SymbolType.Bell   — common, mid win
        25,  // SymbolType.Bar    — common, mid win
        5,   // SymbolType.Wild   — rare bonus symbol
    };

    private static int TotalWeight
    {
        get
        {
            int total = 0;
            foreach (int w in SymbolWeights) total += w;
            return total;
        }
    }

    // ──────────────────────────────────────────────────────────────
    // Public API
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Generates one independent random symbol index per reel.
    /// Each reel is resolved independently — no correlation between reels.
    /// </summary>
    /// <param name="reelCount">Number of reels (typically 3).</param>
    /// <returns>Array of symbol indices, one per reel.</returns>
    public static int[] GenerateOutcome(int reelCount)
    {
        int[] outcome = new int[reelCount];
        for (int i = 0; i < reelCount; i++)
        {
            outcome[i] = PickWeightedSymbol();
        }
        return outcome;
    }

    // ──────────────────────────────────────────────────────────────
    // Private Helpers
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Weighted random selection using a cumulative distribution approach.
    /// O(n) where n = number of symbol types — fine for 5 symbols.
    /// </summary>
    private static int PickWeightedSymbol()
    {
        int roll = Random.Range(0, TotalWeight);
        int cumulative = 0;

        for (int i = 0; i < SymbolWeights.Length; i++)
        {
            cumulative += SymbolWeights[i];
            if (roll < cumulative)
                return i;
        }

        // Fallback (should never reach here if weights sum correctly)
        return SymbolWeights.Length - 1;
    }
}
