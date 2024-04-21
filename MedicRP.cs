using Exiled.API.Features;
using InventorySystem.Items.Usables;
using System;

namespace MedicRP
{
    public class Plugin : Plugin<Config>
    {
        public override string Name { get; } = "MedicRP";
        public override string Author { get; } = "Kanek";
        public override Version RequiredExiledVersion { get; } = new(8, 8, 0);
        public override Version Version { get; } = new(1, 0, 0);
        public static Plugin Instance { get; set; }



        public override void OnEnabled()
        {
            Log.Warn("Medic RP by Kanek has been loaded. 1.0v");
            Instance = this;
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Instance = null;
            base.OnDisabled();
        }

        

    }
}