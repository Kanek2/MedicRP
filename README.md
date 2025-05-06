# MedicRP – Advanced Healing System for SCP: Secret Laboratory

MedicRP is an EXILED plugin that adds a more dynamic and strategic medical system to SCP: Secret Laboratory through the Healing Potential mechanic.

## 🔧 Main Features

### 🏥 Healing System
- Players start with a **Healing Potential** (default 100%) that affects how much health is restored.
- **Medkits** have 3 uses and heal **20 × (current potential / 100)** HP.
- Each medkit use reduces potential by **3.33%** (configurable).
- **SCP-500** fully restores health and resets potential to 100%.
- **Medics** can heal without medkits using a makeshift ability (via noclip key), with visual progress and feedback.
- Healing others is done by looking at them and using a medkit; raycasting selects the closest valid target.

### ⚕️ Medic Role Support
- Medics receive bonus healing (e.g. +20 HP).
- Makeshift healing is exclusive to medical roles.
- Potential and healing bonuses can be customized per role.
- Healing Potential resets when changing roles.

### ⚙️ Fully Configurable
- Customize healing values, medkit uses, SCP-500 effects, movement restrictions, and more in the config file.

## 🔢 Example Healing Values

| Heal Count | Potential | Medkit Heal (Normal) | Medkit Heal (Medic) |
|------------|-----------|----------------------|---------------------|
| 1st        | 100%      | 20 HP                | 40 HP               |
| 2nd        | 96.67%    | 19.33 HP             | 39.33 HP            |
| 3rd        | 93.34%    | 18.67 HP             | 38.67 HP            |
| After SCP-500 | 100%   | 20 HP                | 40 HP               |

## 📦 Installation

1. Install [EXILED](https://github.com/Exiled-Team/EXILED) (v9.5.2+ required)
2. Download `MedicRP.dll`
3. Place it in your server’s `Plugins` folder
4. Edit `configs/MedicRP.yml` as needed
5. Restart the server

## 📃 Plugin Info

| Field           | Value        |
|----------------|--------------|
| Name           | MedicRP      |
| Version        | 2.0.0        |
| Author         | Kanekuu      |
| Requires       | EXILED 9.5.2+|
| Dependencies   | EXILED, MEC, HarmonyX |

## 🤝 Contributing

- Report bugs via GitHub Issues
- Suggest features in Discussions
- Submit pull requests for improvements

## 📜 License

Licensed under the [MIT License](LICENSE.md)

---

*Need help? Open an issue on GitHub or reach out on the EXILED Discord.*
