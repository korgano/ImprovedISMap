# BattleTech Mod Logging Rules

## Core Principles

- Logging is REQUIRED for all mods
- Logs must be structured, consistent, and actionable
- Logging must support:
  - Debugging
  - Conflict detection
  - Performance tracing

---

## Log Levels

Use consistent levels:

- DEBUG → Detailed internal state
- INFO → Lifecycle events
- WARN → Recoverable issues
- ERROR → Failures impacting functionality

---

## Required Logging Points

### Initialization

- Mod startup
- Dependency resolution
- Harmony patch registration

Example:
[INFO] MyMod :: Initializing v1.2.0

---

### JSON/Data Injection

- Files patched
- Merge operations
- Missing targets

Example:
[DEBUG] MyMod :: Injected weapon_def_Laser_X

---

### Harmony Patches

- Patch applied
- Method targeted
- Execution (optional for high-risk patches)

Example:
[INFO] MyMod :: Patched CombatResolver.ResolveAttack

---

### Errors

- ALWAYS include:
  - Exception message
  - Stack trace
  - Context (method, data)

Example:
[ERROR] MyMod :: Failed to patch weapon stats
Exception: NullReferenceException

---

## Formatting Rules

Standard format:

[LEVEL] <ModName> :: <Message>

- NEVER log raw objects without context
- ALWAYS include identifiers (IDs, names)

---

## Performance Logging

- Log slow operations (>50ms)
- Avoid logging inside tight loops unless DEBUG gated

---

## File Locations

Primary log:
- BATTLETECH/Mods/.modtek/ModTek.log

Mods MUST integrate with ModTek logging system

---

## Debug Mode

- Provide toggle via config
- DEBUG logs must be suppressible in production

---

## Conflict Detection

Log when:
- Multiple mods patch same method
- Data is overridden unexpectedly

Example:
[WARN] MyMod :: Conflict detected on weapon_def_Laser_X

---

## Anti-Patterns

- Silent failures ❌
- Logging without context ❌
- Excessive spam in INFO level ❌
- No error handling ❌

---

## Recommended Implementation (C#)

```csharp
public static class Log
{
    public static void Info(string msg) => ModTek.Log.Info($"MyMod :: {msg}");
    public static void Debug(string msg) => ModTek.Log.Debug($"MyMod :: {msg}");
    public static void Warn(string msg) => ModTek.Log.Warn($"MyMod :: {msg}");
    public static void Error(string msg) => ModTek.Log.Error($"MyMod :: {msg}");
}
````

---

## Advanced Practices

* Correlate logs with IDs (combat, unit, weapon)
* Use structured logging where possible
* Tag logs with subsystem (e.g., [AI], [Weapons])

---

## Summary

Good logging is critical in BattleTech modding due to:

* Heavy mod interaction
* Runtime patching (Harmony)
* Dynamic data injection (ModTek)

Logs are the PRIMARY debugging tool in production environments.
