# BattleTech Harmony (DLL Modding) Rules

## Core Principle

- Use Harmony ONLY for runtime method patching.
- Prefer JSON modding first; Harmony is a last resort.

## Patch Types

- Prefix: runs BEFORE original method
- Postfix: runs AFTER original method
- Transpiler: modifies IL code (advanced, avoid unless necessary)

## Best Practices

- Keep patches minimal and isolated
- NEVER fully replace methods unless unavoidable
- Prefer Postfix over Prefix when possible (less invasive)

## Patch Safety

- Always check for nulls and edge cases
- Avoid assumptions about execution order
- Design patches to tolerate other mods

## Targeting

- Patch the MOST specific method possible
- Avoid patching high-frequency core loops unless optimized

## Harmony Initialization

- Use a unique Harmony ID per mod
- Apply patches during mod initialization phase

## Logging

- Log patch registration
- Log execution entry for debugging critical patches
- Use logs to trace mod conflicts

## Conflict Avoidance

- Do not rely on execution order of patches
- Avoid modifying shared global state
- Use guards to prevent duplicate execution

## Performance

- Avoid allocations inside patches
- Cache reflection results
- Minimize logic inside Prefix/Postfix

## Interoperability

- Ensure compatibility with HarmonyX (used by ModTek)
- Do not bundle outdated Harmony versions

## Anti-Patterns

- Using transpilers unnecessarily ❌
- Heavy logic inside patches ❌
- Patching too many methods broadly ❌
- Ignoring other mods' patches ❌
