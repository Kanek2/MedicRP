using Exiled.API.Enums;
using Exiled.API.Interfaces;
using System.Collections.Generic;
using System.ComponentModel;

namespace MedicRP
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = true;

        [Description("Max healing distance")]
        public float HealLookDistance { get; set; } = 3f;

        [Description("Allowed movement during healing")]
        public float HealMaxMovement { get; set; } = 1f;

        [Description("Makeshift healing duration")]
        public int HealingTime { get; set; } = 10;

        [Description("HP from makeshift healing")]
        public float HealingAmount { get; set; } = 10f;

        [Description("Medic bonus HP")]
        public float MedicHealingBuff { get; set; } = 20f;
        
        [Description("Default potential")]
        public float DefaultPotential { get; set; } = 100f;

        [Description("Max medkit uses")]
        public int MaxMedkitUses { get; set; } = 3;

        [Description("Healing reduction per use")]
        public float HealingReduction { get; set; } = 3.33f;

        [Description("Medic role identifiers")]
        public List<string> medicBuffRoles { get; set; } = new()
        {
            "Medic",
            "Doctor",
            "Paramedic",
            "MD",
            "Medical"
        };
    }
}