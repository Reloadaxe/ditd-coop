using System;
using UnityEngine;

namespace DiceLoader
{
    /// <summary>
    /// MonoBehaviour injecté dans IL2CPP via ClassInjector.
    /// Il fait tourner Update/OnGUI pour les mods, détecte les changements d'état du GameManager
    /// et dessine le menu DiceLoader.
    /// </summary>
    public class LoaderBehaviour : MonoBehaviour
    {
        public LoaderBehaviour(IntPtr ptr) : base(ptr) { }

        private GameManager.GameState _lastState = GameManager.GameState.NONE;
        private bool _menuOpen;
        private int _selected = -1; // -1 = liste des mods
        private Vector2 _scroll;
        private Rect _rect = new Rect(40, 40, 520, 600);

        private void Update()
        {
            Scheduler.Tick();
            WindowEnforcer.Tick();

            // Changement d'état global, détecté par polling : plus robuste que de patcher la machine à états générique
            var st = Game.State;
            if (st != _lastState)
            {
                var prev = _lastState;
                _lastState = st;
                DiceEvents.RaiseGameStateChanged(prev, st);
            }

            Loader.ForEachEnabled(m => m.OnUpdate(), nameof(DiceMod.OnUpdate));
        }

        private void OnGUI()
        {
            var ev = Event.current;
            if (ev != null && ev.type == EventType.KeyDown && ev.keyCode == Plugin.MenuKey.Value)
            {
                _menuOpen = !_menuOpen;
                ev.Use();
            }

            Loader.ForEachEnabled(m => m.OnGUI(), nameof(DiceMod.OnGUI));

            if (Plugin.ShowWatermark.Value && !_menuOpen)
                GUI.Label(new Rect(8, Screen.height - 24, 600, 22), $"DiceLoader {Plugin.Version} - {Loader.Mods.Count} mod(s) - {Plugin.MenuKey.Value} : menu");

            if (_menuOpen) DrawMenu();
        }

        private void DrawMenu()
        {
            _rect.height = Math.Min(600f, Screen.height - 80f);
            GUI.Box(_rect, "");
            GUILayout.BeginArea(new Rect(_rect.x + 10, _rect.y + 10, _rect.width - 20, _rect.height - 20));

            UI.BeginRow();
            UI.Header($"DiceLoader {Plugin.Version}  |  Die in the Dungeon {Game.Version}");
            GUILayout.FlexibleSpace();
            if (UI.Button("X", 30)) _menuOpen = false;
            UI.EndRow();
            UI.Label($"État : {Game.State}   Run : {(Game.InRun ? "oui" : "non")}   Combat : {(Game.InBattle ? $"tour {Game.Turn} ({Game.TurnState})" : "non")}");
            UI.Space();

            _scroll = GUILayout.BeginScrollView(_scroll, UI.None);
            if (_selected < 0 || _selected >= Loader.Mods.Count) DrawModList();
            else DrawModPage(Loader.Mods[_selected]);
            GUILayout.EndScrollView();

            GUILayout.EndArea();
        }

        private void DrawModList()
        {
            if (Loader.Mods.Count == 0)
            {
                UI.Label("Aucun mod trouvé.");
                UI.Label($"Dépose des DLL dans :\n{Loader.ModsDirectory}");
                return;
            }

            for (int i = 0; i < Loader.Mods.Count; i++)
            {
                var lm = Loader.Mods[i];
                UI.BeginRow();
                if (lm.Instance != null)
                {
                    bool on = UI.Toggle(lm.IsEnabled, "");
                    if (on != lm.IsEnabled) Loader.SetEnabled(lm, on);
                }
                else UI.Label("[X]");
                UI.Label($"<b>{lm.Info.Name}</b> v{lm.Info.Version} - {lm.Info.Author}");
                GUILayout.FlexibleSpace();
                if (lm.Instance != null && UI.Button("Options", 80)) { _selected = i; _scroll = Vector2.zero; }
                UI.EndRow();
                if (!string.IsNullOrEmpty(lm.Info.Description)) UI.Label("    " + lm.Info.Description);
                if (lm.Error != null) UI.Label($"    <color=#ff6060>Erreur : {lm.Error}</color>");
                UI.Space(4);
            }
        }

        private void DrawModPage(LoadedMod lm)
        {
            UI.BeginRow();
            if (UI.Button("< Retour", 90)) { _selected = -1; _scroll = Vector2.zero; }
            UI.Header($"  {lm.Info.Name} v{lm.Info.Version}");
            UI.EndRow();
            UI.Label($"Id : {lm.Info.Id}   |   Fichier : {System.IO.Path.GetFileName(lm.FilePath)}");
            if (lm.Error != null) UI.Label($"<color=#ff6060>Erreur : {lm.Error}</color>");
            UI.Space();

            if (!lm.IsEnabled) { UI.Label("Mod désactivé."); return; }
            try { lm.Instance.OnSettingsGUI(); }
            catch (Exception e)
            {
                lm.Error = "OnSettingsGUI : " + e.Message;
                Loader.Log.LogError($"[{lm.Info.Name}] OnSettingsGUI : {e}");
                _selected = -1;
            }
        }
    }
}
