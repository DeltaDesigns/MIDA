> [!CAUTION]
> # !!! MOST THINGS WILL ***NOT WORK*** IN THE PROGRAMS CURRENT STATE !!!

# MIDA - the Marathon tool that does (almost) everything
![mida logo idk](https://github.com/user-attachments/assets/dd168342-19ca-4bed-b8fd-9c182abf62f8)

## What is this?

MIDA is a fork of [Charm](https://github.com/MontagueM/Charm/tree/delta/TFS) designed soley for Marathon

The tool focuses on providing as much access to the information in the game files as possible, **ideal for artists and content preservation**.
> [!CAUTION]
> # Disclaimer
> * Before you go any further, understand that MIDA ***IS NOT*** a datamining tool! While it can access many things in the game files, it's main purpose is focused towards **3D artists, content preservation and learning how the game works**!
> * Please **DO NOT** use this tool to spread leaks and spoilers or anything that may break Bungie's TOS. Don't ruin the experience for yourself and others. Uncover things the way they were intended!
> * Seeing this tool used for such acts can and will result in fewer and fewer public updates and releases. I enjoy maintaining and updating this for others, don't be the one to ruin it.

## How do I install and use it?

You'll first need at least one game installation.
MIDA currently supports:

| Version | Description              | Where           |  Main manifest id   | Language manifest id |
|---------|--------------------------|-----------------|---------------------|----------------------|
| 0.0.0.0 | Marathon Closed Alpha | Steam, if you got lucky      |                     |                      |

- You'll need [.NET 8.0 x64](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-8.0.15-windows-x64-installer) and [VC++ Redistributables](https://learn.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist?view=msvc-170#latest-microsoft-visual-c-redistributable-version) installed.

> [!TIP]
> ## Some tips and tricks
> * Middle click tabs to close them.
> * In a packages view, you can type in any hash and it will take you to it. No need to look through all the packages.
> * If you already have the hash of an Entity (Dynamic), you can press CTRL+D to enter 'Dev' view. Paste the hash into the box and press enter. It will open in a viewer and be exported

## Reporting issues

If you experience any issue, you can register an issue in this repository. If the program has crashed, it is extremely valuable to provide the charm.log file.

## Known issues
- Textures will not export if the export path contains a period
- UI elements do not scale correctly for any resolution other than 1080p

## Blender
- Use the [Blender Importer addon](https://github.com/DeltaDesigns/d2-map-importer-addon) to simplify and automate importing maps and models into Blender.

## License

The MIDA source code is licensed under GPLv3. All other used code and DLLs are subject to their own licenses.
