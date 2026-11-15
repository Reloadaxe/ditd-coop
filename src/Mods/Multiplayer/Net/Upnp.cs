using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace DiceMultiplayer.Net
{
    /// <summary>
    /// Ouverture automatique du port de l'hôte sur la box (UPnP IGD), sans serveur :
    ///  1. découverte de la box (SSDP, multicast 239.255.255.250:1900) ;
    ///  2. lecture de sa description, recherche du service WANIPConnection / WANPPPConnection ;
    ///  3. AddPortMapping (TCP, port du jeu → ce PC) puis GetExternalIPAddress (adresse publique).
    /// Tout se fait en arrière-plan ; le résultat est lu par l'interface (Status, ExternalIp). Le port est refermé à la
    /// fin de la partie (Close).
    /// </summary>
    public sealed class Upnp
    {
        public enum State { Working, Open, Failed }

        public volatile State Current = State.Working;
        public volatile string Status = "Ouverture du port sur la box…";
        public volatile string ExternalIp;
        public readonly int Port;

        private string _controlUrl, _serviceType, _localIp;
        private static readonly HttpClient Http = new HttpClient { Timeout = TimeSpan.FromSeconds(4) };

        private Upnp(int port) { Port = port; }

        public static Upnp Start(int port)
        {
            var u = new Upnp(port);
            Task.Run(() => u.Run());
            return u;
        }

        private async Task Run()
        {
            try
            {
                var locations = Discover(TimeSpan.FromSeconds(3));
                if (locations.Count == 0) { Fail("Box introuvable en UPnP (UPnP désactivé ?)"); return; }
                foreach (var loc in locations)
                {
                    if (await FindService(loc)) break;
                }
                if (_controlUrl == null) { Fail("La box ne propose pas l'ouverture de port UPnP"); return; }

                _localIp = LocalIpToward(new Uri(_controlUrl).Host);
                var add = await Soap("AddPortMapping",
                    $"<NewRemoteHost></NewRemoteHost><NewExternalPort>{Port}</NewExternalPort><NewProtocol>TCP</NewProtocol>" +
                    $"<NewInternalPort>{Port}</NewInternalPort><NewInternalClient>{_localIp}</NewInternalClient><NewEnabled>1</NewEnabled>" +
                    $"<NewPortMappingDescription>Die in the Dungeon coop</NewPortMappingDescription><NewLeaseDuration>0</NewLeaseDuration>");
                if (add == null) { Fail("La box a refusé d'ouvrir le port"); return; }

                var ip = await Soap("GetExternalIPAddress", "");
                var m = ip != null ? Regex.Match(ip, "<NewExternalIPAddress>([^<]*)</NewExternalIPAddress>") : null;
                ExternalIp = m != null && m.Success ? m.Groups[1].Value.Trim() : null;
                if (string.IsNullOrEmpty(ExternalIp)) { Fail("Port ouvert, mais adresse publique inconnue"); return; }
                if (IsSharedOrPrivate(ExternalIp))
                {
                    Fail($"Ta connexion partage son adresse publique ({ExternalIp}) : passe par un VPN (Radmin, ZeroTier…)");
                    return;
                }
                Current = State.Open;
                Status = "Port ouvert automatiquement sur la box";
            }
            catch (Exception e) { Fail("Ouverture automatique impossible : " + e.Message); }
        }

        private void Fail(string why) { Status = why; Current = State.Failed; }

        /// <summary>Referme le port sur la box (fin de partie). Sans attente.</summary>
        public void Close()
        {
            if (_controlUrl == null || Current != State.Open) return;
            var body = $"<NewRemoteHost></NewRemoteHost><NewExternalPort>{Port}</NewExternalPort><NewProtocol>TCP</NewProtocol>";
            Task.Run(async () => { try { await Soap("DeletePortMapping", body); } catch { } });
        }

        // ───── SSDP ─────

        private static List<string> Discover(TimeSpan timeout)
        {
            var found = new List<string>();
            var targets = new[]
            {
                "urn:schemas-upnp-org:device:InternetGatewayDevice:1",
                "urn:schemas-upnp-org:device:InternetGatewayDevice:2",
                "urn:schemas-upnp-org:service:WANIPConnection:1",
                "urn:schemas-upnp-org:service:WANPPPConnection:1",
            };
            using var udp = new UdpClient(AddressFamily.InterNetwork);
            udp.Client.ReceiveTimeout = 500;
            var ep = new IPEndPoint(IPAddress.Parse("239.255.255.250"), 1900);
            foreach (var st in targets)
            {
                var req = "M-SEARCH * HTTP/1.1\r\nHOST: 239.255.255.250:1900\r\nMAN: \"ssdp:discover\"\r\nMX: 2\r\nST: " + st + "\r\n\r\n";
                var data = Encoding.ASCII.GetBytes(req);
                udp.Send(data, data.Length, ep);
            }
            var end = DateTime.UtcNow + timeout;
            while (DateTime.UtcNow < end)
            {
                try
                {
                    IPEndPoint from = null;
                    var resp = Encoding.ASCII.GetString(udp.Receive(ref from));
                    var m = Regex.Match(resp, @"^LOCATION:\s*(\S+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    if (m.Success && !found.Contains(m.Groups[1].Value)) found.Add(m.Groups[1].Value);
                }
                catch (SocketException) { }
            }
            return found;
        }

        private async Task<bool> FindService(string location)
        {
            try
            {
                var xml = await Http.GetStringAsync(location);
                foreach (Match s in Regex.Matches(xml, @"<service>(.*?)</service>", RegexOptions.Singleline | RegexOptions.IgnoreCase))
                {
                    var type = Regex.Match(s.Value, @"<serviceType>([^<]*)</serviceType>", RegexOptions.IgnoreCase).Groups[1].Value.Trim();
                    if (!type.Contains("WANIPConnection") && !type.Contains("WANPPPConnection")) continue;
                    var ctrl = Regex.Match(s.Value, @"<controlURL>([^<]*)</controlURL>", RegexOptions.IgnoreCase).Groups[1].Value.Trim();
                    if (string.IsNullOrEmpty(ctrl)) continue;
                    var baseMatch = Regex.Match(xml, @"<URLBase>([^<]*)</URLBase>", RegexOptions.IgnoreCase);
                    var baseUri = baseMatch.Success && baseMatch.Groups[1].Value.Trim().Length > 0 ? new Uri(baseMatch.Groups[1].Value.Trim()) : new Uri(location);
                    _controlUrl = new Uri(baseUri, ctrl).ToString();
                    _serviceType = type;
                    return true;
                }
            }
            catch { }
            return false;
        }

        private async Task<string> Soap(string action, string args)
        {
            var body = "<?xml version=\"1.0\"?><s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\" " +
                       "s:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\"><s:Body>" +
                       $"<u:{action} xmlns:u=\"{_serviceType}\">{args}</u:{action}></s:Body></s:Envelope>";
            using var req = new HttpRequestMessage(HttpMethod.Post, _controlUrl)
            {
                Content = new StringContent(body, Encoding.UTF8, "text/xml"),
            };
            req.Headers.TryAddWithoutValidation("SOAPAction", $"\"{_serviceType}#{action}\"");
            using var resp = await Http.SendAsync(req);
            var text = await resp.Content.ReadAsStringAsync();
            return resp.IsSuccessStatusCode ? text : null;
        }

        /// <summary>Adresse de ce PC sur le réseau de la box (interface utilisée pour la joindre).</summary>
        private static string LocalIpToward(string host)
        {
            using var s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            s.Connect(host, 1900);
            return ((IPEndPoint)s.LocalEndPoint).Address.ToString();
        }

        /// <summary>Adresse privée ou partagée (CGNAT 100.64/10) : injoignable depuis Internet.</summary>
        private static bool IsSharedOrPrivate(string ip)
        {
            if (!IPAddress.TryParse(ip, out var a)) return true;
            var b = a.GetAddressBytes();
            if (b.Length != 4) return false;
            return b[0] == 10 || (b[0] == 172 && b[1] >= 16 && b[1] <= 31) || (b[0] == 192 && b[1] == 168)
                || (b[0] == 100 && b[1] >= 64 && b[1] <= 127) || b[0] == 127 || b[0] == 0;
        }
    }

    /// <summary>
    /// Code d'invitation : adresse IPv4 + port, encodés en 10 caractères lisibles (base 32 de Crockford, sans I/L/O/U),
    /// présentés en deux blocs « XXXXX-XXXXX ». Le champ « Rejoindre » accepte un code ou une adresse IP.
    /// </summary>
    public static class InviteCode
    {
        private const string Alphabet = "0123456789ABCDEFGHJKMNPQRSTVWXYZ";

        public static string Encode(string ip, int port)
        {
            if (!IPAddress.TryParse(ip, out var a) || a.AddressFamily != AddressFamily.InterNetwork) return null;
            var b = a.GetAddressBytes();
            ulong v = ((ulong)b[0] << 40) | ((ulong)b[1] << 32) | ((ulong)b[2] << 24) | ((ulong)b[3] << 16) | (ushort)port;
            var chars = new char[10];
            for (int i = 9; i >= 0; i--) { chars[i] = Alphabet[(int)(v & 31)]; v >>= 5; }
            var s = new string(chars);
            return s.Substring(0, 5) + "-" + s.Substring(5);
        }

        public static bool TryDecode(string code, out string ip, out int port)
        {
            ip = null; port = 0;
            if (string.IsNullOrWhiteSpace(code)) return false;
            var s = code.Trim().ToUpperInvariant().Replace("-", "").Replace(" ", "")
                        .Replace('O', '0').Replace('I', '1').Replace('L', '1');
            if (s.Length != 10) return false;
            ulong v = 0;
            foreach (var c in s)
            {
                int d = Alphabet.IndexOf(c);
                if (d < 0) return false;
                v = (v << 5) | (uint)d;
            }
            port = (int)(v & 0xFFFF);
            ip = $"{(v >> 40) & 255}.{(v >> 32) & 255}.{(v >> 24) & 255}.{(v >> 16) & 255}";
            return port > 0;
        }
    }
}
