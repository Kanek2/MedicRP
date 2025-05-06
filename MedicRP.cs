using Exiled.API.Features;
using Exiled.Events.Handlers;
using System;
using HarmonyLib;

namespace MedicRP
{
    public class Plugin : Plugin<Config>
    {
        public override string Name => "MedicRP";
        public override string Author => "Kanek";
        public override Version RequiredExiledVersion => new Version(9, 5, 2);
        public override Version Version => new Version(2, 0, 0);
        
        public static Plugin Instance { get; private set; }
        private MRPMainEventHandler eventHandler;

        public override void OnEnabled()
        {
            Instance = this;
            eventHandler = new MRPMainEventHandler();
            
            Exiled.Events.Handlers.Player.UsingItem += eventHandler.OnUsingItem;
            Exiled.Events.Handlers.Player.UsingItemCompleted += eventHandler.OnUsedItem;
            Exiled.Events.Handlers.Player.ChangingItem += eventHandler.OnSelectedItem;
            Exiled.Events.Handlers.Player.PickingUpItem += eventHandler.OnPickingUpItem;
            Exiled.Events.Handlers.Player.ChangingRole += eventHandler.OnChangingRole;
            Exiled.Events.Handlers.Player.TogglingNoClip += eventHandler.OnTogglingNoClip;
            
            Harmony harmony = new Harmony("MedicItemsPatch");
            harmony.PatchAll(); 

            
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Exiled.Events.Handlers.Player.UsingItem -= eventHandler.OnUsingItem;
            Exiled.Events.Handlers.Player.UsingItemCompleted -= eventHandler.OnUsedItem;
            Exiled.Events.Handlers.Player.ChangingItem -= eventHandler.OnSelectedItem;
            Exiled.Events.Handlers.Player.PickingUpItem -= eventHandler.OnPickingUpItem;
            Exiled.Events.Handlers.Player.ChangingRole -= eventHandler.OnChangingRole;
            Exiled.Events.Handlers.Player.TogglingNoClip -= eventHandler.OnTogglingNoClip;
            
            eventHandler.Dispose();
            Instance = null;
            base.OnDisabled();
        }
    }
}