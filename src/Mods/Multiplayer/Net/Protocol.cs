using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DiceMultiplayer.Net
{
    /// <summary>Types de messages. Le protocole est du JSON, un message par ligne, sur TCP.</summary>
    public static class T
    {
        public const string Hello = "hello";       // client → hôte : name, ver
        public const string Welcome = "welcome";   // hôte → client : id, players
        public const string Reject = "reject";     // hôte → client : text
        public const string Players = "players";   // hôte → tous : players
        public const string Cursor = "cursor";     // tous : id, x, y (0..1), scene
        public const string Chat = "chat";         // tous : id, text
        public const string Ping = "ping";         // keep-alive
        public const string StartRun = "startrun"; // hôte → tous : run (équipe fusionnée), picks, seed
        public const string Pick = "pick";         // tous : run (choix de personnage), flag = prêt
        public const string HeroState = "hero";    // tous : état de son héros (h = PV, sx = PV max, sy = bloc, flag = K.O.)
        public const string HeroAnim = "hanim";   // tous : animation de mon héros (h = état, x = temps normalisé)
        public const string BoardPlace = "bplace"; // tous : dé posé (uid, sx, sy, text = nom, btn = valeur)
        public const string BoardRemove = "bremove"; // tous : dé retiré (uid)
        public const string BoardClear = "bclear"; // tous : plateau du joueur vidé (fin de tour)
        public const string EnemyDamage = "edmg";  // tous : dégâts à l'ennemi d'index uid (btn = dégâts, flag = ignore le bloc, x>0 = coup fatal)
        public const string EnemyMod = "emod";     // tous : statut posé sur l ennemi d index uid (btn = type, sx = valeur)
        public const string ExecDone = "execdone"; // tous : mes dés ont fini de s exécuter (uid = étage, turn = tour)
        public const string Spell = "spell";       // hôte → tous : sort d ennemi posé sur une case (sx,sy = case, btn = rang, uid = direction)
        public const string SlotVals = "slotvals"; // tous : valeur finale de mes cases (text = "x,y=valeur;…")
        public const string EnemySync = "esync";   // hôte → tous : PV de référence des ennemis (text = "idx:pv:bloc;…")
        public const string Preview = "preview";   // tous : dégâts prévus par mon plateau sur chaque ennemi (text = "idx:dégâts;…")
        public const string OnMap = "onmap";       // tous : flag = de retour sur la carte et prêt à continuer
        public const string Vote = "vote";         // tous : vote pour une case de la carte (path, comp, btn, x, y)
        public const string MapGo = "mapgo";       // hôte → tous : case retenue (rejouée chez tous, hôte compris)
        public const string OpenSel = "opensel";   // hôte → clients : ouvrir la sélection de personnage
        public const string CloseSel = "closesel"; // hôte → clients : retour à l'écran titre
        public const string Click = "click";       // action : path, comp, method, btn, x, y
        public const string PlaceDice = "place";   // action : uid, sx, sy
        public const string ReturnDice = "return"; // action : uid
        public const string Ready = "ready";       // client ↔ hôte : id, turn, flag
        public const string EndTurn = "endturn";   // hôte → tous : turn
        public const string Hash = "hash";         // client → hôte : id, turn, h, text
        public const string Desync = "desync";     // hôte → tous : text
    }

    public sealed class PlayerInfo
    {
        public int Id;
        public string Name;
        public string Color;
        public bool Ready;
    }

    public sealed class RunData
    {
        public int Character;
        public int Skin;
        public int Companion;
        public int[] Relics;
        public int Difficulty;
        public bool Challenges;
    }

    /// <summary>Choix de personnage d'un joueur dans le lobby.</summary>
    public sealed class PlayerPick
    {
        public int Id;
        public RunData Run;
    }

    /// <summary>Message « plat » : seuls les champs utiles à chaque type sont remplis.</summary>
    public sealed class Msg
    {
        public string t;
        public int id;
        public int seq;
        public string name;
        public string ver;
        public string text;
        public string path;
        public string comp;
        public string method;
        public int btn;
        public float x, y;
        public int uid;
        public int sx, sy;
        public int turn;
        public long h;
        public int seed;
        public bool flag;
        public string scene;
        public RunData run;
        public List<PlayerInfo> players;
        public List<PlayerPick> picks;
        /// <summary>Instantané visuel d'un dé (sprites, couleurs, textes, types) : voir Board.Snapshot.</summary>
        public string vis;

        private static readonly JsonSerializerOptions Opt = new JsonSerializerOptions
        {
            IncludeFields = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        };

        public string Serialize() => JsonSerializer.Serialize(this, Opt);
        public static Msg Parse(string line) => JsonSerializer.Deserialize<Msg>(line, Opt);
    }
}
