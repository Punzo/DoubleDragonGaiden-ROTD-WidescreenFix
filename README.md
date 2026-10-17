# WidescreenFix for Double Dragon Gaiden: ROTD

>A BepInEx plugin to fix UI scaling and enable ultrawide support for Double Dragon Gaiden: Rise of the Dragons.

![Description](https://github.com/Punzo/DoubleDragonGaiden-ROTD-WidescreenFix/raw/main/Screenshot.png)

## Features
- UI scaling for ultrawide (tested only on 32:9)
- No more zoomed-in gameplay or unusable menus
- No black bars at the sides: the world is rendered at the full width of the screen
- No invisible walls at the old 16:9 screen edges: you can walk to the edge of your screen
- Open source and easy to customize

## Installation
1. Download `DoubleDragonGaiden-ROTD-WidescreenFix-x.y.z.zip` from the [Releases](https://github.com/Punzo/DoubleDragonGaiden-ROTD-WidescreenFix/releases) page.
2. Extract it into the game folder, the one that contains `DoubleDragonGaiden_ROTD.exe` (in Steam: right-click the game → *Manage* → *Browse local files*).
3. Launch the game. The plugin will automatically patch the game for ultrawide support.

The zip already contains [BepInEx 5.4.23.5](https://github.com/BepInEx/BepInEx) (x64). If you already have BepInEx 5 installed, you can instead copy only `WidescreenFix.dll` into `BepInEx/plugins/`.

To uninstall, delete `winhttp.dll`, `doorstop_config.ini`, `.doorstop_version` and the `BepInEx` folder from the game folder.

## Configuration
After the first launch, edit `BepInEx/config/com.yourname.widescreenfix.cfg`:

| Setting | Default | Description |
|---|---|---|
| `WidenWorld` | `true` | Render the world at the full screen width (removes the black bars at the sides). |
| `WideBounds` | `true` | Move the invisible side walls to the real screen edges (never past the room or arena bounds), and keep enemies in the side areas counted as on screen. |
| `StretchOverlays` | `true` | Stretch full-screen fades, flashes and cutscene letterbox bars to the full screen width. |
| `DebugDump` | `false` | Log cameras, canvases and raw images after each scene load (for development). |

## Building from Source
You need the .NET [SDK](https://dotnet.microsoft.com/en-us/download) (10.0+) and [BepInEx](https://github.com/BepInEx/BepInEx) installed.

1. Open a terminal in this folder.
2. Run:
	```
	dotnet build -c Release
	```
	If the game is not in the default Steam folder, add `-p:GameDir="D:\path\to\DoubleDragonGaiden_ROTD"`.
3. The compiled DLL will be in `bin/Release/netstandard2.1/`.
4. Copy the DLL to your game's `BepInEx/plugins/` folder.

To create the release zip (BepInEx + plugin, ready to extract), run:
```
powershell -ExecutionPolicy Bypass -File package.ps1
```
It builds the plugin, downloads the pinned BepInEx version into `.cache/`, and writes `release/DoubleDragonGaiden-ROTD-WidescreenFix-<version>.zip` and `release/WidescreenFix.dll`.

### Project References
The `.csproj` references these DLLs from your game install:
- `DoubleDragonGaiden_ROTD_Data/Managed/UnityEngine.dll`, `UnityEngine.CoreModule.dll`, `UnityEngine.UIModule.dll`, `UnityEngine.UI.dll`
- `DoubleDragonGaiden_ROTD_Data/Managed/Assembly-CSharp.dll`
- `BepInEx/core/BepInEx.dll`, `0Harmony.dll`

## License
MIT License. See [LICENSE](LICENSE) for details.

## Credits
- Created by Davide Punzo
- Powered by [BepInEx](https://github.com/BepInEx/BepInEx)

---
Pull requests and improvements welcome!
