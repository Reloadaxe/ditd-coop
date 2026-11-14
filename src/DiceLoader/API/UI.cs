using System;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace DiceLoader
{
    /// <summary>
    /// Petits wrappers IMGUI utilisables dans <see cref="DiceMod.OnSettingsGUI"/>.
    /// Sous IL2CPP, les méthodes GUILayout attendent un Il2CppReferenceArray&lt;GUILayoutOption&gt;.
    /// Ces helpers le fournissent pour toi.
    /// Méthodes disponibles dans le jeu (le reste a été strippé) : Label, Button, Toggle, TextArea, Space,
    /// Begin/EndHorizontal, Begin/EndVertical, Begin/EndScrollView, Box.
    /// </summary>
    public static class UI
    {
        public static readonly Il2CppReferenceArray<GUILayoutOption> None = new Il2CppReferenceArray<GUILayoutOption>(0);

        public static Il2CppReferenceArray<GUILayoutOption> Opt(params GUILayoutOption[] o) => new Il2CppReferenceArray<GUILayoutOption>(o);

        public static void Label(string text) => GUILayout.Label(text, None);
        public static void Header(string text) => GUILayout.Label("<b>" + text + "</b>", None);
        public static bool Button(string text) => GUILayout.Button(text, None);
        public static bool Button(string text, float width) => GUILayout.Button(text, Opt(GUILayout.Width(width)));
        public static bool Toggle(bool value, string text) => GUILayout.Toggle(value, text, None);
        public static void Space(float px = 8f) => GUILayout.Space(px);
        public static void BeginRow() => GUILayout.BeginHorizontal(None);
        public static void EndRow() => GUILayout.EndHorizontal();
        public static void BeginColumn() => GUILayout.BeginVertical(None);
        public static void EndColumn() => GUILayout.EndVertical();

        /// <summary>Entier modifiable avec des boutons − / +.</summary>
        public static int IntStepper(string label, int value, int step = 1, int min = int.MinValue, int max = int.MaxValue)
        {
            BeginRow();
            GUILayout.Label($"{label} : {value}", Opt(GUILayout.Width(220)));
            if (GUILayout.Button("-" + step, Opt(GUILayout.Width(50)))) value -= step;
            if (GUILayout.Button("+" + step, Opt(GUILayout.Width(50)))) value += step;
            EndRow();
            return Math.Clamp(value, min, max);
        }

        /// <summary>Float modifiable avec des boutons − / +.</summary>
        public static float FloatStepper(string label, float value, float step = 0.1f, float min = float.MinValue, float max = float.MaxValue)
        {
            BeginRow();
            GUILayout.Label($"{label} : {value:0.##}", Opt(GUILayout.Width(220)));
            if (GUILayout.Button("-", Opt(GUILayout.Width(50)))) value -= step;
            if (GUILayout.Button("+", Opt(GUILayout.Width(50)))) value += step;
            EndRow();
            return Math.Clamp(value, min, max);
        }

        /// <summary>Champ texte (TextArea mono-ligne : TextField est strippé dans ce build).</summary>
        public static string Text(string value) => GUILayout.TextArea(value ?? "", None);
    }
}
