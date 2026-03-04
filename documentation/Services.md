# Services

This game is basing itself on good practices in **high cohesion, low coupling**, this is done by splitting the game in to different services.

Current services planned:

| Services | MapGeneratorService | AnimationService | NetworkService (autoload) | CombatSystemService | InputService |
| - | - | - | - | - | - |
| Description | Generates maps and relays the data generated to the godot engine | Animation function to add on top of vector2D positions * time | Networking for multiplayer gameplay | Calculation for ingame combat, damage etc | Fetch input from player (gamepad, touch, keyboard etc) and relays it |

<details>
<summary>MapGeneratorService.cs</summary>

**TODO**

</details>

<details>
<summary>AnimationService.cs</summary>

**TODO**

</details>

<details>
<summary>NetworkService.cs</summary>

**TODO**

</details>

<details>
<summary>CombatSystemService.cs</summary>

### Main idea
1. Put combat state (health + armor) on an entity using **`CombatContainer`**.
2. Create an attack using **`DamageApply`** (raw damage per `DamageType`).
3. Armor is stored as **`DamageArmor`** (% reduction per `DamageType`).
4. Call `DamageApply.ApplyTo(target)` (or `target.ApplyDamage(damage)`) to deal damage.

### CombatContainer (put this on entities)

`CombatContainer` holds:

- `Health` (int)
- `Armor` (`DamageArmor`)

#### Create a target

```csharp
using Combat;

var target = new CombatContainer(
    health: 100,
    armor: new DamageArmor(fire: 25)
);
```

#### Deal damage to the target

```csharp
var hit = new DamageApply(fire: 50);

var (isDead, damageTaken) = hit.ApplyTo(target);
```

### DamageApply (raw damage)

```csharp
var damage = new DamageApply(fire: 50);

int fire = damage.GetValue(DamageType.Fire);
int poison = damage.GetValue(DamageType.Poison);
```

### DamageArmor (percent reduction)

```csharp
var armor = new DamageArmor(fire: 25);

armor.GetValue(DamageType.Fire);
armor.GetValue(DamageType.Poison);
```

### How armor reduces damage

```
final = raw * (100 - armorPercent) / 100
```

### Full example

```csharp
using Combat;

var target = new CombatContainer(
    health: 100,
    armor: new DamageArmor(fire: 25)
);

var attack = new DamageApply(fire: 50);

var (isDead, damageTaken) = attack.ApplyTo(target);
```

</details>

<details>
<summary>InputService.cs</summary>

**TODO**

</details>
