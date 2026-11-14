using System.Linq;
using System.Text;
using DiceLoader;

namespace DiceMultiplayer.Sync
{
    /// <summary>Empreinte de l'état de jeu, comparée entre joueurs pour détecter une divergence.</summary>
    public static class StateHash
    {
        public static (long hash, string detail) Compute()
        {
            var sb = new StringBuilder();
            sb.Append("hp=").Append(Game.Health).Append('/').Append(Game.MaxHealth)
              .Append(";blk=").Append(Game.Block)
              .Append(";gold=").Append(Game.Gold)
              .Append(";turn=").Append(Game.Turn)
              .Append(";deck=").Append(Game.Deck.Count)
              .Append(";relics=").Append(string.Join(",", Game.RelicTitles.Select(r => (int)r).OrderBy(x => x)));

            foreach (var e in Game.Enemies.OrderBy(e => Game.Safe(() => Game.Battle.GetBattleIndexOfEntity(e))))
                sb.Append(";e=").Append(Game.Safe(() => (int)e.EnemyName)).Append(':').Append(e.CurrentHealth).Append(':').Append(e.CurrentBlock);

            var dm = Game.Dice;
            if (dm != null && Game.InBattle)
            {
                var hand = Game.Safe(() => dm.GetAllGODicesOnHand());
                if (hand != null)
                    sb.Append(";hand=").Append(string.Join(",", hand.Select(d => Game.Safe(() => d.CurrentActiveDice.uid + ":" + d.CurrentActiveDice.CurrentFaceValue)).OrderBy(x => x)));
            }

            sb.Append(";").Append(Heroes.HashPart());
            var s = sb.ToString();
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
