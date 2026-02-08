# DeclSchema

**DeclSchema** is a directive that introduces a new [Schema](../../Constructs/Schema.md) construct into the [Global Scope](../../Scopes/GlobalScope.md).  
Schemas are blueprints to [Modules](../../Constructs/Module.md). They can mutate any field and the module will inherit those fields in the same state.  
Schemas do not support inheritance and each schema must be built from scratch. This is to avoid overcomplicated lineage of schemas blurring what's happening.  

---
### Overloads

#### `DeclSchema(Name: String) { ... }`

Creates a new Schema with the specified **Name**.

| Parameter | Type   | Description                           |
|-----------|--------|---------------------------------------|
| Name      | String | The name to assign to the new Schema. |

**Body:**  
The directive body is a [Schema Scope](../../Scopes/SchemaScope.md) and is interpreted in that context.

**Example:**

```plaintext
DeclSchema("Plugin")
{
    When("{Configuration}", "Dev*")
    {
        RuntimeLibraries = "UseDebug";
        bDebugSymbols = "Yes";
    };
};
DeclModule("AudioMgmt", "Plugin")
{
    # If current build configuration is Development, AudioMgmt currently has RuntimeLibraries and bDebugSymbols set as described in the schema.
    # The normal schema body is run first before the module body, so it inherits the state of those fields.
};
```
