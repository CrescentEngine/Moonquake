# Include

**Include** is a directive that lexes and parses another **description file** and introduces its *abstract syntax tree* into the [Global Scope](../../Scopes/GlobalScope.md). The runtime loads the target file and merges its AST into the global scope of the current **description file**.  
As mentioned above, inclusion is not textual but structural.

---
### Overloads

#### `Include(Filepath: String)`

Includes the file from the given **Filepath**. It must conform to the following standard:
1. If the given path leads to a regular file, it must end in **.mqmod**.
2. If the given path leads to a directory, it must contain a file in that directory named the same as the directory itself and must end in **.mqmod**.
3. If the given path leads to another kind of filesystem entry or doesn't lead to anything at all, the runtime will error out.

| Parameter | Type   | Description                                         |
|-----------|--------|-----------------------------------------------------|
| Name      | String | The direct or eventual path to the file to include. |

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
