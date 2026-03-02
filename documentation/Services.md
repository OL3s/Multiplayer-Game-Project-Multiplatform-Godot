# Services

This game is basing itself on good practices in **high cohesion, low coupling**, this is done by splitting the game in to different services.

Current services planned:
| Services | [MapGeneratorService](##mapgeneratorservicecs) | [AnimationService](##animationservicecs) | [NetworkService (autoload)](##networkservicecs) | [CombatSystemService](##combatsystemservicecs) | [InputService](##InputServicecs)
| - | - | - | - | - | - |
| Description | Generates maps and relays the data generated to the godot engine | Animation function to add on top of vector2D positions * time | Networking for multiplayer gameplay | Calculation for ingame combat, damage etc | Fetch input from player (gamepad, touch, keyboard etc) and relays it |
## MapGeneratorService.cs

**TODO**

## AnimationService.cs

**TODO**

## NetworkingService.cs

**TODO**

## CombasSystemService.cs

# CombatSystemService.cs

## Main idea
1. Put combat state (health + armor) on an entity using **`CombatContainer`**.
2. Create an attack using **`DamageApply`** (raw damage per `DamageType`).
3. Armor is stored as **`DamageArmor`** (% reduction per `DamageType`).
4. Call `DamageApply.ApplyTo(target)` (or `target.ApplyDamage(damage)`) to deal damage.

## CombatContainer (put this on entities)
`CombatContainer` holds:
- `Health` (int)
- `Armor` (`DamageArmor`)

### Create a target
```csharp
using Combat;

var target = new CombatContainer(
    health: 100,
    armor: new DamageArmor(fire: 25) // only set what you care about
);
```

### Deal damage to the target
```csharp
var hit = new DamageApply(fire: 50); // only set what you care about

var (isDead, damageTaken) = hit.ApplyTo(target);
// target.Health was reduced by damageTaken
```

## DamageApply (raw damage)
`DamageApply` represents **raw damage amounts** before armor.

### Example
```csharp
var damage = new DamageApply(fire: 50); // ignore the rest

int fire = damage.GetValue(DamageType.Fire);      // 50
int poison = damage.GetValue(DamageType.Poison);  // 0 (missing = 0)
```

## DamageArmor (percent reduction)
`DamageArmor` represents **% reduction** per damage type:
- `0` = no reduction
- `100` = immune  
Values are clamped to `0..100`.

### Example
```csharp
var armor = new DamageArmor(fire: 25); // ignore the rest

armor.GetValue(DamageType.Fire);   // 25
armor.GetValue(DamageType.Poison); // 0 (missing = 0)
```

## How armor reduces damage (the math)
Per damage type:
```
final = raw * (100 - armorPercent) / 100
```

## Full example (simple)
```csharp
using Combat;

var target = new CombatContainer(
    health: 100,
    armor: new DamageArmor(fire: 25) // 25% fire reduction, everything else defaults to 0
);

var attack = new DamageApply(fire: 50); // only fire damage

var (dead, taken) = attack.ApplyTo(target);

// fire: 50 * (100-25)/100 = 37
// target.Health = 100 - 37 = 63
```

## InputService.cs

**TODO**
