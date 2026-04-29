# BattleTech mod.json Schema (Validated Rules)

## Purpose

Defines a strict, machine-validated structure for ModTek-compatible mods to:
- Prevent load errors
- Enforce dependency correctness
- Improve interoperability across mods

---

## JSON Schema (Draft 7)

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "type": "object",
  "required": ["Name", "Version"],
  "properties": {
    "Name": {
      "type": "string",
      "minLength": 1,
      "pattern": "^[A-Za-z0-9._-]+$"
    },
    "Enabled": {
      "type": "boolean",
      "default": true
    },
    "Version": {
      "type": "string",
      "pattern": "^\\d+\\.\\d+\\.\\d+(-[A-Za-z0-9]+)?$"
    },
    "Description": {
      "type": "string"
    },
    "Author": {
      "type": "string"
    },
    "Website": {
      "type": "string",
      "format": "uri"
    },
    "Contact": {
      "type": "string"
    },
    "DependsOn": {
      "type": "array",
      "items": {
        "type": "string"
      },
      "uniqueItems": true
    },
    "ConflictsWith": {
      "type": "array",
      "items": {
        "type": "string"
      },
      "uniqueItems": true
    },
    "LoadAfter": {
      "type": "array",
      "items": {
        "type": "string"
      },
      "uniqueItems": true
    },
    "LoadBefore": {
      "type": "array",
      "items": {
        "type": "string"
      },
      "uniqueItems": true
    },
    "DLL": {
      "type": "string",
      "pattern": ".*\\.dll$"
    },
    "DLLEntryPoint": {
      "type": "string"
    },
    "Manifest": {
      "type": "array",
      "items": {
        "type": "object",
        "required": ["Type", "Path"],
        "properties": {
          "Type": {
            "type": "string"
          },
          "Path": {
            "type": "string"
          }
        }
      }
    }
  },
  "additionalProperties": false
}
````

---

## Validation Rules

* `Name` MUST be globally unique across mods
* `Version` MUST follow semantic versioning
* `DependsOn` MUST reference existing mods
* `ConflictsWith` MUST NOT overlap with dependencies
* `LoadAfter` / `LoadBefore` MUST NOT create cycles

---

## Semantic Constraints (Non-JSON)

These MUST be validated in tooling:

* No circular dependency graphs
* DLL must exist if declared
* Entry point must resolve to a valid method
* Manifest paths must exist on disk

---

## Example (Valid)

```json
{
  "Name": "MyWeaponPack",
  "Version": "1.2.0",
  "Author": "ModAuthor",
  "DependsOn": ["CAB"],
  "LoadAfter": ["CoreMod"],
  "DLL": "MyWeaponPack.dll",
  "DLLEntryPoint": "MyWeaponPack.Init"
}
```

---

## Anti-Patterns

* Missing Version ❌
* Using spaces in Name ❌
* Circular dependencies ❌
* Referencing non-existent mods ❌
* Including unused DLL ❌

---

## Tooling Recommendation

* Validate schema via CI (AJV or similar)
* Add runtime validation before mod load
* Fail fast on schema violations
