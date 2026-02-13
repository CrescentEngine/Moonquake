# DeclModule

**DeclModule** is a directive that introduces a new [Module](../../Constructs/Root.md) construct into the [Global Scope](../../Scopes/GlobalScope.md).  
Modules are the building blocks of [Roots](../../Constructs/Root.md). Moonquake will build each module of a Root if a build order is ordered.
Modules do not support inheritance from modules, however they can be templated from [Schemas](../../Constructs/Schemas.md) which will bring its defaults and apply any deferred functionality.

---
### Overloads

#### `DeclModule(Name: String) { ... }`

Creates a new Module with the specified **Name**.

| Parameter | Type   | Description                           |
|-----------|--------|---------------------------------------|
| Name      | String | The name to assign to the new Module. |

**Body:**  
The directive body is a [Module Scope](../../Scopes/ModuleScope.md) and is interpreted in that context.

**Example:**

```plaintext
DeclModule("StrategyGame")
{
    # Any valid statement in a Module Scope, such as assigning to the field 'IntermediatePath'.
};
```
---

#### `DeclModule(Name: String, Schema: String) { ... }`

Creates a new Module with the specified **Name**, templated from the given **Schema**.

| Parameter | Type   | Description                                  |
|-----------|--------|----------------------------------------------|
| Name      | String | The name to assign to the new Module.        |
| Schema    | String | The name of the Schema to use as a template. |

**Body:**  
The directive body is a [Module Scope](../../Scopes/ModuleScope.md) and is interpreted in that context.
The order of execution is as follows:
1. Every statement (except Defer() directive statements) of the Schema's body is run first.
2. The module body is run.
3. If provided, body of the Defer() directive of the Schema is run.

**Example:**

```plaintext
DeclModule("StrategyGame", "GameSchema")
{
    # Any valid statement in a Module Scope, such as assigning to the field 'OutputType',
    # or calling the Protect() directive.

    # Let's demonstrate the order of execution.
    # Before we even arrive here, the body of Schema was run and all changes were applied to this module.
    # So if GameSchema did 'LanguageStandard = "Cpp20"', this module has that state now.

    # First statement within this module's body, as stated above, schema body was already run.
    Path = "C:/Dev/StrategyGame";

    # This is where our implementation of the module ends as we have no more statements.
    # After this, if the schema we used, 'GameSchema' in this case, had a Defer() directive statement
    # in it, the body of that directive will now be executed. If not, this phase is skipped.
    # After that, execution will exit from this scope.
};
```
