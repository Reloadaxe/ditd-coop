using DiceLoader;

namespace DiceMultiplayer.Sync
{
    /// <summary>
    /// Empreinte de ce qui doit être identique chez tous : les ennemis (index de combat et PV) et le tour.
    /// Le reste (héros, deck, main, énergie, reliques) appartient à chaque joueur et diffère normalement.
    /// </summary>
    public static class StateHash
    {
        public static (long hash, string detail) Compute()
        {
            var s = $"turn={Game.Turn};enemies={Combat.HashPart()}";
            return (Fnv(s), s);
        }

        private static long Fnv(string s)
        {
            unchecked
            {
                long h = (long)14695981039346656037UL;
                foreach (char c in s) { h ^= c; h *= 1099511628211L; }
                return h;
            }
        }
    }
}
