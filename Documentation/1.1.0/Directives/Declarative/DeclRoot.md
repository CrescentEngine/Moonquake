# DeclRoot

**DeclRoot** is a directive that introduces a new [Root](../../Constructs/Root.md) construct into the [Global Scope](../../Scopes/GlobalScope.md).  
Roots do not support inheritance and therefore each Root is created from the same blank template with certain defaults.  

---
### Overloads

#### `DeclRoot(Name: String) { ... }`

Creates a new Root with the specified **Name**.

| Parameter | Type   | Description                         |
|-----------|--------|-------------------------------------|
| Name      | String | The name to assign to the new Root. |

**Body:**  
The directive body is a [Root Scope](../../Scopes/RootScope.md) and is interpreted in that context.

**Example:**

```plaintext
DeclRoot("StrategyGame")
{
    # Any valid statement in a Root Scope, such as assigning to the field 'Configurations'.
};
```
