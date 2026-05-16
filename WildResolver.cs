// Bonus feature — Wild symbol substitution resolver
/// <summary>
/// WildResolver — Handles WILD symbol substitution logic.
///
/// A WILD symbol can substitute for any other symbol.
/// If all three reels are WILD, it pays out as the Wild win multiplier.
/// If two are WILD and one is a regular symbol, WILDs become that symbol.
/// </summary>
public static class WildResolver
{
    private static readonly int WildIndex = (int)SymbolType.Wild;

    /// <summary>
    /// Returns a copy of the symbols array with WILDs replaced by the
    /// most common non-wild symbol. If all are wild, they remain wild.
    /// </summary>
    public static int[] Resolve(int[] symbols)
    {
        int[] resolved = (int[])symbols.Clone();

        // Count non-wild symbols and find the majority symbol
        int nonWildSymbol = -1;
        int nonWildCount  = 0;

        foreach (int s in symbols)
        {
            if (s != WildIndex)
            {
                nonWildCount++;
                nonWildSymbol = s;
            }
        }

        // No non-wild symbols → all are wild, no substitution needed
        if (nonWildCount == 0) return resolved;

        // If there is a mix, wilds become the non-wild symbol
        // (only meaningful if there's a single non-wild value — for 3-reel that's enough)
        for (int i = 0; i < resolved.Length; i++)
        {
            if (resolved[i] == WildIndex)
                resolved[i] = nonWildSymbol;
        }

        return resolved;
    }
}
