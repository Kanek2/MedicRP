using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Exiled.API.Features;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MedicRP.Localization
{
    public sealed class Tranlationmanager
    {
        private readonly Dictionary<string, Dictionary<string, string>> _dict;
        private readonly string _defaultLang;
        private const string FileName = "MedicRPTranslations.json";

        public Tranlationmanager()
        {
            var path = Path.Combine(Paths.Configs, FileName);
            if (!File.Exists(path))
            {
                var initial = new
                {
                    meta = new { DefaultLanguage = "en" },
                   en = new Dictionary<string, string>
                        {
                            // ─── MEDKIT ────────────────────────────────
                            ["MedkitEmpty"]            = "<color=red>Medkit is empty!</color>",
                            ["SelfFullHealth"]         = "<color=red>You are already at full health!</color>",
                            ["TargetFullHealth"]       = "<color=red>{0} is already at full health!</color>",
                            ["TargetNoLongerNeedsHealing"] = "<color=red>{0} no longer needs healing!</color>",
                            ["HealingInterrupted"]     = "<color=red>Healing interrupted!</color>",
                            ["TargetOutOfRange"]       = "<color=red>Target out of range!</color>",
                            ["MedkitStatus"]           = "<color=green>Medkit: <color=white>{0}%</color> efficiency\nUses left: <color=white>{1}</color></color>",
                            ["PickedUpMedkit"]         = "<color=green>Picked up a medkit!\nUses left: <color=white>{0}</color></color>",
                            ["MedkitProgress"]   = "<color=green>Healing {0}... [<color=white>{1}%</color>] Efficiency: <color=white>{2}%</color> | Uses left: <color=white>{3}</color></color>",
                            ["MedkitDoneSelf"]   = "<color=green>Healed {0} by <color=white>{1}</color> HP! Remaining potential: <color=white>{2}%</color></color>",
                            ["MedkitDoneTarget"] = "<color=green>{0} healed you by <color=white>{1}</color> HP!</color>",
                            // ─── PAINKILLERS ───────────────────────────
                        
                            ["PainkillerProgress"] = "<color=green>Taking painkillers... <color=white>{0}%</color> | Eff: <color=white>{1}%</color></color>",
                            ["PainkillerDone"]     = "<color=green>Healed <color=white>{0}</color> HP (Remaining potential: <color=white>{1}%</color>)</color>",
                            // ─── SCP-500 ───────────────────────────────
                            ["SCP500_Reset"] = "<color=yellow>SCP-500 restored potential to <color=white>{0}%</color></color>"
                        },

                        pl = new Dictionary<string, string>
                        {
                            // ─── APTECZKA ──────────────────────────────
                            ["MedkitEmpty"]            = "<color=red>Apteczka jest pusta!</color>",
                            ["SelfFullHealth"]         = "<color=red>Masz już pełne zdrowie!</color>",
                            ["TargetFullHealth"]       = "<color=red>{0} ma już pełne zdrowie!</color>",
                            ["TargetNoLongerNeedsHealing"] = "<color=red>{0} nie potrzebuje już leczenia!</color>",
                            ["HealingInterrupted"]     = "<color=red>Leczenie przerwane!</color>",
                            ["TargetOutOfRange"]       = "<color=red>Cel jest poza zasięgiem!</color>",
                            ["MedkitStatus"]           = "<color=green>Apteczka: <color=white>{0}%</color> efektywności\nPozostało użyć: <color=white>{1}</color></color>",
                            ["PickedUpMedkit"]         = "<color=green>Podniesiono apteczkę!\nPozostało użyć: <color=white>{0}</color></color>",

                         
                            ["MedkitProgress"]   = "<color=green>Leczenie {0}... [<color=white>{1}%</color>] Ef: <color=white>{2}%</color> | Pozostało: <color=white>{3}</color></color>",
                            ["MedkitDoneSelf"]   = "<color=green>Wyleczono {0} o <color=white>{1}</color> HP! Pozostały potencjał: <color=white>{2}%</color></color>",
                            ["MedkitDoneTarget"] = "<color=green>{0} uleczył cię o <color=white>{1}</color> HP!</color>",

                            // ─── TABLETKI ──────────────────────────────
                            ["PainkillerProgress"] = "<color=green>Używanie tabletek... <color=white>{0}%</color> | Ef: <color=white>{1}%</color></color>",
                            ["PainkillerDone"]     = "<color=green>Uleczono <color=white>{0}</color> HP (Pozostały potencjał: <color=white>{1}%</color>)</color>",

                            // ─── SCP-500 ───────────────────────────────
                            ["SCP500_Reset"] = "<color=yellow>SCP-500 przywrócił potencjał do <color=white>{0}%</color></color>"
                        }

                };
                File.WriteAllText(path, JsonConvert.SerializeObject(initial, Newtonsoft.Json.Formatting.Indented));
            }

            var root = JObject.Parse(File.ReadAllText(path));
            _defaultLang = root["meta"]?["DefaultLanguage"]?.ToString() ?? "en";
            _dict = new Dictionary<string, Dictionary<string, string>>();
            foreach (var pair in root)
            {
                if (pair.Key == "meta") continue;
                _dict[pair.Key] = pair.Value.ToObject<Dictionary<string, string>>();
            }
        }

        public string T(Player player, string key, params object[] args)
        {
            var lang = player.SessionVariables.TryGetValue("lang", out var l) ? l.ToString() : _defaultLang;
            return T(lang, key, args);
        }

        public string T(string lang, string key, params object[] args)
        {
            if (!_dict.TryGetValue(lang, out var d)) d = _dict[_defaultLang];
            if (!d.TryGetValue(key, out var t)) t = key;
            return args.Length > 0 ? string.Format(t, args) : t;
        }
    }
}
