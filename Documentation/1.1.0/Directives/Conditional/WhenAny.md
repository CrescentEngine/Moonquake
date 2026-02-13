# WhenAny

**WhenAny** is a [conditional directive](../../Concepts/ConditionalDirective.md) that affects control flow. It's a wildcard pattern matcher that checks if a given string matches any of the provided patterns and if so, enters the directive body. Control flow remains unaffected if the pattern doesn't match.

---
### Overloads

#### `WhenAny(Expression: String, Patterns: Array) { ... }`

Evaluates the given Expression against given Patterns. If any matches, the directive body is executed; otherwise, control flow continues unchanged.  
Pattern matching supports '*' as a wildcard matching zero or more characters. Matching is case-sensitive.

| Parameter  | Type   | Description                                  |
|------------|--------|----------------------------------------------|
| Expression | String | The expression to mach the pattern to.       |
| Patterns   | Array  | Patterns to match the expression against.    |

**Body:**  
The body of the WhenAny directive executes normally if the pattern matches. No special runtime context is introduced.

**Example:**

```plaintext
DeclModule("WindowingLibrary")
{
    OutputType = "StaticLibrary";

    # You could think of this as: if ((Platform == "Windows" && Architecture == "x64") || (Configuration == "Development" && Architecture == "x64"))
    WhenAny("{Configuration}-{Platform}-{Architecture}", [ "*-Windows-x64", "Development-*-x64" ])
    {
        DefineGlobalMacro("NICHE_CONFIG_DETECTED_OR_SOMETHING");
    };
};
```
