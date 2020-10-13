# 👀 VRCEyeTracking

Provides real eye tracking in VRChat via the HTC Vive Pro Eye's eye tracking SDK. This MelonLoader mod modifies Avatar V3 Parameters according to data returned by the HMD's Eye Trackers. 

## 💾 Installation

Installation of the mod may be slightly different to most MelonLoader mods due to SRanipal's requirement for additional dependency DLLs.
**The DLLs included in the "Extras" folder of the zip need to be in the same folder as VRChat.exe**<br>
As standard, the main "EyeTrack.dll" file will need to be put in your Mods folder

## 🛠 Avatar Setup

For this mod to work, you'll need to be using an avatar with the correct parameters. The system is designed to control your avatar's eyes via simple blend states but what the parameters control is completely up to you. The mod searches for the following float parameters to modify:
- EyesX
- EyesY
- LeftEyeLid
- RightEyeLid
- EyesWiden
- EyesDilation

It's not required to use all of these parameters and, similar to the setup of parameters with Unity Animation Controllers, these are all case-sensitive.<br>
**Support for individual Eye Tracking is Coming Soon**

## 📜 Disclaimer

As with all VRChat mods, modifying the game client can result in account termination, be it temporary or permanent. While this mod doesn't ruin the experience for others, using it may still be a bannable offence.<br>
**USE AT YOUR OWN RISK**. I will not be held responsible for any punishments you may recieve for using this mod.

## 👋 Credits

* [HerpDerpinstine/MelonLoader](https://github.com/HerpDerpinstine/MelonLoader)
* [VIVE](https://www.vive.com/) for the SRAnipal SDK and their continuous support! ❤
