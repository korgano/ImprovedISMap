# BattleTech ModTek Rules

## Core Principles

- NEVER overwrite base game files. Use ModTek’s runtime injection and JSON merge system.
- ALL mods must be self-contained and portable.
- Mod loading is dependency-driven; DO NOT rely on manual load ordering.
- Prefer JSON modding over DLL modding unless behavior changes are required.

## Mod Structure

Each mod MUST follow:

/Mods/<ModName>/
  mod.json
  /data/ (optional JSON overrides)
  /dll/ (optional compiled assemblies)
  /assets/ (optional content)

### mod.json (Required)

- Defines:
  - Name
  - Version
  - Dependencies
  - LoadAfter/LoadBefore
- Acts as the single source of truth for load order and compatibility.

## JSON Modding Rules

- Use JSON merges instead of replacing files.
- Target game data located in:
  BattleTech_Data/StreamingAssets/data/
- Common editable domains:
  - weapons
  - mechs
  - constants
  - shops
  - star systems

### Merging

- Prefer additive changes over destructive overrides.
- Use advanced merging features:
  - Append arrays instead of replacing
  - Patch specific fields only
- Avoid full file replacement unless absolutely necessary.

## Manifest Manipulation

- DO NOT edit VersionManifest.csv directly.
- Use ModTek manifest injection.
- Ensure assets are declared and discoverable via mod.json.

## Dependency Management

- Explicitly define dependencies in mod.json.
- Avoid circular dependencies.
- Ensure compatibility with:
  - Community Asset Bundle (CAB)
  - Other large modpacks (e.g., RogueTech, BTA)
 
## DLL Modding Rules

Use DLL mods ONLY when:
- Game logic must be altered
- JSON cannot express required behavior

### Requirements

- Target correct BattleTech assembly versions
- Use ModTek injectors/preloader system
- Log extensively using ModTek logging

## Logging

- ALWAYS log:
  - Initialization
  - Patch application
  - Errors
- Use logs for:
  - Load order debugging
  - Conflict detection
 
## Performance Rules

- Avoid heavy runtime computation in patches
- Cache repeated lookups
- Prefer static data where possible

## Compatibility Rules

- NEVER assume exclusive control of a system
- Design for coexistence with other mods
- Use tags and conditional logic instead of hard overrides

## Content Packs

- Use shared assets (CAB) when possible
- Avoid duplicating large assets
- Ensure assets are referenced, not embedded redundantly

## Testing

- Test with:
  - Clean install
  - Popular modpacks
  - Multiple load orders

- Validate:
  - Game startup
  - Save compatibility
  - Combat scenarios
 
## Anti-Patterns

- Editing base files directly ❌
- Hardcoding IDs without fallback ❌
- Replacing entire JSON files unnecessarily ❌
- Ignoring dependency declarations ❌
