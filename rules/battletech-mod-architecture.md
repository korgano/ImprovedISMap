# BattleTech Mod Architecture Rules

## Layered Approach

1. Data Layer (JSON)
2. Behavior Layer (Harmony DLL)
3. Assets Layer (CAB / custom assets)

## Preferred Strategy

- Start with JSON changes
- Add Harmony patches ONLY where necessary
- Reuse community assets instead of creating duplicates

## Mod Types

### 1. Data Mods
- Weapons, stats, constants
- Pure JSON
- Highest compatibility

### 2. Content Mods
- New mechs, factions, items
- JSON + assets

### 3. Overhaul Mods
- Large-scale gameplay changes
- JSON + Harmony + assets

## Load Order Strategy

- Use mod.json dependencies
- Avoid implicit ordering assumptions

## Save Compatibility

- Do not remove IDs used in saves
- Maintain backward compatibility when possible

## Folder Strategy

- Keep mods isolated
- No shared mutable state across mods

## Evolution Strategy

- Version your mod.json
- Provide migration paths for breaking changes

## Ecosystem Awareness

- Many mods depend on shared frameworks
- Ensure compatibility with:
  - CAB
  - Major modpacks

## Summary

BattleTech modding relies on a hybrid model:
- JSON for data-driven changes
- Harmony for behavior injection
- ModTek for orchestration and loading
