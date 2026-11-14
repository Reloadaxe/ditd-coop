using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace DitdLauncher
{
    /// <summary>Contenu de dist/manifest.json, généré par publish.ps1.</summary>
    [DataContract]
    public sealed class Manifest
    {
        [DataMember(Name = "version")] public string Version;
        [DataMember(Name = "changelog")] public string Changelog;
        [DataMember(Name = "launcher")] public RemoteFile Launcher;
        [DataMember(Name = "bepinex")] public BepInExInfo BepInEx;
        [DataMember(Name = "files")] public List<RemoteFile> Files = new List<RemoteFile>();
        /// <summary>Chemins (relatifs au dossier du jeu) à supprimer s'ils existent : anciens fichiers renommés.</summary>
        [DataMember(Name = "remove")] public List<string> Remove = new List<string>();

        public static Manifest Parse(string json)
        {
            var ser = new DataContractJsonSerializer(typeof(Manifest));
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
                return (Manifest)ser.ReadObject(ms);
        }
    }

    [DataContract]
    public sealed class RemoteFile
    {
        /// <summary>Destination relative au dossier du jeu (ex. "Mods/Multiplayer/Multiplayer.dll").</summary>
        [DataMember(Name = "path")] public string Path;
        /// <summary>URL relative à dist/ (ex. "files/Multiplayer.dll") ou absolue.</summary>
        [DataMember(Name = "url")] public string Url;
        [DataMember(Name = "sha256")] public string Sha256;
        [DataMember(Name = "size")] public long Size;
        [DataMember(Name = "version")] public string Version;
        /// <summary>Composant optionnel affiché en case à cocher (ex. "DevTools").</summary>
        [DataMember(Name = "optional")] public string Optional;
    }

    [DataContract]
    public sealed class BepInExInfo
    {
        [DataMember(Name = "version")] public string Version;
        [DataMember(Name = "url")] public string Url;
    }
}
