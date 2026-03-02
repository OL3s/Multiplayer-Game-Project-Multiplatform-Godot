# Services

This game is basing itself on good practices in **high cohesion, low coupling**, this is done by splitting the game in to different services.

Current services planned:
| Services | MapGeneratorService.cs | AnimationService.cs | NetworkService.cs (autoload) | CombatSystemService.cs |
| - | - | - | - | - |
| Description | Generates maps and relays the data generated to the godot engine | Animation function to add on top of vector2D positions * time | Networking for multiplayer gameplay | Calculation for ingame combat, damage etc |
