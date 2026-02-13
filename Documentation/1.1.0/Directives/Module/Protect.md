# Protect

**Protect** is a special directive that's only valid within [module scopes](../../Scopes/ModuleScope.md). It marks a given [field](../../Concepts/Field.md) as immutable by the [dubious assignment operator](../../Operators/DubiousAssignment.md), which means the operation will result in no-op no matter what. This gives the module the power to enforce that a [schema](../../Constructs/Schema.md) cannot override a field that's in its default state (either never assigned to or never assigned since the last [unassignment](../../Operators/Unassignment.md)).  
Multiple calls on the same field have no effect; only the first invocation counts.

---
### Overloads

#### `Protect(FieldName: String)`

Protects the field with the given name.

| Parameter | Type   | Description                           |
|-----------|--------|---------------------------------------|
| FieldName | String | The name of the field to protect.     |

**Example:**

```plaintext
DeclSchema("Plugin")
{
    OutputType = "DynamicLibrary";
    Linkages += "EnginePluginService";
    Defer()
    {
        OutputPath ?= "{Root.Path}/Binaries/{Configuration}-{Platform}-{Architecture}/{Module.Name}";
        EnvironmentSDK ?= "1.0";
    };
};
DeclModule("AudioPlugin", "Plugin")
{
    Protect("EnvironmentSDK"); # We never assigned to EnvironmentSDK, so it is still in its default state.
                             # Deferred body runs later and the normal body of Schema didn't touch it before module body got executed.
};
# After module declaration, AudioPlugin.EnvironmentSDK is still in its default state.
# Normally the dubious assignment in the Defer() body would set it to 1.0 since the field was never assigned in the module body.
# However since we protected the EnvironmentSDK field, it stayed as is.
# OutputPath is also dubiously assigned and it will be assigned because we didn't protect it nor did we assign it in the Module.
```
