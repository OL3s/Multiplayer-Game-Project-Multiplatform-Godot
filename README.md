# Game Title
Fast-paced 2–8 player 2D PvP arena with fully destructible environment.  
Built in Godot with C#.

## Tech/Requirements
- Godot Engine v4.6.1.stable.mono.official.14d19694e (Mono)
   - *must download the .NET version og dodot*
- .NET 10
- C# knowledge

<img width="875" height="715" alt="image" src="https://github.com/user-attachments/assets/2ec0488b-7d2a-4da1-bb0d-9b88730abdd4" />

<details>
<summary>PlantUML source</summary>

@startuml
title Godot Treestructure on runtime
left to right direction
skinparam shadowing false
skinparam componentStyle rectangle

node "/root (SceneTree root)" as ROOT {

  folder "Autoloads (Services)\n(always loaded as Nodes)" as AUTO {
    component "InputService\n(Node singleton)" as INP
    component "NetworkService\n(Node singleton)" as NET
  }

  folder "Scenes\n(instanced into the tree)" as SCENES {
    component "Menu.tscn\n(root node)" as MENU
    component "Lobby.tscn\n(root node)" as LOBBY
    component "Game.tscn\n(root node)" as GAME {

      component "Player.tscn\n(instanced Node)" as PLAYER
      component "TileSetWalls\n(TileMap node)" as WALLS
      component "Props\n(Node2D container)" as PROPS

    }
  }
}

rectangle "C# Code\n(not in SceneTree by default)" as CODE {
  component "CombatSystemService.cs\n(static)" as COMBAT
  component "MapGenerationService.cs\n(static)" as MAPGEN
}

' Input / logic flow
PLAYER <-- INP

' Combat affects gameplay objects
PLAYER <-- COMBAT
WALLS  <-- COMBAT
PROPS  <-- COMBAT

' Map generation affects level
GAME <-- MAPGEN

' Networking
NET --> GAME
NET --> LOBBY

@enduml
</details>

Netcoding -> ENet

## Importent docs
- **[Services](./documentation/Services.md)**


![Structure](./documentation/godot-runtime-diagram.png)

<details>
<summary>PlantUML source</summary>

```
@startuml
left to right direction
skinparam monochrome true
skinparam shadowing false
skinparam linetype ortho
skinparam usecaseBorderThickness 2
skinparam rectangleBorderThickness 2

database "MySQL" as DB

rectangle "HTTP Service" as HTTP {
  usecase "Matchmaker" as Matchmaker
  usecase "Login" as LoginS
}

rectangle "Godot Runtime (same codebase)" {

  rectangle "Client (n instances)" {
	usecase "Login" as LoginC
	usecase "Menu" as Menu
	usecase "Lobby" as LobbyC
	usecase "Game" as GameC

	LoginC <-> LoginS
	LoginC ..> Menu
	Menu ..> LobbyC
	LobbyC ..> GameC
  }

  rectangle "Server (autonomy)" {
	usecase "Lobby" as LobbyS
	usecase "Game" as GameS

	LobbyS --> GameS
  }
}

' HTTP flows
Menu --> Matchmaker
LoginS <--> DB

' Match assignment
Matchmaker --> LobbyS : Docker execute

' Netcoding (WebRTC)
LobbyC <.> LobbyS : WebRTC
GameC <.> GameS : WebRTC

@enduml
```
</details>

## Core Gameplay
- 2–8 players
- 2D PvP arena combat
- Fully destructible tilebased map
- Round-based matches
- Skill-based movement & aiming

**Map illustration example:**
<img width="992" height="541" alt="image" src="https://github.com/user-attachments/assets/fd0c4b7b-47fa-4cc9-a0c3-92ca383355df" />
*This example is not that accurate, but to give an idea on what direction the maps may like.*


## Multiplayer
- Server-authoritative model
- Clients send: input / actions  
- Server syncs: state / destruction / hits  
- Dedicated server supported

![Communication Model](./documentation/network-usage-diag.png)

<details>
<summary>PlantUML source</summary>

```
@startuml
title ENet RPC Flow (Server Authoritative)
left to right direction
skinparam shadowing false

rectangle "Client 1" as C1
rectangle "Client 2" as C2
rectangle "Client 3" as C3
rectangle "Client 4" as C4

node "Server" as S

' Client 1 sends request to server
C1 --> S : [Rpc(MultiplayerApi.RpcMode.AnyPeer)]\nRequestFunction(payload)

' Server executes authoritative logic
S --> S : Validate + Simulate

' Server broadcasts result to all clients (including sender)
S --> C1 : [Rpc(MultiplayerApi.RpcMode.Authority)]\nExecuteFunction(state)
S --> C2 : [Rpc(MultiplayerApi.RpcMode.Authority)]\nExecuteFunction(state)
S --> C3 : [Rpc(MultiplayerApi.RpcMode.Authority)]\nExecuteFunction(state)
S --> C4 : [Rpc(MultiplayerApi.RpcMode.Authority)]\nExecuteFunction(state)

@enduml
```

</details>

- Crossplay on most platforms
	- Android
	- HTML5
	- Windows
	- Linux
	- Others come later:
		- iOS
		- MacOS
		- Xbox
		- Playstation

## Database
- MySQL

## Status
Prototype / early development

## License
All rights reserved. No use, distribution, or modification permitted without permission from the authors.
