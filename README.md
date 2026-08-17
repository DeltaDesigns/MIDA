# <img width="32" height="32" src="https://github.com/user-attachments/assets/694c5305-5e82-42d1-b341-862d5908bf1b" /> MIDA - the Marathon tool that does ~~(almost) everything~~ things.

## What is this?

* MIDA (**M**arathon **I**nformation and **D**ata **A**ssistant?) is a fork of [Charm](https://github.com/MontagueM/Charm/tree/delta/EOF) designed solely for Marathon
* MIDA is designed for 3D artists, content creators, content preservation, and nerds who like the inner workings of the Tiger Engine. It's main focus is on extracting 3D models.

> [!WARNING]
> * MIDA is still WIP, expect issues/bugs/crashes.
> * For developers: *A lot* of the schema structs are leftover from Charm for Destiny. Some parts of the code are probably also obsolete as certain things are no longer required for Marathon. I will clean these things up at some point.

> [!CAUTION]
> # Disclaimer
> * Before you go any further, understand that MIDA ***IS NOT a datamining tool!!***
> * While it can access many things in the game files, it's main purpose is focused towards **3D artists, content preservation and learning how the game works**!
> * Please ***DO NOT*** use this tool to spread leaks and spoilers or anything that may break Bungie's TOS. Don't ruin the experience for yourself and others. Uncover things the way they were intended!
> * Seeing this tool used for such acts WILL result in fewer public updates and the removal of certain features!

## How do I install and use it?
- Unlike Charm, MIDA will currently only support the latest version of Marathon, so you will need Marathon installed on Steam.
- You'll need [.NET 10.0 x64](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-10.0.3-windows-x64-installer) and [VC++ Redistributables](https://learn.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist?view=msvc-170#latest-microsoft-visual-c-redistributable-version) installed.
- Download the [latest release](https://github.com/DeltaDesigns/MIDA/releases/latest), extract the entire archive, and run MIDA.exe.
- You will be prompted to set the games packages path and MIDA's export path before you can continue.
    - For example: `C:\Program Files\Steam\steamapps\common\Marathon\packages` for the game, `G:\MIDA Output` for the output

## Reporting issues
If you experience any issues, bugs, or crashes, feel free to create an issue in this repository or in the Marathon Model Rips [Discord](https://discord.gg/yqE9AAZAw6) `#mida-tool-help` channel.
It would help greatly if you provide the latest crash log (`/Logs` folder) and steps to reproduce the issue.

## Known issues
- If the program doesn't open for you, MAKE SURE you have .NET 10 installed (see above)
- Package Path Cache creation may get stuck in rare instances, simply restart the program.
- The Animated Background may cause startup crashes for some people, set "AnimatedBackground" to false in your config.json file if this the case.
- Textures will not export if the export path contains a period or a special character.
- UI elements may not scale correctly for any resolution other than 1080p.
- Steam updates can sometimes fail to remove old package files which can/will cause crashes.
    - A complete uninstall/reinstall of the game is the easiest solution.

> [!TIP]
> ## Some tips and tricks
> * Middle click tabs to close them.
> * In a packages view, you can type in any hash and it will take you to it. No need to look through all the packages.
> * If you already have the hash of an Entity (Dynamic), you can press CTRL+D while on the Main Menu to enter 'Dev' view. Paste the hash into the box and press enter. It will open in a viewer and be exported.

## Blender
- Use the [Blender Importer addon](https://github.com/DeltaDesigns/d2-map-importer-addon) to simplify and automate importing maps and models into Blender.

## License
The MIDA source code is licensed under GPLv3. All other used code and DLLs are subject to their own licenses.
Just don't misuse the code. Respect developers.
