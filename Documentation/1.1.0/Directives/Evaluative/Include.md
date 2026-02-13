# Include

**Include** is a directive that lexes and parses another **description file** and introduces its **abstract syntax tree** into the [global scope](../../Scopes/GlobalScope.md). The runtime loads the target file and merges its AST into the global scope of the current description file.  
As mentioned above, inclusion is not textual but structural. Unlike a traditional textual include, the parser fully interprets the included file into an AST, allowing references and definitions to behave as if they were written in the including file.  
Multiple invocations on the same description file is permitted, however circular inclusions will raise a runtime error. A circular include is where a file includes another and that file either directly or indirectly includes the first file that included it.

---
### Overloads

#### `Include(Path: String)`

Includes the file from the given **Path**. It must conform to the following standard:
- If the given path leads to a regular file, it must end in **.mqmod**.
- If the given path leads to a directory, it must contain a **.mqmod** file whose name matches the directory name.
- If the given path leads to another kind of filesystem entry or doesn't lead to any entry at all, a runtime error will be raised.

| Parameter | Type   | Description                                         |
|-----------|--------|-----------------------------------------------------|
| Path      | String | The direct or eventual path to the file to include. |

**Example:**

```plaintext
DeclRoot("GameEngine")
{
    Modules    = [ "Engine-Renderer", "Engine-Physics", "Engine-AI", "Engine-Client" ];
    MainModule = "Engine-Client";
};
Include("Renderer");                     # Includes "Renderer/Renderer.mqmod".
Include("Physics");                      # Includes "Physics/Physics.mqmod".
Include("AI/AI-Build.mqmod");            # Includes "AI/AI-Build.mqmod".
Include("{Root.Path}/Templates/Client"); # Includes "{Root.Path}/Templates/Client/Client.mqmod".
```
