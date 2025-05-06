
```markdown
# MedicRP Plugin for SCP: Secret Laboratory



MedicRP is an EXILED framework plugin that revolutionizes the medical system in SCP: Secret Laboratory, adding depth and strategy to healing mechanics with its unique Healing Potential System.

## Features

### 🏥 Comprehensive Healing System
• The plugin supports a multi-method healing system – healing can be performed using medkits, SCP‑500, or makeshift healing (activated by medics using the noclip function).
• Each player starts with a default healing potential (by default 100%), which directly influences the amount of health restored.
• The effectiveness of healing decreases with each medkit use by a set reduction (default 3.33%).
  For example, medkits heal an amount calculated as 20 × (current potential/100), and players with medical roles receive an additional bonus.
• Makeshift healing for medics uses coroutine processing to provide progressive feedback during the healing process,
  with visual hints indicating the progress, healing potential, and remaining medkituses.
 • The plugin implements target detection using raycasting – it selects the closest alive target within the configured look distance while ignoring the healer.  


```

Key aspects:
- Each player starts with `DefaultPotential` (100% by default)
- Healing effectiveness decreases by `HealingReduction` (3.33% by default) after each heal
- Medkits heal for `20 × (current potential/100)` HP
- SCP-500 completely resets potential to 100%
- Potential is role-specific and resets when changing roles

### ⚕️ Role-Based Medic System
- Special buffs for medical roles (configurable)
- Unique makeshift healing ability
- Enhanced healing effectiveness (+20 HP bonus by default)

### ⚙️ Fully Configurable
- Adjust all parameters via config file:
  - Healing amounts and durations
  - Movement restrictions during healing
  - Medkit uses and depletion rates
  - Healing potential mechanics

## Installation

1. Ensure you have [EXILED](https://github.com/Exiled-Team/EXILED) installed (v9.5.2 or higher)
2. Download the latest MedicRP release
3. Place the `MedicRP.dll` in your EXILED plugins folder
4. Configure settings in `configs/MedicRP.yml`
5. Restart your server


## Healing Effectiveness Examples

| Heal Count | Potential | Medkit Heal (Normal) | Medkit Heal (Medic) |
|------------|-----------|----------------------|---------------------|
| 1st        | 100%      | 20 HP                | 40 HP               |
| 2nd        | 96.67%    | 19.33 HP             | 39.33 HP            |
| 3rd        | 93.34%    | 18.67 HP             | 38.67 HP            |
| ...        | ...       | ...                  | ...                 |
| After SCP-500 | 100%   | 20 HP                | 40 HP               |

*Medics receive DefaultPotential + MedicHealingBuff (20 HP by default)*

## Plugin Information

| Detail | Information |
|--------|-------------|
| **Name** | MedicRP |
| **Version** | 2.0.0 |
| **Author** | Kanekuu |
| **Required EXILED** | 9.5.2+ |
| **Dependencies** | EXILED, MEC, HarmonyX |

## Contributing

We welcome contributions! Here's how you can help:

1. Report bugs via [GitHub Issues]()
2. Suggest features in Discussions
3. Submit pull requests for improvements


## License

This project is licensed under the [MIT License](LICENSE.md).

---

*For support, create an issue on GitHub or tell me on EXILED server.*
```

