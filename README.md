# UnityImmersifyPlayer
Within the Immersify.eu project we developed a Unity3D wrapper for the Spin-Digital HEVC decoder and prepared everything to mix high resolution videos with real-time rendering and virtual reality.

## How to get a Immersify Player License
Find out how to create your Immersify License [here](SpinDigitalLicense.md)

## Working in the Editor:
To use the license in the Unity3D editor, the following file has to be put inside the Unity3D installation directory (e.g. C:\Program Files\Unity\Editor) and additionally to the Unity Project's Plugin folder (relative path is Assets\Immersify\Plugins\x86_64):
-	lservrc

All required DLLs are already placed in the projects plugin directory (Assets\Immersify\Plugins\x86_64).


## Running a Build:
To run a build, the following files have to be placed beside the executable (same directory):
-	lservrc
-	lsnnet64.dll

All other DLLs can be placed as usual in <ProjectName>_Data\Plugins. They are copied to this directory automatically when creating a build. It is fine, if the lsnnet64.dll is existing in the _Data\Plugins additionally (placed there during the build process), because it is ignored at this location.

## Project License
The source and the assets are provided without any warranty under the MIT license. (Please see LICENSE for details.) Third party parts are licensed under their own license.
The Unity project uses files from third parties as the "TextMesh Pro" and the "Oculus" Plugin.

## Disclaimer
The IMMERSIFY project has received funding from the European Union’s Horizon 2020 research and innovation programme under grant agreement No 762079. The IMMERSIFY project does not receive any external funding for the creation and maintenance of this project from advertisers or any other commercial interest.