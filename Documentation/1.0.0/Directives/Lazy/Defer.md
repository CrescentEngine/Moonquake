# Defer

**Defer** is a special directive that's only valid within [Schema Scopes](../../Scopes/SchemaScope.md). The runtime records its body for deferred execution during [module order of evaluation, stage 3](../../Concept/ModuleOrderOfEvaluation.md), by [Modules](../../Constructs/Module.md) that are instantiated (templated) from its schema.

---
### Overloads

#### `Defer() { ... }`

Declares the deferred execution stage of this Schema.

**Body:**  
The directive body is a [Deferred Scope](../../Scopes/DeferredScope.md) and is interpreted in that context. Deferred scopes are heavily restricted on operations that mutate module state, they can only use [dubious assignments](../../Operators/DubiousAssignment.md) and can only call [conditional directives](../../Concepts/ConditionalDirective.md).

**Example:**

```plaintext
DeclSchema("Plugin")
{
    Linkages += "EnginePluginService";
    Defer()
    {
        OutputPath ?= "{Root.Path}/Binaries/{Configuration}-{Platform}-{Architecture}"; # Modifying a module field, we can only use dubious assignment.
        When("{Platform}", "Win*") # This is okay to call because When is a conditional directive.
        {
            EnvironmentSDK ?= "Latest"; # ... can still only do dubious assignment.
        };

        # NOT OKAY! Cannot use regular assignment on a module field within a deferred scope.
        # Path = "{Root.Path}/Random";
        # Not okay as well. Field appendment to module fields is illegal just like regular assignment.
        # Linkages += "SuspiciousProject";
    };
};
```
