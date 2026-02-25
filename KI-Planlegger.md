# Plan: 2D PvP Arena Shooter (grid + destructible tiles) + læringsløp Godot → Roblox

## 1) Spillkonsept (core)
- **Type:** 2D arena shooter, PvP, 2–8 spillere
- **Map:** grid-basert (TileMap), deler av kartet er **destruerbart**
- **Prosjektiler:** kuler er **objekter** (ikke hitscan)
- **Teams (server settings):**
  - `team_id = 0` → Free-for-all (alle er fiender)
  - `team_id = 1..N` → samme team = allies
  - Ally-regel: hvis en av dem har `team_id=0` → alltid fiender
- **Nettverksmodell:** dedicated server (for å unngå port-forwarding for spillere)
- **Autoritet:** server er alltid “truth”
  - klienter sender kun: input (move/aim/shoot)
  - server simulerer: movement, collisions, bullets, tile-destruction, damage/HP

---

## 2) Systemdesign (høy-nivå)
### Scener
- `Login.tscn` → login to game
- `Menu.tscn` → start/join (dedicated server IP)
- `Lobby.tscn` → players join/ready + settings (TeamsEnabled, map seed)
- `Game.tscn` → match
- `Player.tscn` → CharacterBody2D + view
- `Bullet.tscn` → Area2D/CharacterBody2D (server sim)

### Autoloads (global)
- `NetManager`:
  - Start server/client
  - håndtere connect/disconnect events
- `GameState`:
  - match config (TeamsEnabled, etc.)
  - mapping `peerId -> teamId`
  - match meta (score, timer, seed)

### Nettverk (ENet/UDP)
- **Server:**
  - spawner players og bullets
  - oppdaterer tilemap (destructible tiles)
  - broadcaster snapshots/events
- **Clients:**
  - local prediction for egen spiller (senere)
  - remote players = interpolasjon
  - bullets renderes fra server state

---

## 3) Destructible grid tiles (tilemap)
### Krav
- Server er eneste som bestemmer:
  - hvilke tiles blir ødelagt
  - når de blir ødelagt
- Clients får oppdatering som en hendelse:
  - “tile (x,y,layer) changed to empty/new tile”

### Minimum nett-synk
- Server sender tile-change events:
  - position (grid x,y)
  - tile id / “empty”
  - layer (hvis flere)
- Clients anvender endringen lokalt på TileMap

---

## 4) Bullet / projectile system (objekt-basert)
### Bullet ansvar
- Server simulerer:
  - posisjon, velocity
  - kollisjon med players + world
  - damage/knockback
  - treff på tiles → request tile-destruction

### Nett-synk
- Spawn:
  - client sender “shoot request” (aim + timestamp)
  - server spawner bullet og replicerer spawn til alle
- Update:
  - enten snapshot (pos/vel) periodisk
  - eller deterministisk bullet (clients “follow” spawn params + korrigeres ved behov)
- Despawn:
  - server sender bullet-despawn event

---

## 5) Framgangsplan (Godot først)
### Fase 0 — Baseline prosjekt
- Oppsett repo + scenes + input map
- Implementer `NetManager` + connect/disconnect
- Dedicated server headless export (senere)

### Fase 1 — Multiplayer “skeleton”
- Lobby: join/ready
- Server scene-change: Lobby → Game
- Server spawner 2–8 `Player` med authority
- Clients ser alle spillere

### Fase 2 — Movement sync
- Server-authoritative movement
- Clients:
  - egen spiller: (start enkel) ingen prediction → så add prediction
  - andre spillere: interpolasjon til server snapshots

### Fase 3 — Bullets (objekt)
- Client shoot request → server spawner bullet
- Bullet collisions på server
- Damage event broadcast
- Basic HP + death + respawn

### Fase 4 — Destructible tiles
- TileMap grid with “damageable tiles”
- Bullet hits tile → server bestemmer tile update
- Server broadcaster tile-change events
- Clients oppdaterer tilemap identisk

### Fase 5 — Teams + rules
- `TeamsEnabled` setting i lobby
- Server assign team_id ved join (balanced eller host-valg)
- Friendly fire policy (default: off)
- Scoreboard: team score eller FFA kills

### Fase 6 — Polish + stability
- Lag compensation light (valgfritt)
- Rate limiting / sanity checks (anti-cheat basics)
- Spectate / reconnect (valgfritt)
- Match timer + map rotation/seed

---

## 6) Etter Godot: Roblox senere (mål)
### Hensikt
- Bruke Roblox for:
  - ekstremt rask “play together”
  - mindre nettverk/infra
  - cross-platform “out of the box”
### Plan
- Re-implement samme core loop:
  - arena → bullets → destructible-ish (må tilpasses Roblox physics)
  - teams + scoreboard
- Lære Luau + Roblox replication model
- Bruke VS Code workflow (Rojo) når du er klar

---

## 7) Kritiske regler (for å unngå desync/cheat)
- Clients sender aldri damage/hit-confirmation
- Server bestemmer:
  - hits
  - HP
  - tile destruction
  - bullet collisions
- Clients:
  - input + rendering + interpolation
  - prediction kun for “feel”, aldri truth

---
