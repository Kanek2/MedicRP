using Exiled.API.Enums;
using Exiled.API.Interfaces;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Exiled.API.Features;

namespace MedicRP
{
    public class Config : IConfig
    {

        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = true;


        [Description("Distance at which you can use the healing command")]
        public float HealLookDistance { get; set; } = 1;
        [Description("Distance at which you can move while using commands")]
        public float HealMaxMovement { get; set; } = 1f;
        [Description("Maximum camera movement range while using commands")]
        public float HealMaxLookingRange { get; set; } = 5f;
        [Description("Enable/disable the heal command")]
        public bool Medkithealcommand { get; set; } = true;
        [Description("Time for makeshift healing")]
        public int HealingTime { get; set; } = 10;
        [Description("Amount of HP added after makeshift healing")]
        public float HealingAmount { get; set; } = 20;
        [Description("HP boost to medkit after patching up as a medic")]
        public float MedicHealingBuff { get; set; } = 20;

        [Description("Keywords to determine if someone is a medic")]
        public List<string> medicBuffRoles { get; set; } = new()
        {
          "Medic",
          "Doctor",
          "Paramedic",
          "MD",
          "Medical",
          "DEBUGMEDIC"
        };





    }
}
