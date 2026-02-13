# When

**When** is a [conditional directive](../../Concepts/ConditionalDirective.md) that affects control flow. It's a wildcard pattern matcher that checks if a given string matches a given pattern and if so, enters the directive body. Control flow remains unaffected if the pattern doesn't match.

---
### Overloads

#### `When(Expression: String, Pattern: String) { ... }`

Evaluates the given Expression against Pattern. If it matches, the directive body is executed; otherwise, control flow continues unchanged.  
Pattern matching supports '*' as a wildcard matching zero or more characters. Matching is case-sensitive.

| Parameter  | Type   | Description                                  |
|------------|--------|----------------------------------------------|
| Expression | String | The expression to mach the pattern to.       |
| Pattern    | String | The pattern to match the expression against. |

**Body:**  
The body of the When directive executes normally if the pattern matches. No special runtime context is introduced.

**Example:**

```plaintext
DeclModule("WindowingLibrary")
{
    OutputType = "StaticLibrary";

    # If expression evaluates to any of these:
    #   Development-x64
    #   Debug-ARM64
    #   ... etc. anything that matches the pattern.
    # The block will be executed and DefineGlobalMacro will run.
    #
    # Some examples that make it won't run:
    #   Development-x86
    #   Debug-ARMV7
    #   Dist-x64
    When("{Configuration}-{Architecture}", "De*-*64")
    {
        DefineGlobalMacro("IDEAL_DIAGCONFIG");
    };
};
```
