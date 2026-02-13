# WhenNot

**WhenNot** is a [conditional directive](../../Concepts/ConditionalDirective.md) that affects control flow. It's a wildcard pattern matcher that checks if a given string doesn't match a given pattern and if so, enters the directive body. Control flow remains unaffected if the pattern matches.

---
### Overloads

#### `WhenNot(Expression: String, Pattern: String) { ... }`

Evaluates the given Expression against Pattern. If it doesn't match, the directive body is executed; otherwise, control flow continues unchanged.  
Pattern matching supports '*' as a wildcard matching zero or more characters. Matching is case-sensitive.

| Parameter  | Type   | Description                                  |
|------------|--------|----------------------------------------------|
| Expression | String | The expression to mach the pattern to.       |
| Pattern    | String | The pattern to match the expression against. |

**Body:**  
The body of the WhenNot directive executes normally if the pattern doesn't match. No special runtime context is introduced.

**Example:**

```plaintext
DeclModule("Renderer")
{
    OutputType = "DynamicLibrary";

    # If Expression resolves to any of these for example:
    #   Dist-x64
    #   Dist-x86
    #   Development-x86
    # ... the WhenNot black will be executed since the pattern doesn't match.
    #
    # If Expression rather resolves to any of these for example:
    #   Development-x64
    #   Debug-x64
    #   Dev-ARM64
    # The block won't be run as the pattern matches.
    WhenNot("{Configuration}-{Architecture}", "De*-*64")
    {
        DefineGlobalMacro("DYSTOPIC_CONFIG");
    };
};
```
