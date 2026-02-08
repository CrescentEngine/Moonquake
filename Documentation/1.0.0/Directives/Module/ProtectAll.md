# ProtectAll

**ProtectAll** is a special directive that's only valid within [module scopes](../../Scopes/ModuleScope.md). It marks all [fields](../../Concepts/Field.md) as immutable by the [dubious assignment operator](../../Operators/DubiousAssignment.md), which means the operation will result in no-op no matter what. This gives the module the power to enforce that a [schema](../../Constructs/Schema.md) cannot mutate the module in its [deferred scope](../../Scopes/DeferredScope.md) at all.
Multiple calls to this directive have no effect; only the first invocation counts.

---
### Overloads

#### `ProtectAll()`

Protects every field of the module, equivalent to calling ```Protect()``` on each field individually.

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
        IncludePaths ?= [ "EvilDir", "Vendor/EvilLib" ];
    };
};
DeclModule("AudioPlugin", "Plugin")
{
    ProtectAll();
};
# After module declaration, AudioPlugin.OutputPath, AudioPlugin.EnvironmentSDK and AudioPlugin.IncludePaths are still in their default states.
# Normally the dubious assignments in the Defer() body would set them to their respective values as written since the field was never assigned in the module body.
# However since we protected all fields of the module, every dubious assignment resulted in no-op and the fields stayed as is.
```
