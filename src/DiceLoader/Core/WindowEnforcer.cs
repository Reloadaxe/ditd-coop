using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;

namespace DiceLoader
{
    /// <summary>
    /// Argument de lancement « -ditd-window X Y L H » (passé par le launcher pour les tests à plusieurs fenêtres).
    /// Au chargement du menu, le jeu réapplique sa résolution enregistrée (souvent le plein écran) : on réimpose
    /// donc le mode fenêtré, la taille et la position pendant les premières minutes, puis on laisse la main au joueur.
    /// </summary>
    internal static class WindowEnforcer
    {
        private static bool _enabled;
        private static int _x, _y, _w, _h;
        private static float _nextCheck;
        private const float EnforceSeconds = 150f;

        [DllImport("user32.dll")] private static extern bool SetWindowPos(IntPtr h, IntPtr after, int x, int y, int cx, int cy, uint flags);
        [DllImport("user32.dll")] private static extern bool GetWindowRect(IntPtr h, out RECT r);
        [StructLayout(LayoutKind.Sequential)] private struct RECT { public int Left, Top, Right, Bottom; }

        public static void Init()
        {
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 4; i++)
            {
                if (!string.Equals(args[i], "-ditd-window", StringComparison.OrdinalIgnoreCase)) continue;
                if (int.TryParse(args[i + 1], out _x) && int.TryParse(args[i + 2], out _y)
                    && int.TryParse(args[i + 3], out _w) && int.TryParse(args[i + 4], out _h) && _w > 200 && _h > 150)
                {
                    _enabled = true;
                    Loader.Log.LogInfo($"Fenêtre imposée : {_w}x{_h} en ({_x},{_y}) pendant {EnforceSeconds:0} s.");
                }
            }
        }

        public static void Tick()
        {
            if (!_enabled) return;
            float t = Time.realtimeSinceStartup;
            if (t > EnforceSeconds) { _enabled = false; return; }
            if (t < _nextCheck) return;
            _nextCheck = t + 0.5f;

            try
            {
                // Taille du rendu : fenêtre moins bordures et barre de titre
                int cw = Math.Max(320, _w - 16), ch = Math.Max(240, _h - 39);
                if (Screen.fullScreenMode != FullScreenMode.Windowed || Math.Abs(Screen.width - cw) > 4 || Math.Abs(Screen.height - ch) > 4)
                    Screen.SetResolution(cw, ch, FullScreenMode.Windowed);

                var hwnd = Process.GetCurrentProcess().MainWindowHandle;
                if (hwnd == IntPtr.Zero) return;
                if (GetWindowRect(hwnd, out var r) && (Math.Abs(r.Left - _x) > 2 || Math.Abs(r.Top - _y) > 2 || Math.Abs(r.Right - r.Left - _w) > 4 || Math.Abs(r.Bottom - r.Top - _h) > 4))
                    SetWindowPos(hwnd, IntPtr.Zero, _x, _y, _w, _h, 0x0004 /* SWP_NOZORDER */);
            }
            catch (Exception e)
            {
                Loader.Log.LogWarning("WindowEnforcer : " + e.Message);
                _enabled = false;
            }
        }
    }
}
