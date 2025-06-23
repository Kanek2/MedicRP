using Exiled.API.Enums;
using Exiled.API.Interfaces;
using System.Collections.Generic;
using System.ComponentModel;

namespace MedicRP
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;

        [Description("Medkit healing duration in seconds")]
        public float HealingTime { get; set; } = 10f;

        [Description("Total HP healed from medkit")]
        public float HealingAmount { get; set; } = 10f;

        [Description("Painkiller healing duration in seconds")]
        public float PainkillerDuration { get; set; } = 2f;

        [Description("Total HP healed from painkiller")]
        public float PainkillerTotalHeal { get; set; } = 15f;

        [Description("Potential loss per medkit use")]
        public float PotentialLossPerMedkitUse { get; set; } = 3.33f;

        [Description("Potential loss per painkiller use")]
        public float PotentialLossPainkiller { get; set; } = 2f;

        [Description("Maximum healing distance")]
        public float HealDistance { get; set; } = 3f;

        [Description("Medic bonus HP")]
        public float MedicHealingBuff { get; set; } = 20f;
        
        [Description("Default potential [0-100]")]
        public float DefaultPotential { get; set; } = 100f;

        [Description("Max medkit uses")]
        public int MaxMedkitUses { get; set; } = 3;
        

        [Description("Medic role identifiers")]
        public List<string> MedicBuffRoles { get; set; } = new()
        {
            "Medic",
            "Doctor",
            "Paramedic",
            "MD",
            "Medical"
        };
    }
}