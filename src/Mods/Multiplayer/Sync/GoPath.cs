using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DiceMultiplayer.Sync
{
    /// <summary>
    /// Identifie un GameObject par son chemin dans la hiérarchie : "Racine#0/Enfant#3/Bouton#1".
    /// L'index de fratrie permet de distinguer les clones. Comme tout le monde joue la même run,
    /// les hiérarchies sont identiques d'un client à l'autre.
    /// </summary>
    public static class GoPath
    {
        public static string Of(GameObject go)
        {
            var parts = new List<string>();
            var t = go.transform;
            while (t != null)
            {
                parts.Add(t.name.Replace("/", "_") + "#" + t.GetSiblingIndex());
                t = t.parent;
            }
            parts.Reverse();
            return string.Join("/", parts);
        }

        public static GameObject Find(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            var segs = path.Split('/');
            Split(segs[0], out var rootName, out _);

            GameObject root = GameObject.Find("/" + rootName);
            if (root == null) return null;
            var t = root.transform;

            for (int i = 1; i < segs.Length; i++)
            {
                Split(segs[i], out var name, out var idx);
                Transform next = null;
                if (idx >= 0 && idx < t.childCount)
                {
                    var c = t.GetChild(idx);
                    if (c.name.Replace("/", "_") == name) next = c;
                }
                if (next == null) // repli : premier enfant actif portant ce nom
                {
                    for (int k = 0; k < t.childCount; k++)
                    {
                        var c = t.GetChild(k);
                        if (c.name.Replace("/", "_") == name && (next == null || c.gameObject.activeInHierarchy)) next = c;
                    }
                }
                if (next == null) return null;
                t = next;
            }
            return t.gameObject;
        }

        private static void Split(string seg, out string name, out int idx)
        {
            int h = seg.LastIndexOf('#');
            if (h < 0) { name = seg; idx = -1; return; }
            name = seg.Substring(0, h);
            if (!int.TryParse(seg.Substring(h + 1), out idx)) idx = -1;
        }

        /// <summary>Vrai si un parent (ou l'objet lui-même) a un nom contenant un des mots-clés.</summary>
        public static bool PathContains(string path, IEnumerable<string> keywords)
        {
            foreach (var k in keywords)
                if (!string.IsNullOrWhiteSpace(k) && path.IndexOf(k.Trim(), System.StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            return false;
        }
    }
}
