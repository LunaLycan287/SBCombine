# Streamer.bot Combine
This tool allows you to easily compile a project with multiple source files to one source file you can use in Streamer.bot.

## Command Line Options
| Parameter Short | Parameter Long | Default                                                    | Description                                                                                                                            |
|-----------------|----------------|------------------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------|
| -d              | --directory    | Current Directory                                          | Specify working directory.                                                                                                             |
| -e              | --exclude      | `**\obj\*`, `**\bin\*` and `**\Properties\AssemblyInfo.cs` | Exclude files from the list using glob pattern.                                                                                        |
| -i              | --include      | `**\*.cs`                                                  | Include files from the list using glob pattern. If this is not set all *.cs files in the directory and any sub-directory will be used. |
| -m              | --main         | First file found                                           | Main file to put at the top.                                                                                                           |
| -o              | --output       | `*-Merged.sbcs`                                            | Sets the output filename. '*' will be replaced with the main file name.                                                                |
| -u              | -usings        | true                                                       | If common implicit usings should be added if dependent libraries are found.                                                            |

## Examples

### Example One
```bash
SBYTMD
│   Player.cs
│   StateResponse.cs
│   Video.cs
|   _Socket.cs
│   _Socket-Merged.sbcs
```
```
SBCombine.exe -m "_Socket.cs"
```
This basic example:
- Is run in SBYTMD
- Has main file set to: `-m "_Socket.cs"`
- Results in an out file: `_Socket-Merged.sbcs`

### Example Two
```bash
SBYTMD
│   SBYTMD.sln
├───SBYTMD-Setup
├───SBYTMD-GetInfo
├───SBYTMD-Api
│   │   Player.cs
│   │   SBYTMD-Api.csproj
│   │   StateResponse.cs
│   │   Video.cs
└───SBYTMD-Socket
    │   SBYTMD-Socket.csproj
    │   _Socket-Merged.sbcs
    |   _Socket.cs
```
```
SBCombine.exe -i "SBYTMD-Api/*.cs" "SBYTMD-Socket/*.cs" -m "SBYTMD-Socket/_Socket.cs" -o "SBYTMD-Socket/*-Merged.sbcs"
```
This example:
- Is run in the Directory `SBYTMD`.
- Includes all .cs files in SBYTMD-Api : `-i "SBYTMD-Api/*.cs"`
- Includes all .cs files in SBYTMD-Socket: `-i [...] "SBYTMD-Socket/*.cs"`
- Has the main file set to: `-m "SBYTMD-Socket/_Socket.cs"`
- Has the out file in a sub directory: `-o "SBYTMD-Socket/*-Merged.sbcs"`

## Developing in 3rd Party IDE
When developing in 3rd Party IDE like vscode, Visual Studio or Rider you will need to keep in mind a few specifics.

### Creating a project
Keep in mind that Streamer.Bot code is mostly limited to `4.7.2.`, so when creating a new Project select this framework.  
I suggest using creating a class library project, since you will have no main entry point.  
SBCombine.exe can then be called as an external Tool:
- Rider:
  - Run -> Edit Configurations
  - "+" symbol
  - Native Executable
- Visual Studio
  - Tools -> External Tools
  - Add
 
### CPH Access
To have access to `CPH` add the following to your code inside `CPHInline`:
```csharp
public class CPHInline {
    private Streamer.bot.Plugin.Interface.IInlineInvokeProxy CPH;
}
```
  
When passing it as a parameter make sure to include it without full path:
```csharp
using Streamer.bot.Plugin.Interface;

public void SaveToSB(IInlineInvokeProxy cph) {
    cph.SetGlobalVar(VarSong, Video.Title);
}
```

### Namespaces inside project
This tool does not recognize namespaces created inside this project and does not filter them from using.  
Please always access local namespaces directly.

⚠️ Incorrect:
```csharp
using SBYTMD;

public class CPHInline {
    private StateResponse lastState = new StateResponse();
}

namespace SBYTMD {
    class StateResponse {[...]}
}
```
✅ Correct:
```csharp
public class CPHInline {
    private SBYTMD.StateResponse lastState = new SBYTMD.StateResponse();
}

namespace SBYTMD {
    class StateResponse {[...]}
}
```