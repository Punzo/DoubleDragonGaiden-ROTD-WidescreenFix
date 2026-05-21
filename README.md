# WidescreenFix for Double Dragon Gaiden: ROTD

>A BepInEx plugin to fix UI scaling and enable proper 32:9 ultrawide support for Double Dragon Gaiden: Rise of the Dragons.

![Description](https://github.com/Punzo/DoubleDragonGaiden-ROTD-WidescreenFix/raw/main/Screenshot.png)

## Features
- UI scaling for ultrawide (tested only on 32:9)
- No more zoomed-in gameplay or unusable menus
- Open source and easy to customize

## Installation
1. **Install [BepInEx 5.x](https://github.com/BepInEx/BepInEx/releases)** for your game (place all BepInEx files in the same folder as the game EXE).
2. Build or download `WidescreenFix.dll`.
3. Place `WidescreenFix.dll` in the `BepInEx/plugins/` folder inside your game directory.
4. Launch the game. The plugin will automatically patch the UI for ultrawide support.

## Building from Source
You need the .NET SDK (6.0+ or 10.0+) and [BepInEx](https://github.com/BepInEx/BepInEx) installed.

1. Open a terminal in this folder.
2. Run:
	```
	dotnet build -c Release
	```
3. The compiled DLL will be in `bin/Release/netstandard2.1/`.
4. Copy the DLL to your game's `BepInEx/plugins/` folder.

### Project References
Make sure your `.csproj` references these DLLs from your game install:
- `DoubleDragonGaiden_ROTD_Data/Managed/UnityEngine.dll`
- `DoubleDragonGaiden_ROTD_Data/Managed/UnityEngine.UI.dll`
- `DoubleDragonGaiden_ROTD_Data/Managed/UnityEngine.CoreModule.dll`
- `BepInEx/core/BepInEx.dll`

## License
MIT License. See [LICENSE](LICENSE) for details.

## Credits
- Created by Davide Punzo
- Powered by [BepInEx](https://github.com/BepInEx/BepInEx)

---
Pull requests and improvements welcome!
