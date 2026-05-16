/// <summary>
/// SymbolType — Canonical enum for all slot symbols.
/// Integer values map directly to sprite atlas indices (0-based).
/// </summary>
public enum SymbolType
{
    Seven  = 0,   // Highest value — jackpot symbol
    Cherry = 1,   // Common low-value symbol
    Bell   = 2,   // Mid-value symbol
    Bar    = 3,   // Mid-value symbol
    Wild   = 4,   // Bonus: substitutes for any symbol
}

/// <summary>
/// PayoutTable — Static data class defining credit multipliers for each winning symbol.
/// All payouts are expressed as multiples of the current bet.
/// 
/// Design rationale: keeping payout data in one place makes balancing trivial
/// without hunting through logic code.
/// </summary>
public static class PayoutTable
{
    /// <summary>
    /// Returns the credit payout for a winning spin.
    /// </summary>
    /// <param name="symbolIndex">The matched SymbolType cast to int.</param>
    /// <param name="bet">Credits wagered this spin.</param>
    /// <returns>Total credits to award (includes return of original bet).</returns>
    public static int GetPayout(int symbolIndex, int bet)
    {
        int multiplier = GetMultiplier((SymbolType)symbolIndex);
        return bet * multiplier;
    }

    /// <summary>
    /// Per-symbol payout multipliers.
    /// Seven × 50 is intentionally jackpot-tier; Cherry × 3 keeps small wins frequent.
    /// </summary>
    private static int GetMultiplier(SymbolType symbol)
    {
        return symbol switch
        {
            SymbolType.Seven  => 50,   // JACKPOT
            SymbolType.Bar    => 10,   // High
            SymbolType.Bell   => 5,    // Medium
            SymbolType.Cherry => 3,    // Low (most common)
            SymbolType.Wild   => 20,   // Bonus symbol — rare, high reward
            _                 => 1,    // Fallback: return bet only
        };
    }

    /// <summary>Returns a display string of all current paylines for the UI.</summary>
    public static string GetPaytableDescription()
    {
        return
            "🎰 PAYTABLE\n" +
            "━━━━━━━━━━━━━━━━━━━━\n" +
            "7️⃣  7️⃣  7️⃣   ×  50  — JACKPOT!\n" +
            "🃏  🃏  🃏   ×  20  — WILD WIN\n" +
            "BAR BAR BAR  ×  10\n" +
            "🔔  🔔  🔔   ×   5\n" +
            "🍒  🍒  🍒   ×   3\n" +
            "━━━━━━━━━━━━━━━━━━━━\n" +
            "WILD substitutes any symbol";
    }
}
