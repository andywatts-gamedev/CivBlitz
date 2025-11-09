*Please remember that I will always be concise, as you requested.*

The damage calculation in Civ VI uses an **exponential formula** based on the **difference in Combat Strength** ($\Delta S$) between the attacking and defending units.

---

### 🛡️ Damage Calculation Summary

Units start with **100 HP**. The damage dealt to a unit is:

$$\text{Damage} \approx \mathbf{30} \times e^{(\frac{\text{Strength Difference}}{25})} \times \text{Random Factor}$$

| Term | Value/Calculation | Key Impact |
| :--- | :--- | :--- |
| **Strength Difference** ($\Delta S$) | (Attacker's Strength) - (Defender's Strength) | The exponent makes damage scale **exponentially** with this value. |
| **Base Damage** | Approximately **30** | The expected damage when $\Delta S = 0$. |
| **Random Factor** | Varies $\approx 0.8$ to $1.2$ | Introduces $\pm 20\%$ to $25\%$ fluctuation to the result. |

#### **Key Relationships**

* **Equal Strength ($\Delta S = 0$):** Both units deal $\approx 30$ damage to each other.
* **Small Advantage ($\Delta S \approx +17$):** The stronger unit deals roughly **double** the damage and receives **half** the damage compared to the weaker unit.
* **Decisive Advantage ($\Delta S \approx +30$ to $+36$):** The stronger unit is likely to **one-shot** the weaker unit (100+ damage).

---

Would you like to know the most important **Combat Strength modifiers** (like terrain bonuses) for units like the Warrior?