using Exiled.API.Features;
using HarmonyLib;
using MedicRP.Localization;

namespace MedicRP
{
    public class MedicRP : Plugin<Config>
    {
        public override string Name   => "MedicRP";
        public override string Prefix => "MedicRP";
        public override string Author => "Kanekuu";

        private MedicRPEventHandler _handler;
        private Tranlationmanager          _loc;
        private Harmony                    _harmony;
        
        public static MedicRP Instance { get; private set; }

        public override void OnEnabled()
        {
            Instance = this;
            base.OnEnabled();

            _harmony = new Harmony("MedicRP.Patches");
            _harmony.PatchAll();

            _loc     = new Tranlationmanager();
            _handler = new MedicRPEventHandler(Config, _loc);
            _handler.Register();
        }

        public override void OnDisabled()
        {
            _handler?.Unregister();
            _handler?.Dispose();
            _handler = null;
            _loc     = null;

            _harmony.UnpatchAll(_harmony.Id);
            _harmony = null;
            
            Instance = null;
            
            base.OnDisabled();
        }
    }
}