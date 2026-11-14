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
        public const string StartRun = "startrun"; // hôte → tous : run, seed
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

        private static readonly JsonSerializerOptions Opt = new JsonSerializerOptions
        {
            IncludeFields = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        };

        public string Serialize() => JsonSerializer.Serialize(this, Opt);
        public static Msg Parse(string line) => JsonSerializer.Deserialize<Msg>(line, Opt);
    }
}
