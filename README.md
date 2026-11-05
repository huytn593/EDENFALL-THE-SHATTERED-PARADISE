# Edenfall: The Shattered Paradise

An atmospheric dark-fantasy narrative RPG and interactive prologue set in **Dawn Sanctuary**.
Embody **Aurelia**, a Luminary seeker uncovering ancient celestial ruins, forging bonds with enigmatic inhabitants, and navigating the fragile balance of a shattered world.

## Core Features & Systems Replicated from Unity Source Architecture

1. **Exploration & Navigation Engine (`Assets/_Edenfall/Scripts/Exploration/`)**
   - Real-time interactive 2.5D Sanctuary exploration map with environmental interactive nodes, shrines, astral gates, and hidden ruins.
   - Dynamic player movement, stamina, and spatial awareness with lighting aura.

2. **Branching Dialogue & Cinematic System (`Assets/_Edenfall/Scripts/Dialogue/`)**
   - Rich multi-path branching dialogue with emotional tone tags (Introspective, Firm, Compassionate, Mystical).
   - Audio feedback and atmospheric typing effects.

3. **NPC Behavior, Memory & Relationships (`Assets/_Edenfall/Scripts/NPC/`)**
   - Dynamic relationship tracking (Affinity, Trust, Suspicion) for key characters:
     - **Aurelia** (Protagonist / Luminary Exile)
     - **Archivist Vaelen** (Keeper of Forgotten Shards)
     - **Commander Teresa** (Vanguard of the Shattered Bastion)
     - **The Resonant Echo / Sylvan Wraith** (Spiritual remnant of Paradise)
   - NPC memory log recording player decisions and unlocking unique branches.

4. **Quest & World State Engine (`Assets/_Edenfall/Data/Quests/` & `WorldStates/`)**
   - Multi-stage quests with branching outcomes affecting the Dawn Sanctuary's celestial resonance.
   - Real-time world state tracking: Celestial Eclipse level, Leyline Balance, Sanctuary wards.

5. **Inventory & Relic Forging (`Assets/_Edenfall/Scripts/Inventory/`)**
   - Inventory of ancient relics, celestial shards, astral maps, and botanical alchemical ingredients.
   - Relic synthesis & inspection with deep lore inscriptions.

6. **Audio & Ambience Synthesis (`Assets/_Edenfall/Audio/`)**
   - Built-in Web Audio API sound synthesizer producing atmospheric astral drones, sanctuary chimes, dialogue clicks, and mystical resonance without external audio asset dependencies.

7. **Art Bible & Game Design Document Explorer (`Docs/GDD/`, `ArtProduction/`)**
   - Comprehensive in-game lore codex, character profiles, concept art viewer, and environment design dossiers.

## Running the App

```bash
npm install
npm run dev
```

Built for Node.js 22 + React + Vite + Tailwind CSS.
